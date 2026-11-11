using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.Properties;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        /// <inheritdoc cref="Fuse{TSource, Inner}(in ValueEnumerable{TSource, Inner})"/>
        public static ValueEnumerable<TSource, Nodes.Options.Node<TSource, SystemIEnumerable.Node<TSource>>, Nodes.Options.Pull<TSource, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>> Fuse<TSource>(this IEnumerable<TSource> source)
            => source.OfEnumerable().Fuse();

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) =>
            source switch
            {
                ICollection<TSource> c => c.Contains(value),
                // a Cistern chain (a Concat of collections) answers from its parts before anything is pushed
                _ => source.OfEnumerable().Contains(value)
            };


        public static bool Any<TSource>(this IEnumerable<TSource> source)
            => source switch
            {
                System.Collections.ICollection c => c.Count > 0,
                ICollection<TSource> c => c.Count > 0,
                IReadOnlyCollection<TSource> c => c.Count > 0,
                _ => source.OfEnumerable().Any()
            };

        public static TSource First<TSource>(this IEnumerable<TSource> source) =>
            source.OfEnumerable().First();

        public static TSource? FirstOrDefault<TSource>(this IEnumerable<TSource> source) =>
            source.OfEnumerable().FirstOrDefault();

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) =>
            source.OfEnumerable().FirstOrDefault(defaultValue);



        // an IList<T> is decided from its count with at most one indexer read, as System.Linq decides one
        public static TSource Single<TSource>(this IEnumerable<TSource> source) =>
            source switch
            {
                IList<TSource> list => list.Count switch
                {
                    1 => list[0],
                    0 => throw new InvalidOperationException(SR.NoElements),
                    _ => throw new InvalidOperationException(SR.MoreThanOneElement),
                },
                _ => SystemIEnumerable.ExecutePush<TSource, TSource, Single<TSource>>(source ?? throw new ArgumentNullException(nameof(source)), new Single<TSource>()),
            };

        public static TSource? SingleOrDefault<TSource>(this IEnumerable<TSource> source) =>
            source switch
            {
                IList<TSource> list => list.Count switch
                {
                    1 => list[0],
                    0 => default,
                    _ => throw new InvalidOperationException(SR.MoreThanOneElement),
                },
                _ => SystemIEnumerable.ExecutePush<TSource, TSource?, SingleOrDefault<TSource>>(source ?? throw new ArgumentNullException(nameof(source)), new SingleOrDefault<TSource>()),
            };

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) =>
            source switch
            {
                IList<TSource> list => list.Count switch
                {
                    1 => list[0],
                    0 => defaultValue,
                    _ => throw new InvalidOperationException(SR.MoreThanOneElement),
                },
                _ => SystemIEnumerable.ExecutePush<TSource, TSource, SingleOrDefault<TSource>>(source ?? throw new ArgumentNullException(nameof(source)), new SingleOrDefault<TSource>(defaultValue)),
            };

        // a count known without enumerating sizes the dictionary, and an empty source is answered before anything is
        // pushed, as System.Linq answers one
        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            int? knownCount =
                source.TryGetNonEnumeratedCount(out var count)
                    ? count
                    : null;

            var dictionary =
                knownCount switch
                {
                    0 => new Dictionary<TKey, TSource>(comparer),
                    _ => SystemIEnumerable.ExecutePush<TSource, Dictionary<TKey, TSource>, ToDictionary<TSource, TKey>>(source, new ToDictionary<TSource, TKey>(keySelector, comparer, knownCount ?? 0)),
                };

            return dictionary;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            int? knownCount =
                source.TryGetNonEnumeratedCount(out var count)
                    ? count
                    : null;

            var dictionary =
                knownCount switch
                {
                    0 => new Dictionary<TKey, TValue>(comparer),
                    _ => SystemIEnumerable.ExecutePush<TSource, Dictionary<TKey, TValue>, ToDictionary<TSource, TKey, TValue>>(source, new ToDictionary<TSource, TKey, TValue>(keySelector, elementSelector, comparer, knownCount ?? 0)),
                };

            return dictionary;
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source) =>
            source.OfEnumerable().Last();

        public static TSource? LastOrDefault<TSource>(this IEnumerable<TSource> source) =>
            source.OfEnumerable().LastOrDefault();

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) =>
            source.OfEnumerable().LastOrDefault(defaultValue);

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue) =>
            source.OfEnumerable().LastOrDefault(predicate, defaultValue);

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) =>
            SequenceEqual(first, second, null);

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            // two sequences neither of which is a Cistern chain are compared as System.Linq compares them (counts first,
            // collections by their indexers, spans vectorised), which a node over each has no better way to do
            if (!IsCisternChain(first) && !IsCisternChain(second))
                return System.Linq.Enumerable.SequenceEqual(first, second, comparer);

            var equal = first.OfEnumerable().SequenceEqual(second, comparer);

            return equal;
        }

        // a chain met as an IEnumerable<T>, rather than a node that is a collection of its own (a lookup, a grouping)
        internal static bool IsCisternChain<T>(IEnumerable<T>? source) => source is INode and not ICollection<T>;

        public static T[] ToArray<T>(this IEnumerable<T> source)
        {
            return source switch
            {
                null => throw new ArgumentNullException(nameof(source)),
                ICollection<T> c => ICollectionToArray(c),
                INode or IReadOnlyCollection<T> or System.Collections.ICollection => source.OfEnumerable().ToArray(),
                _ => EnumeratorToArray(source),
            };

            // a source only its enumerator reads asks no request, each a test of its runtime type that over a reference
            // type is a dictionary lookup, and fills the builder in the builder's own loop
            static T[] EnumeratorToArray(IEnumerable<T> source)
            {
                var enumerator = source.GetEnumerator();

                // a wrapper can hand out a Cistern chain's own enumerator, which pushes what it can and disposes itself
                if (enumerator is Chain.PullEnumerator<T> pull)
                    return ErasedChain.PushRest<T, T[], ToArrayViaBuilderForward<T>>(pull, new ToArrayViaBuilderForward<T>());

                var builder = new Utils.SegmentedArrayBuilder<T>();
                try
                {
                    using (enumerator)
                        builder.AddRange(enumerator);

                    return builder.ToArray();
                }
                finally
                {
                    builder.Dispose();
                }
            }

            static T[] ICollectionToArray(ICollection<T> c)
            {
                var count = c.Count;
                if (count <= 0)
                    return Array.Empty<T>();

                var result = new T[count];
                c.CopyTo(result, 0);
                return result;
            }
        }

        public static List<T> ToList<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // a node that is also a collection (a lookup, a grouping) is copied by the list's constructor
            return source is INode and not ICollection<T>
                ? source.OfEnumerable().ToList()
                : new List<T>(source);
        }

        public static List<T> ToListUseStack<T>(this IEnumerable<T> source, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is ICollection<T>
                ? new List<T>(source)
                : source.OfEnumerable().ToListUseStack(maxStackItemCount, arrayPoolInfo);
        }

        public static List<T> ToListUsePool<T>(this IEnumerable<T> source, ArrayPool<T>? maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is ICollection<T>
                ? new List<T>(source)
                : source.OfEnumerable().ToListUsePool(maybeArrayPool, maybeCleanBuffers, viaPull);
        }


        public static int Count<T>(this IEnumerable<T> inner, bool ignorePotentialSideEffects = false) =>
            inner.OfEnumerable().Count(ignorePotentialSideEffects);

        public static long LongCount<T>(this IEnumerable<T> source, bool ignorePotentialSideEffects = false) =>
            source.OfEnumerable().LongCount(ignorePotentialSideEffects);

        public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count) =>
            source.OfEnumerable().TryGetNonEnumeratedCount(out count);

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return first.OfEnumerable().Concat(second.OfEnumerable());
        }

        // a chain built through IEnumerable<T> has lost its static type, so the node to extend is found at runtime
        public static ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemIEnumerable.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemIEnumerable.Node<T>, ErasedEnumerator<T, SystemIEnumerable.Node<T>>>> Append<T>(this IEnumerable<T> source, T element) =>
            source is ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemIEnumerable.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemIEnumerable.Node<T>, ErasedEnumerator<T, SystemIEnumerable.Node<T>>>> appended
                ? new (appended.Node.Append(element))
                : new (NodeImpl.Append<T, SystemIEnumerable.Node<T>>(ToNode(source), element));

        public static ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemIEnumerable.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemIEnumerable.Node<T>, ErasedEnumerator<T, SystemIEnumerable.Node<T>>>> Prepend<T>(this IEnumerable<T> source, T element) =>
            source is ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemIEnumerable.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemIEnumerable.Node<T>, ErasedEnumerator<T, SystemIEnumerable.Node<T>>>> prepended
                ? new (prepended.Node.Prepend(element))
                : new (NodeImpl.Prepend<T, SystemIEnumerable.Node<T>>(ToNode(source), element));

        public static ValueEnumerable<(T First, U Second), Nodes.Zip.Node<T, U, (T, U), SystemIEnumerable.Node<T>, SystemIEnumerable.Node<U>, ErasedEnumerator<U, SystemIEnumerable.Node<U>>, Nodes.Zip.PairSelector<T, U>>, Nodes.Zip.Pull<T, U, (T, U), SystemIEnumerable.Node<T>, SystemIEnumerable.Node<U>, ErasedEnumerator<U, SystemIEnumerable.Node<U>>, Nodes.Zip.PairSelector<T, U>, ErasedEnumerator<T, SystemIEnumerable.Node<T>>>> Zip<T, U>(this IEnumerable<T> first, IEnumerable<U> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return new (new (new (first), new (second), default));
        }

        public static ValueEnumerable<TResult, Nodes.Zip.Node<T, U, TResult, SystemIEnumerable.Node<T>, SystemIEnumerable.Node<U>, ErasedEnumerator<U, SystemIEnumerable.Node<U>>, Nodes.Zip.FuncSelector<T, U, TResult>>, Nodes.Zip.Pull<T, U, TResult, SystemIEnumerable.Node<T>, SystemIEnumerable.Node<U>, ErasedEnumerator<U, SystemIEnumerable.Node<U>>, Nodes.Zip.FuncSelector<T, U, TResult>, ErasedEnumerator<T, SystemIEnumerable.Node<T>>>> Zip<T, U, TResult>(this IEnumerable<T> first, IEnumerable<U> second, Func<T, U, TResult> resultSelector)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var firstNode = new SystemIEnumerable.Node<T>(first);
            var secondNode = new SystemIEnumerable.Node<U>(second);

            var zip = NodeImpl.Zip<T, U, TResult, SystemIEnumerable.Node<T>, SystemIEnumerable.Node<U>, ErasedEnumerator<U, SystemIEnumerable.Node<U>>>(in firstNode, in secondNode, resultSelector);

            return new (zip);
        }

        public static ValueEnumerable<(T First, U Second, V Third), Nodes.Zip.Node<(T, U), V, (T, U, V), Nodes.Zip.Node<T, U, (T, U), SystemIEnumerable.Node<T>, SystemIEnumerable.Node<U>, ErasedEnumerator<U, SystemIEnumerable.Node<U>>, Nodes.Zip.PairSelector<T, U>>, SystemIEnumerable.Node<V>, ErasedEnumerator<V, SystemIEnumerable.Node<V>>, Nodes.Zip.TripleSelector<T, U, V>>, Nodes.Zip.Pull<(T, U), V, (T, U, V), Nodes.Zip.Node<T, U, (T, U), SystemIEnumerable.Node<T>, SystemIEnumerable.Node<U>, ErasedEnumerator<U, SystemIEnumerable.Node<U>>, Nodes.Zip.PairSelector<T, U>>, SystemIEnumerable.Node<V>, ErasedEnumerator<V, SystemIEnumerable.Node<V>>, Nodes.Zip.TripleSelector<T, U, V>, Nodes.Zip.Pull<T, U, (T, U), SystemIEnumerable.Node<T>, SystemIEnumerable.Node<U>, ErasedEnumerator<U, SystemIEnumerable.Node<U>>, Nodes.Zip.PairSelector<T, U>, ErasedEnumerator<T, SystemIEnumerable.Node<T>>>>> Zip<T, U, V>(this IEnumerable<T> first, IEnumerable<U> second, IEnumerable<V> third)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (third == null)
                throw new ArgumentNullException(nameof(third));

            var firstNode = new SystemIEnumerable.Node<T>(first);
            var secondNode = new SystemIEnumerable.Node<U>(second);
            var thirdNode = new SystemIEnumerable.Node<V>(third);

            var zip = NodeImpl.Zip<T, U, V, SystemIEnumerable.Node<T>, SystemIEnumerable.Node<U>, SystemIEnumerable.Node<V>, ErasedEnumerator<U, SystemIEnumerable.Node<U>>, ErasedEnumerator<V, SystemIEnumerable.Node<V>>>(in firstNode, in secondNode, in thirdNode);

            return new (zip);
        }


        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) =>
            source switch
            {
                TSource[] a => a.OfArray().ElementAt(index),
                ImmutableArray<TSource> i => i.OfImmutableArray().ElementAt(index),
                List<TSource> l => l.OfList().ElementAt(index),
                // indexed in place, as System.Linq does, rather than asked through a Cistern node's generic virtual request (a grouping)
                IList<TSource> l => (uint)index < (uint)l.Count ? l[index] : throw new ArgumentOutOfRangeException(nameof(index)),
                var e => e.OfEnumerable().ElementAt(index)
            };

        public static TSource? ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index) => source.OfEnumerable().ElementAtOrDefault(index);

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, Index index) => source.OfEnumerable().ElementAt(index);

        public static TSource? ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, Index index) => source.OfEnumerable().ElementAtOrDefault(index);

        public static ValueEnumerable<TResult, CisternCast.Node<TResult>, ErasedEnumerator<TResult, CisternCast.Node<TResult>>> Cast<TResult>(this System.Collections.IEnumerable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new(new CisternCast.Node<TResult>(source));
        }

        public static ValueEnumerable<TResult, CisternOfType.Node<TResult>, ErasedEnumerator<TResult, CisternOfType.Node<TResult>>> OfType<TResult>(this System.Collections.IEnumerable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new(new CisternOfType.Node<TResult>(source));
        }

        public static (U, V) Fork<T, U, V>(this IEnumerable<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v)
            => NodeImpl.Fork(new SystemIEnumerable.Node<T>(source), t2u, t2v);
        public static (U, V, W) Fork<T, U, V, W>(this IEnumerable<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, W> t2w)
            => NodeImpl.Fork(new SystemIEnumerable.Node<T>(source), t2u, t2v, t2w);

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, SystemIEnumerable.Node<TSource>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
        {
            if (first == null)
                throw new ArgumentNullException("first");

            return first.OfEnumerable().Except(second, comparer);
        }

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TKey, SystemIEnumerable.Node<TSource>, FuncToIFunc<TSource, TKey>>, Nodes.Except.Pull<TSource, TKey, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, FuncToIFunc<TSource, TKey>>> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            var exceptBy =
                first
                .OfEnumerable()
                .ExceptBy(second, keySelector, comparer);

            return exceptBy;
        }

        public static ValueEnumerable<TSource, Nodes.Intersect.Node<TSource, TSource, SystemIEnumerable.Node<TSource>, Nodes.Except.IdentityKey<TSource>>, Nodes.Intersect.Pull<TSource, TSource, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            var intersect =
                first
                .OfEnumerable()
                .Intersect(second, comparer);

            return intersect;
        }

        public static ValueEnumerable<TSource, Nodes.Intersect.Node<TSource, TKey, SystemIEnumerable.Node<TSource>, FuncToIFunc<TSource, TKey>>, Nodes.Intersect.Pull<TSource, TKey, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, FuncToIFunc<TSource, TKey>>> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            var intersectBy =
                first
                .OfEnumerable()
                .IntersectBy(second, keySelector, comparer);

            return intersectBy;
        }

        public static ValueEnumerable<TResult, Nodes.Join.Node<TOuter, TInner, TKey, TResult, SystemIEnumerable.Node<TOuter>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>, Nodes.Join.Pull<TOuter, TInner, TKey, TResult, SystemIEnumerable.Node<TOuter>, ErasedEnumerator<TOuter, SystemIEnumerable.Node<TOuter>>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var join =
                outer
                .OfEnumerable()
                .Join(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

            return join;
        }

        public static ValueEnumerable<(TOuter Outer, TInner Inner), Nodes.Join.Node<TOuter, TInner, TKey, (TOuter Outer, TInner Inner), SystemIEnumerable.Node<TOuter>, Nodes.Zip.PairSelector<TOuter, TInner>>, Nodes.Join.Pull<TOuter, TInner, TKey, (TOuter Outer, TInner Inner), SystemIEnumerable.Node<TOuter>, ErasedEnumerator<TOuter, SystemIEnumerable.Node<TOuter>>, Nodes.Zip.PairSelector<TOuter, TInner>>> Join<TOuter, TInner, TKey>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var join =
                outer
                .OfEnumerable()
                .Join(inner, outerKeySelector, innerKeySelector, comparer);

            return join;
        }

        public static ValueEnumerable<TResult, Nodes.LeftJoin.Node<TOuter, TInner, TKey, TResult, SystemIEnumerable.Node<TOuter>, SystemIEnumerable.Node<TInner>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>, Nodes.LeftJoin.Pull<TOuter, TInner, TKey, TResult, SystemIEnumerable.Node<TOuter>, ErasedEnumerator<TOuter, SystemIEnumerable.Node<TOuter>>, SystemIEnumerable.Node<TInner>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>> LeftJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner?, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var leftJoin =
                outer
                .OfEnumerable()
                .LeftJoin(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

            return leftJoin;
        }

        public static ValueEnumerable<(TOuter Outer, TInner? Inner), Nodes.LeftJoin.Node<TOuter, TInner, TKey, (TOuter Outer, TInner? Inner), SystemIEnumerable.Node<TOuter>, SystemIEnumerable.Node<TInner>, Nodes.LeftJoin.PairSelector<TOuter, TInner>>, Nodes.LeftJoin.Pull<TOuter, TInner, TKey, (TOuter Outer, TInner? Inner), SystemIEnumerable.Node<TOuter>, ErasedEnumerator<TOuter, SystemIEnumerable.Node<TOuter>>, SystemIEnumerable.Node<TInner>, Nodes.LeftJoin.PairSelector<TOuter, TInner>>> LeftJoin<TOuter, TInner, TKey>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var leftJoin =
                outer
                .OfEnumerable()
                .LeftJoin(inner, outerKeySelector, innerKeySelector, comparer);

            return leftJoin;
        }

        public static ValueEnumerable<TResult, Nodes.LeftJoin.Node<TInner, TOuter, TKey, TResult, SystemIEnumerable.Node<TInner>, SystemIEnumerable.Node<TOuter>, Nodes.RightJoin.FuncSelector<TInner, TOuter, TResult>>, Nodes.LeftJoin.Pull<TInner, TOuter, TKey, TResult, SystemIEnumerable.Node<TInner>, ErasedEnumerator<TInner, SystemIEnumerable.Node<TInner>>, SystemIEnumerable.Node<TOuter>, Nodes.RightJoin.FuncSelector<TInner, TOuter, TResult>>> RightJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter?, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var rightJoin =
                outer
                .OfEnumerable()
                .RightJoin(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

            return rightJoin;
        }

        public static ValueEnumerable<(TOuter? Outer, TInner Inner), Nodes.LeftJoin.Node<TInner, TOuter, TKey, (TOuter? Outer, TInner Inner), SystemIEnumerable.Node<TInner>, SystemIEnumerable.Node<TOuter>, Nodes.RightJoin.PairSelector<TInner, TOuter>>, Nodes.LeftJoin.Pull<TInner, TOuter, TKey, (TOuter? Outer, TInner Inner), SystemIEnumerable.Node<TInner>, ErasedEnumerator<TInner, SystemIEnumerable.Node<TInner>>, SystemIEnumerable.Node<TOuter>, Nodes.RightJoin.PairSelector<TInner, TOuter>>> RightJoin<TOuter, TInner, TKey>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var rightJoin =
                outer
                .OfEnumerable()
                .RightJoin(inner, outerKeySelector, innerKeySelector, comparer);

            return rightJoin;
        }

        public static ValueEnumerable<TResult, Nodes.FullJoin.Node<TOuter, TInner, TKey, TResult, SystemIEnumerable.Node<TOuter>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>, Nodes.FullJoin.Pull<TOuter, TInner, TKey, TResult, SystemIEnumerable.Node<TOuter>, ErasedEnumerator<TOuter, SystemIEnumerable.Node<TOuter>>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>> FullJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter?, TInner?, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var fullJoin =
                outer
                .OfEnumerable()
                .FullJoin(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

            return fullJoin;
        }

        public static ValueEnumerable<(TOuter? Outer, TInner? Inner), Nodes.FullJoin.Node<TOuter, TInner, TKey, (TOuter? Outer, TInner? Inner), SystemIEnumerable.Node<TOuter>, Nodes.FullJoin.PairSelector<TOuter, TInner>>, Nodes.FullJoin.Pull<TOuter, TInner, TKey, (TOuter? Outer, TInner? Inner), SystemIEnumerable.Node<TOuter>, ErasedEnumerator<TOuter, SystemIEnumerable.Node<TOuter>>, Nodes.FullJoin.PairSelector<TOuter, TInner>>> FullJoin<TOuter, TInner, TKey>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var fullJoin =
                outer
                .OfEnumerable()
                .FullJoin(inner, outerKeySelector, innerKeySelector, comparer);

            return fullJoin;
        }

        public static ValueEnumerable<TResult, Nodes.GroupJoin.Node<TOuter, TInner, TKey, TResult, SystemIEnumerable.Node<TOuter>>, Nodes.GroupJoin.Pull<TOuter, TInner, TKey, TResult, SystemIEnumerable.Node<TOuter>, ErasedEnumerator<TOuter, SystemIEnumerable.Node<TOuter>>>> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var groupJoin =
                outer
                .OfEnumerable()
                .GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

            return groupJoin;
        }

        public static ValueEnumerable<System.Linq.IGrouping<TOuter, TInner>, Nodes.GroupJoin.Node<TOuter, TInner, TKey, System.Linq.IGrouping<TOuter, TInner>, SystemIEnumerable.Node<TOuter>>, Nodes.GroupJoin.Pull<TOuter, TInner, TKey, System.Linq.IGrouping<TOuter, TInner>, SystemIEnumerable.Node<TOuter>, ErasedEnumerator<TOuter, SystemIEnumerable.Node<TOuter>>>> GroupJoin<TOuter, TInner, TKey>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));

            var groupJoin =
                outer
                .OfEnumerable()
                .GroupJoin(inner, outerKeySelector, innerKeySelector, comparer);

            return groupJoin;
        }

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>, Nodes.Except.IdentityKey<TSource>>> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            var union =
                first switch
                {
                    ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>, Nodes.Except.IdentityKey<TSource>>> unionOfEnumerables => unionOfEnumerables.Union(second, comparer),
                    _ when TryUnionOfTypedUnion(first, second, comparer, out var unionOfTyped) => unionOfTyped,
                    _ => first.OfEnumerable().Union(second, comparer),
                };

            return union;
        }

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TKey, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, FuncToIFunc<TSource, TKey>>, Nodes.Except.Pull<TSource, TKey, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>, FuncToIFunc<TSource, TKey>>> UnionBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            var unionBy =
                first
                .OfEnumerable()
                .UnionBy(second, keySelector, comparer);

            return unionBy;
        }

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, SystemIEnumerable.Node<TSource>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>> Distinct<TSource>(this IEnumerable<TSource> first, IEqualityComparer<TSource>? comparer = null)
            => first.OfEnumerable().Distinct(comparer);

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, Nodes.GroupBy.Node<TSource, TKey, SystemIEnumerable.Node<TSource>>, Nodes.GroupBy.Pull<TSource, TKey, SystemIEnumerable.Node<TSource>>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            => new (NodeImpl.GroupBy(ToNode(source), keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, Nodes.GroupBy.Node<TSource, TKey, TElement, SystemIEnumerable.Node<TSource>>, Nodes.GroupBy.Pull<TSource, TKey, TElement, SystemIEnumerable.Node<TSource>>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(ToNode(source), keySelector, elementSelector, comparer));

        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TResult, SystemIEnumerable.Node<TSource>>, Nodes.GroupByResult.Pull<TSource, TKey, TResult, SystemIEnumerable.Node<TSource>>> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(ToNode(source), keySelector, resultSelector, comparer));
        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TElement, TResult, SystemIEnumerable.Node<TSource>>, Nodes.GroupByResult.Pull<TSource, TKey, TElement, TResult, SystemIEnumerable.Node<TSource>>> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(ToNode(source), keySelector, elementSelector, resultSelector, comparer));

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source) => source;

        // an ordering System.Linq built holds its keys inside its own object, so only the interface can extend it
        public static System.Linq.IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this System.Linq.IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var ordered =
                source.CreateOrderedEnumerable(keySelector, comparer, descending: false);

            return ordered;
        }

        public static System.Linq.IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this System.Linq.IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var ordered =
                source.CreateOrderedEnumerable(keySelector, comparer, descending: true);

            return ordered;
        }
    }
}
