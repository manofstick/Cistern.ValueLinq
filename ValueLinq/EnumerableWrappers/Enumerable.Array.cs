using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.Optimizations;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class ValueLinqArray
    {

        public static bool Any<TSource>(this TSource[] source) => source.Length > 0;

        // --

        public static TSource Last<TSource>(this TSource[] source) => source.OfArray().Last();
        public static TSource LastOrDefault<TSource>(this TSource[] source) => source.OfArray().LastOrDefault();

        public static List<T> ToList<T>(this T[] source) => source.OfArray().ToList();

        public static int Count<T>(this T[] source) => source.Length;

        public static ValueEnumerable<TResult, SelectManyNode<TResult, EnumerableNode<TResult>, SelectNode<TSource, EnumerableNode<TResult>, ArrayNode<TSource>>>> SelectMany<TSource, TResult>(this TSource[] source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(new(new(source), source => new(selector(source)))));
        }

        public static ValueEnumerable<TResult, SelectManyNode<TResult, EnumerableNode<TResult>, SelectIdxNode<TSource, EnumerableNode<TResult>, ArrayNode<TSource>>>> SelectMany<TSource, TResult>(this TSource[] source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(new(new(source), (source, idx) => new(selector(source, idx)))));
        }

        //public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ArrayNode<TSource>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this TSource[] source, Func<TSource, IEnumerable<TResult>> selector)
        //{
        //    if (selector == null)
        //        throw new ArgumentNullException(nameof(selector));

        //    return source.OfArray().SelectMany(src => selector(src).OfEnumerable());
        //}

        //public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ArrayNode<TSource>, NodeU>> SelectMany<TSource, TResult, NodeU>(this TSource[] source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
        //    where NodeU : INode<TResult>
        //{
        //    if (selector == null)
        //        throw new ArgumentNullException(nameof(selector));

        //    return source.OfArray().SelectMany(selector);
        //}

        public static IEnumerable<T> Concat<T>(this T[] first, T[] second)
            => first.OfArray().Concat(second.OfArray());

        public static TSource ElementAt<TSource>(this TSource[] source, int index)
            => NodeImpl.ElementAt<TSource, ArrayNode<TSource>>(new ArrayNode<TSource>(source), index);
        public static TSource ElementAtOrDefault<TSource>(this TSource[] source, int index)
            => NodeImpl.ElementAtOrDefault<TSource, ArrayNode<TSource>>(new ArrayNode<TSource>(source), index);

        public static (U, V) Fork<T, U, V>(this T[] source, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v)
            => NodeImpl.Fork(new ArrayNode<T>(source), t2u, t2v);
        public static (U, V, W) Fork<T, U, V, W>(this T[] source, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>>, W> t2w)
            => NodeImpl.Fork(new ArrayNode<T>(source), t2u, t2v, t2w);

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, GroupByNode<TSource, TKey, ArrayNode<TSource>>> GroupBy<TSource, TKey>(this TSource[] source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
            => new(NodeImpl.GroupBy(new ArrayNode<TSource>(source), keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, GroupByNode<TSource, TKey, TElement, ArrayNode<TSource>>> GroupBy<TSource, TKey, TElement>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
            => new(NodeImpl.GroupBy(new ArrayNode<TSource>(source), keySelector, elementSelector, comparer));
    }
}
