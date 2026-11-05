using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class ValueLinqList
    {
        public static bool Any<TSource>(this List<TSource> source) => source.Count() > 0;

        public static TSource Last<TSource>(this List<TSource> source) =>
            (source ?? throw new ArgumentNullException(nameof(source))) switch
            {
                { Count: 0 } => throw new InvalidOperationException(Properties.SR.NoElements),
                var list => list[^1],
            };

        public static TSource Last<TSource>(this List<TSource> source, Func<TSource, bool> predicate) => source.OfList().Last(predicate);
        public static TSource? LastOrDefault<TSource>(this List<TSource> source) =>
            (source ?? throw new ArgumentNullException(nameof(source))) switch
            {
                { Count: 0 } => default,
                var list => list[^1],
            };

        public static TSource? LastOrDefault<TSource>(this List<TSource> source, Func<TSource, bool> predicate) => source.OfList().LastOrDefault(predicate);
        public static TSource LastOrDefault<TSource>(this List<TSource> source, TSource defaultValue) =>
            (source ?? throw new ArgumentNullException(nameof(source))) switch
            {
                { Count: 0 } => defaultValue,
                var list => list[^1],
            };

        public static TSource LastOrDefault<TSource>(this List<TSource> source, Func<TSource, bool> predicate, TSource defaultValue) => source.OfList().LastOrDefault(predicate, defaultValue);

        public static bool SequenceEqual<TSource>(this List<TSource> first, IEnumerable<TSource> second) => SequenceEqual(first, second, null);

        public static bool SequenceEqual<TSource>(this List<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            if (!Enumerable.IsCisternChain(second))
                return System.Linq.Enumerable.SequenceEqual(first, second, comparer);

            var equal = first.OfList().SequenceEqual(second, comparer);

            return equal;
        }

        public static List<T> ToList<T>(this List<T> source) => new (source);

        public static int Count<T>(this List<T> source) => (source ?? throw new ArgumentNullException(nameof(source))).Count;

        public static long LongCount<T>(this List<T> source) => (source ?? throw new ArgumentNullException(nameof(source))).Count;

        public static bool TryGetNonEnumeratedCount<T>(this List<T> source, out int count)
        {
            count = (source ?? throw new ArgumentNullException(nameof(source))).Count;
            return true;
        }

        public static ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemList.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemList.Node<T>, SystemList.Pull<T>>> Append<T>(this List<T> source, T element) => new (NodeImpl.Append<T, SystemList.Node<T>>(Enumerable.ToNode(source), element));
        public static ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemList.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemList.Node<T>, SystemList.Pull<T>>> Prepend<T>(this List<T> source, T element) => new (NodeImpl.Prepend<T, SystemList.Node<T>>(Enumerable.ToNode(source), element));

        public static TSource ElementAt<TSource>(this List<TSource> source, int index) => (source ?? throw new ArgumentNullException(nameof(source)))[index];
        public static TSource? ElementAtOrDefault<TSource>(this List<TSource> source, int index) => (uint)index < (uint)(source ?? throw new ArgumentNullException(nameof(source))).Count ? source[index] : default;

        public static (U, V) Fork<T, U, V>(this List<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v)
            => NodeImpl.Fork(new SystemList.Node<T>(source), t2u, t2v);
        public static (U, V, W) Fork<T, U, V, W>(this List<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, W> t2w)
            => NodeImpl.Fork(new SystemList.Node<T>(source), t2u, t2v, t2w);

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, Nodes.GroupBy.Node<TSource, TKey, SystemList.Node<TSource>>, Nodes.GroupBy.Pull<TSource, TKey, SystemList.Node<TSource>>> GroupBy<TSource, TKey>(this List<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        => new(NodeImpl.GroupBy(new SystemList.Node<TSource>(source), keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, Nodes.GroupBy.Node<TSource, TKey, TElement, SystemList.Node<TSource>>, Nodes.GroupBy.Pull<TSource, TKey, TElement, SystemList.Node<TSource>>> GroupBy<TSource, TKey, TElement>(this List<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemList.Node<TSource>(source), keySelector, elementSelector, comparer));

        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TResult, SystemList.Node<TSource>>, Nodes.GroupByResult.Pull<TSource, TKey, TResult, SystemList.Node<TSource>>> GroupBy<TSource, TKey, TResult>(this List<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemList.Node<TSource>(source), keySelector, resultSelector, comparer));
        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TElement, TResult, SystemList.Node<TSource>>, Nodes.GroupByResult.Pull<TSource, TKey, TElement, TResult, SystemList.Node<TSource>>> GroupBy<TSource, TKey, TElement, TResult>(this List<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemList.Node<TSource>(source), keySelector, elementSelector, resultSelector, comparer));
    }
}
