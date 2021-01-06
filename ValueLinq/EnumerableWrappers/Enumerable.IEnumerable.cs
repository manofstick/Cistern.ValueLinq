using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) =>
            source switch
            {
                ICollection<TSource> c => c.Contains(value),
                _ => EnumerableNode.FastEnumerateSwitch<TSource, bool, Contains<TSource>>(source, new Contains<TSource>(value))
            };


        public static bool Any<TSource>(this IEnumerable<TSource> source)
            => source switch
            {
                System.Collections.ICollection c => c.Count > 0,
                ICollection<TSource> c => c.Count > 0,
                IReadOnlyCollection<TSource> c => c.Count > 0,
                _ => source.OfEnumerable().Any()
            };



        public static TSource Last<TSource>(this IEnumerable<TSource> source) =>
            source.OfEnumerable().Last();

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) =>
            source.OfEnumerable().LastOrDefault();

        public static T[] ToArray<T>(this IEnumerable<T> source)
        {
            return source is ICollection<T> c
                ? ICollectionToArray(c)
                : source.OfEnumerable().ToArray();

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

            return source is INode
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

        public static List<T> ToListUsePool<T>(this IEnumerable<T> source, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is ICollection<T>
                ? new List<T>(source)
                : source.OfEnumerable().ToListUsePool(maybeArrayPool, maybeCleanBuffers, viaPull);
        }


        public static int Count<T>(this IEnumerable<T> inner, bool ignorePotentialSideEffects = false) =>
            inner.OfEnumerable().Count(ignorePotentialSideEffects);

        public static ValueEnumerable<TResult, SelectManyNode<TResult, EnumerableNode<TResult>, SelectNode<TSource, EnumerableNode<TResult>, EnumerableNode<TSource>>>> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new (new (new( new(source), source => new (selector(source)))));
        }
       public static ValueEnumerable<TResult, SelectManyNode<TResult, EnumerableNode<TResult>, SelectIdxNode<TSource, EnumerableNode<TResult>, EnumerableNode<TSource>>>> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(new(new(source), (source, idx) => new(selector(source, idx)))));
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TCollection, TResult, EnumerableNode<TCollection>, SelectNode<TSource, (TSource, EnumerableNode<TCollection>), EnumerableNode<TSource>>>> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (collectionSelector == null)
                throw new ArgumentNullException(nameof(collectionSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return new (new (new (new (source), src => (src, new(collectionSelector(src)))), resultSelector));
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TCollection, TResult, EnumerableNode<TCollection>, SelectIdxNode<TSource, (TSource, EnumerableNode<TCollection>), EnumerableNode<TSource>>>> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (collectionSelector == null)
                throw new ArgumentNullException(nameof(collectionSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return new(new(new(new(source), (src, idx) => (src, new(collectionSelector(src, idx)))), resultSelector));
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return first.OfEnumerable().Concat(second.OfEnumerable());
        }

        public static ValueEnumerable<(T First, U Second), ZipNode<T, U, EnumerableNode<T>, EnumerableNode<U>>> Zip<T, U>(this IEnumerable<T> first, IEnumerable<U> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return new (new (new (first), new (second)));
        }


        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) =>
            source.OfEnumerable().ElementAt(index);

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index) => source.OfEnumerable().ElementAtOrDefault(index);

        public static ValueEnumerable<TResult, CastNode<TResult>> Cast<TResult>(this System.Collections.IEnumerable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new(new CastNode<TResult>(source));
        }

        public static ValueEnumerable<TResult, OfTypeNode<TResult>> OfType<TResult>(this System.Collections.IEnumerable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new(new OfTypeNode<TResult>(source));
        }

        public static (U, V) Fork<T, U, V>(this IEnumerable<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v)
            => NodeImpl.Fork(new EnumerableNode<T>(source), t2u, t2v);
        public static (U, V, W) Fork<T, U, V, W>(this IEnumerable<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>>, W> t2w)
            => NodeImpl.Fork(new EnumerableNode<T>(source), t2u, t2v, t2w);

        public static ValueEnumerable<TSource, ExceptNode<TSource, EnumerableNode<TSource>>> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer = null)
        {
            if (first == null)
                throw new ArgumentNullException("first");

            return new(first.OfEnumerable().Except(second, comparer));
        }

        public static ValueEnumerable<TSource, ExceptNode<TSource, EnumerableNode<TSource>>> Distinct<TSource>(this IEnumerable<TSource> first, IEqualityComparer<TSource> comparer = null)
            => new (first.OfEnumerable().Distinct(comparer));

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, GroupByNode<TSource, TKey, EnumerableNode<TSource>>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
            => new (NodeImpl.GroupBy(ToNode(source), keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, GroupByNode<TSource, TKey, TElement, EnumerableNode<TSource>>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
            => new(NodeImpl.GroupBy(ToNode(source), keySelector, elementSelector, comparer));

        public static ValueEnumerable<TResult, GroupByResultNode<TSource, TKey, TResult, EnumerableNode<TSource>>> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer = null)
            => new(NodeImpl.GroupBy(ToNode(source), keySelector, resultSelector, comparer));
        public static ValueEnumerable<TResult, GroupByResultNode<TSource, TKey, TElement, TResult, EnumerableNode<TSource>>> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer = null)
            => new(NodeImpl.GroupBy(ToNode(source), keySelector, elementSelector, resultSelector, comparer));
    }
}
