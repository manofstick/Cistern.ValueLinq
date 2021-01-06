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
        public static bool Contains<TSource>(this ReadOnlyMemory<TSource> source, TSource value)
        {
            var aggregate = new Contains<TSource>(value);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        // ---------------





        public static bool Any<TSource>(this ReadOnlyMemory<TSource> source) => source.Length > 0;


        // --

        public static TSource Last<TSource>(this ReadOnlyMemory<TSource> source) => source.OfMemory().Last();
        public static TSource LastOrDefault<TSource>(this ReadOnlyMemory<TSource> source) => source.OfMemory().LastOrDefault();

        public static List<T> ToList<T>(this ReadOnlyMemory<T> source) => source.OfMemory().ToList();

        public static int Count<T>(this ReadOnlyMemory<T> inner) => inner.Length;

        public static ValueEnumerable<TResult, SelectManyNode<TResult, EnumerableNode<TResult>, SelectNode<TSource, EnumerableNode<TResult>, MemoryNode<TSource>>>> SelectMany<TSource, TResult>(this ReadOnlyMemory<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(new(new(source), source => new(selector(source)))));
        }

        public static ValueEnumerable<TResult, SelectManyNode<TResult, EnumerableNode<TResult>, SelectIdxNode<TSource, EnumerableNode<TResult>, MemoryNode<TSource>>>> SelectMany<TSource, TResult>(this ReadOnlyMemory<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(new(new(source), (source, idx) => new(selector(source, idx)))));
        }
        //public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, MemoryNode<TSource>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this ReadOnlyMemory<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        //{
        //    if (selector == null)
        //        throw new ArgumentNullException(nameof(selector));

        //    return source.OfMemory().SelectMany(src => selector(src).OfEnumerable());
        //}

        //public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, MemoryNode<TSource>, NodeU>> SelectMany<TSource, TResult, NodeU>(this ReadOnlyMemory<TSource> source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
        //    where NodeU : INode<TResult>
        //{
        //    if (selector == null)
        //        throw new ArgumentNullException(nameof(selector));

        //    return source.OfMemory().SelectMany(selector);
        //}

        public static IEnumerable<T> Concat<T>(this ReadOnlyMemory<T> first, ReadOnlyMemory<T> second) => first.OfMemory().Concat(second.OfMemory());

        public static TSource ElementAt<TSource>(this ReadOnlyMemory<TSource> source, int index) => source.OfMemory().ElementAt(index);
        public static TSource ElementAtOrDefault<TSource>(this ReadOnlyMemory<TSource> source, int index) => source.OfMemory().ElementAtOrDefault(index);

        public static (U, V) Fork<T, U, V>(this ReadOnlyMemory<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v)
            => NodeImpl.Fork(new MemoryNode<T>(source), t2u, t2v);
        public static (U, V, W) Fork<T, U, V, W>(this ReadOnlyMemory<T> source, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>>, W> t2w)
            => NodeImpl.Fork(new MemoryNode<T>(source), t2u, t2v, t2w);

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, GroupByNode<TSource, TKey, MemoryNode<TSource>>> GroupBy<TSource, TKey>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        => new(NodeImpl.GroupBy(new MemoryNode<TSource>(source), keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, GroupByNode<TSource, TKey, TElement, MemoryNode<TSource>>> GroupBy<TSource, TKey, TElement>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
            => new(NodeImpl.GroupBy(new MemoryNode<TSource>(source), keySelector, elementSelector, comparer));

        public static ValueEnumerable<TResult, GroupByResultNode<TSource, TKey, TResult, MemoryNode<TSource>>> GroupBy<TSource, TKey, TResult>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer = null)
            => new(NodeImpl.GroupBy(new MemoryNode<TSource>(source), keySelector, resultSelector, comparer));
    }
}
