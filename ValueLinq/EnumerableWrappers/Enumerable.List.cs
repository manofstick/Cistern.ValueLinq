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

        public static TSource Last<TSource>(this List<TSource> source) => source[source.Count - 1];
        public static TSource LastOrDefault<TSource>(this List<TSource> source) => source.Count == 0 ? default : source[source.Count - 1];

        public static List<T> ToList<T>(this List<T> source) => new (source);

        public static int Count<T>(this List<T> inner) => inner.Count;

        public static ValueEnumerable<TResult, SelectManyNode<TResult, EnumerableNode<TResult>, SelectNode<TSource, EnumerableNode<TResult>, ListNode<TSource>>>> SelectMany<TSource, TResult>(this List<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(new(new(source), source => new(selector(source)))));
        }

        public static ValueEnumerable<TResult, SelectManyNode<TResult, EnumerableNode<TResult>, SelectIdxNode<TSource, EnumerableNode<TResult>, ListNode<TSource>>>> SelectMany<TSource, TResult>(this List<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(new(new(source), (source, idx) => new(selector(source, idx)))));
        }

        //public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ListNode<TSource>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this List<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        //{
        //    if (selector == null)
        //        throw new ArgumentNullException(nameof(selector));

        //    return source.OfList().SelectMany(src => selector(src).OfEnumerable());
        //}

        //public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ListNode<TSource>, NodeU>> SelectMany<TSource, TResult, NodeU>(this List<TSource> source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
        //    where NodeU : INode<TResult>
        //{
        //    if (selector == null)
        //        throw new ArgumentNullException(nameof(selector));

        //    return source.OfList().SelectMany(selector);
        //}

        public static IEnumerable<T> Concat<T>(this List<T> first, List<T> second) => new ValueEnumerable<T, ConcatNode<T, ListNode<T>, ListNode<T>>>(NodeImpl.Concat<T, ListNode<T>, ListNode<T>>(Enumerable.ToNode(first), Enumerable.ToNode(second)));

        public static TSource ElementAt<TSource>(this List<TSource> source, int index) => source[index];
        public static TSource ElementAtOrDefault<TSource>(this List<TSource> source, int index) => index >= source.Count ? default : source[index];

        public static (U, V) Fork<T, U, V>(this List<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v)
            => NodeImpl.Fork(new ListNode<T>(source), t2u, t2v);
        public static (U, V, W) Fork<T, U, V, W>(this List<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>>, W> t2w)
            => NodeImpl.Fork(new ListNode<T>(source), t2u, t2v, t2w);

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, GroupByNode<TSource, TKey, ListNode<TSource>>> GroupBy<TSource, TKey>(this List<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        => new(NodeImpl.GroupBy(new ListNode<TSource>(source), keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, GroupByNode<TSource, TKey, TElement, ListNode<TSource>>> GroupBy<TSource, TKey, TElement>(this List<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
            => new(NodeImpl.GroupBy(new ListNode<TSource>(source), keySelector, elementSelector, comparer));
    }
}
