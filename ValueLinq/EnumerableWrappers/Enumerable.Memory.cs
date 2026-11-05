using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;


namespace Cistern.ValueLinq
{
    public static partial class ValueLinqMemory
    {
        public static bool Contains<TSource>(this ReadOnlyMemory<TSource> source, TSource value) =>
            SystemMemory.ExecutePush<TSource, bool, Contains<TSource>>(source, new Contains<TSource>(value));

        // ---------------





        public static bool Any<TSource>(this ReadOnlyMemory<TSource> source) => source.Length > 0;

        public static TSource Last<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate) => source.OfMemory().Last(predicate);
        public static TSource? LastOrDefault<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate) => source.OfMemory().LastOrDefault(predicate);


        // --

        public static TSource Last<TSource>(this ReadOnlyMemory<TSource> source) => source.OfMemory().Last();
        public static TSource? LastOrDefault<TSource>(this ReadOnlyMemory<TSource> source) => source.OfMemory().LastOrDefault();
        public static TSource LastOrDefault<TSource>(this ReadOnlyMemory<TSource> source, TSource defaultValue) => source.OfMemory().LastOrDefault(defaultValue);
        public static TSource LastOrDefault<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate, TSource defaultValue) => source.OfMemory().LastOrDefault(predicate, defaultValue);

        public static bool SequenceEqual<TSource>(this ReadOnlyMemory<TSource> first, IEnumerable<TSource> second) => first.OfMemory().SequenceEqual(second, null);
        public static bool SequenceEqual<TSource>(this ReadOnlyMemory<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer) => first.OfMemory().SequenceEqual(second, comparer);

        public static List<T> ToList<T>(this ReadOnlyMemory<T> source) => source.OfMemory().ToList();

        public static int Count<T>(this ReadOnlyMemory<T> inner) => inner.Length;

        public static long LongCount<T>(this ReadOnlyMemory<T> source) => source.Length;

        public static bool TryGetNonEnumeratedCount<T>(this ReadOnlyMemory<T> source, out int count)
        {
            count = source.Length;
            return true;
        }

        public static ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemMemory.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemMemory.Node<T>, SystemMemory.Pull<T>>> Append<T>(this ReadOnlyMemory<T> source, T element) => new (NodeImpl.Append<T, SystemMemory.Node<T>>(Enumerable.ToNode(source), element));
        public static ValueEnumerable<T, Nodes.AppendPrepend.Node<T, SystemMemory.Node<T>>, Nodes.AppendPrepend.Pull<T, SystemMemory.Node<T>, SystemMemory.Pull<T>>> Prepend<T>(this ReadOnlyMemory<T> source, T element) => new (NodeImpl.Prepend<T, SystemMemory.Node<T>>(Enumerable.ToNode(source), element));

        public static TSource ElementAt<TSource>(this ReadOnlyMemory<TSource> source, int index) => source.OfMemory().ElementAt(index);
        public static TSource? ElementAtOrDefault<TSource>(this ReadOnlyMemory<TSource> source, int index) => source.OfMemory().ElementAtOrDefault(index);

        public static (U, V) Fork<T, U, V>(this ReadOnlyMemory<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v)
            => NodeImpl.Fork(new SystemMemory.Node<T>(source), t2u, t2v);
        public static (U, V, W) Fork<T, U, V, W>(this ReadOnlyMemory<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, W> t2w)
            => NodeImpl.Fork(new SystemMemory.Node<T>(source), t2u, t2v, t2w);

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, Nodes.GroupBy.Node<TSource, TKey, SystemMemory.Node<TSource>>, Nodes.GroupBy.Pull<TSource, TKey, SystemMemory.Node<TSource>>> GroupBy<TSource, TKey>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        => new(NodeImpl.GroupBy(new SystemMemory.Node<TSource>(source), keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, Nodes.GroupBy.Node<TSource, TKey, TElement, SystemMemory.Node<TSource>>, Nodes.GroupBy.Pull<TSource, TKey, TElement, SystemMemory.Node<TSource>>> GroupBy<TSource, TKey, TElement>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemMemory.Node<TSource>(source), keySelector, elementSelector, comparer));

        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TResult, SystemMemory.Node<TSource>>, Nodes.GroupByResult.Pull<TSource, TKey, TResult, SystemMemory.Node<TSource>>> GroupBy<TSource, TKey, TResult>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemMemory.Node<TSource>(source), keySelector, resultSelector, comparer));
        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TElement, TResult, SystemMemory.Node<TSource>>, Nodes.GroupByResult.Pull<TSource, TKey, TElement, TResult, SystemMemory.Node<TSource>>> GroupBy<TSource, TKey, TElement, TResult>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            => new(NodeImpl.GroupBy(new SystemMemory.Node<TSource>(source), keySelector, elementSelector, resultSelector, comparer));
    }
}
