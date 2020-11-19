using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static bool Any<TSource>(this IEnumerable<TSource> source)
            => source switch
            {
                System.Collections.ICollection c => c.Count > 0,
                ICollection<TSource> c => c.Count > 0,
                IReadOnlyCollection<TSource> c => c.Count > 0,
                _ => source.OfEnumerable().Any()
            };

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source switch
            {
                ICollection<TSource> { Count: 0 } => false,
                IReadOnlyCollection<TSource> { Count: 0 } => false,
                _ => source.OfEnumerable().Any(predicate),
            };
        }

        // --

        public static ValueEnumerable<U, SelectLegacyNode<T, U>> Select<T, U>(this IEnumerable<T> source, Func<T, U> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, SelectLegacyNode<T,U>>(new SelectLegacyNode<T, U>(source, selector));
        }

        public static ValueEnumerable<U, Select_InNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, InFunc<T, U> f) =>
            inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, SelectIdxNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, Func<T, int, U> f) =>
            inner.OfEnumerable().Select(f);

        public static ValueEnumerable<T, WhereLegacyNode<T>> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if(source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, WhereLegacyNode<T>>(new WhereLegacyNode<T>(source, predicate));
        }

        public static ValueEnumerable<T, Where_InNode<T, EnumerableNode<T>>> Where<T>(this IEnumerable<T> inner, InFunc<T, bool> f) =>
            inner.OfEnumerable().Where(f);

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
                if (count == 0)
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

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, EnumerableNode<TSource>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfEnumerable().SelectMany(src => selector(src).OfEnumerable());
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, EnumerableNode<TSource>, NodeU>> SelectMany<TSource, TResult, NodeU>(this IEnumerable<TSource> source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfEnumerable().SelectMany(selector);
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return first.OfEnumerable().Concat(second.OfEnumerable());
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) =>
            source.OfEnumerable().ElementAt(index);

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index) => source.OfEnumerable().ElementAtOrDefault(index);

        public static ValueEnumerable<T, SkipNode<T, EnumerableNode<T>>> Skip<T>(this IEnumerable<T> source, int count) => source.OfEnumerable().Skip(count);
        public static ValueEnumerable<T, SkipWhileNode<T, EnumerableNode<T>>> SkipWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfEnumerable().SkipWhile(predicate);
        }
        public static ValueEnumerable<T, SkipWhileIdxNode<T, EnumerableNode<T>>> SkipWhile<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfEnumerable().SkipWhileIdx(predicate);
        }
        public static ValueEnumerable<T, TakeNode<T, EnumerableNode<T>>> Take<T>(this IEnumerable<T> source, int count) => source.OfEnumerable().Take(count);

        // -- Value based select

        public static ValueEnumerable<U, ValueSelectNode<T, U, EnumerableNode<T>, IFunc>> Select<T, U, IFunc>(this IEnumerable<T> prior, IFunc selector, U u = default)
            where IFunc : IFuncBase<T, U> => prior.OfEnumerable().Select(selector, u);

        public static ValueEnumerable<T, ValueWhereNode<T, EnumerableNode<T>, Predicate>> Where<T, Predicate>(this IEnumerable<T> inner, Predicate predicate)
            where Predicate : IFuncBase<T, bool>
            => inner.OfEnumerable().Where(predicate);
    }
}
