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

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ListNode<TSource>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this List<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfList().SelectMany(src => selector(src).OfEnumerable());
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ListNode<TSource>, NodeU>> SelectMany<TSource, TResult, NodeU>(this List<TSource> source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfList().SelectMany(selector);
        }

        public static IEnumerable<T> Concat<T>(this List<T> first, List<T> second) => new ValueEnumerable<T, ConcatNode<T, ListNode<T>, ListNode<T>>>(NodeImpl.Concat<T, ListNode<T>, ListNode<T>>(Enumerable.ToNode(first), Enumerable.ToNode(second)));

        public static TSource ElementAt<TSource>(this List<TSource> source, int index) => source[index];
        public static TSource ElementAtOrDefault<TSource>(this List<TSource> source, int index) => index >= source.Count ? default : source[index];
    }
}
