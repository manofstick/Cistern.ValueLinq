using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;


namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static ValueEnumerable<TSource, ReverseNode<TSource, MemoryNode<TSource>>> Reverse<TSource>(this ReadOnlyMemory<TSource> source)
            => source.OfMemory().Reverse();

        public static bool Contains<TSource>(this ReadOnlyMemory<TSource> source, TSource value)
        {
            var aggregate = new Contains<TSource>(value);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        // ---------------





        public static bool Any<TSource>(this ReadOnlyMemory<TSource> source) => source.Length > 0;


        // --

        public static ValueEnumerable<U, SelectNode<T, U, MemoryNode<T>>> Select<T, U>(this ReadOnlyMemory<T> inner, Func<T, U> f) => inner.OfMemory().Select(f);

        public static ValueEnumerable<U, Select_InNode<T, U, MemoryNode<T>>> Select<T, U>(this ReadOnlyMemory<T> inner, InFunc<T, U> f) => inner.OfMemory().Select(f);

        public static ValueEnumerable<U, SelectIdxNode<T, U, MemoryNode<T>>> Select<T, U>(this ReadOnlyMemory<T> inner, Func<T, int, U> f) => inner.OfMemory().Select(f);

        public static ValueEnumerable<T, WhereNode<T, MemoryNode<T>>> Where<T>(this ReadOnlyMemory<T> inner, Func<T, bool> f) => inner.OfMemory().Where(f);

        public static ValueEnumerable<T, Where_InNode<T, MemoryNode<T>>> Where<T>(this ReadOnlyMemory<T> inner, InFunc<T, bool> f) => inner.OfMemory().Where(f);

        public static TSource Last<TSource>(this ReadOnlyMemory<TSource> source) => source.OfMemory().Last();
        public static TSource LastOrDefault<TSource>(this ReadOnlyMemory<TSource> source) => source.OfMemory().LastOrDefault();

        public static List<T> ToList<T>(this ReadOnlyMemory<T> source) => source.OfMemory().ToList();

        public static int Count<T>(this ReadOnlyMemory<T> inner) => inner.Length;

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, MemoryNode<TSource>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this ReadOnlyMemory<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfMemory().SelectMany(src => selector(src).OfEnumerable());
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, MemoryNode<TSource>, NodeU>> SelectMany<TSource, TResult, NodeU>(this ReadOnlyMemory<TSource> source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfMemory().SelectMany(selector);
        }

        public static IEnumerable<T> Concat<T>(this ReadOnlyMemory<T> first, ReadOnlyMemory<T> second) => first.OfMemory().Concat(second.OfMemory());

        public static TSource ElementAt<TSource>(this ReadOnlyMemory<TSource> source, int index) => source.OfMemory().ElementAt(index);
        public static TSource ElementAtOrDefault<TSource>(this ReadOnlyMemory<TSource> source, int index) => source.OfMemory().ElementAtOrDefault(index);

        public static ValueEnumerable<T, SkipNode<T, MemoryNode<T>>> Skip<T>(this ReadOnlyMemory<T> source, int count) => source.OfMemory().Skip(count);
        public static ValueEnumerable<T, SkipWhileNode<T, MemoryNode<T>>> SkipWhile<T>(this ReadOnlyMemory<T> source, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfMemory().SkipWhile(predicate);
        }
        public static ValueEnumerable<T, SkipWhileIdxNode<T, MemoryNode<T>>> SkipWhile<T>(this ReadOnlyMemory<T> source, Func<T, int, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfMemory().SkipWhileIdx(predicate);
        }
        public static ValueEnumerable<T, TakeNode<T, MemoryNode<T>>> Take<T>(this ReadOnlyMemory<T> source, int count) => source.OfMemory().Take(count);
        public static ValueEnumerable<T, TakeWhileNode<T, MemoryNode<T>>> TakeWhile<T>(this ReadOnlyMemory<T> source, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfMemory().TakeWhile(predicate);
        }
        public static ValueEnumerable<T, TakeWhileIdxNode<T, MemoryNode<T>>> TakeWhile<T>(this ReadOnlyMemory<T> source, Func<T, int, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfMemory().TakeWhile(predicate);
        }

        // -- Value based select

        public static ValueEnumerable<U, ValueSelectNode<T, U, MemoryNode<T>, IFunc>> Select<T, U, IFunc>(this ReadOnlyMemory<T> prior, IFunc selector, U u = default)
            where IFunc : IFunc<T, U> => prior.OfMemory().Select(selector, u);

        public static ValueEnumerable<T, ValueWhereNode<T, MemoryNode<T>, Predicate>> Where<T, Predicate>(this ReadOnlyMemory<T> inner, Predicate predicate)
            where Predicate : IFunc<T, bool>
            => inner.OfMemory().Where(predicate);
    }
}
