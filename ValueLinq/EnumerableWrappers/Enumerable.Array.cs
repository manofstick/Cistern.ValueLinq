using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class CisternArrayExtensions
    {

        public static ValueEnumerable<TSource, ReverseNode<TSource, ArrayNode<TSource>>> Reverse<TSource>(this TSource[] source)
            => source.OfArray().Reverse();

        public static bool Any<TSource>(this TSource[] source) => source.Length > 0;

        // --

        public static ValueEnumerable<U, SelectNode<T, U, ArrayNode<T>>> Select<T, U>(this T[] inner, Func<T, U> f) => inner.OfArray().Select(f);

        public static ValueEnumerable<U, Select_InNode<T, U, ArrayNode<T>>> Select<T, U>(this T[] inner, InFunc<T, U> f) => inner.OfArray().Select(f);

        public static ValueEnumerable<U, SelectIdxNode<T, U, ArrayNode<T>>> Select<T, U>(this T[] inner, Func<T, int, U> f) => inner.OfArray().Select(f);

        public static ValueEnumerable<T, WhereNode<T, ArrayNode<T>>> Where<T>(this T[] inner, Func<T, bool> f) => inner.OfArray().Where(f);

        public static ValueEnumerable<T, Where_InNode<T, ArrayNode<T>>> Where<T>(this T[] inner, InFunc<T, bool> f) => inner.OfArray().Where(f);

        public static TSource Last<TSource>(this TSource[] source) => source.OfArray().Last();
        public static TSource LastOrDefault<TSource>(this TSource[] source) => source.OfArray().LastOrDefault();

        public static List<T> ToList<T>(this T[] source) => source.OfArray().ToList();

        public static int Count<T>(this T[] source) => source.Length;

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ArrayNode<TSource>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this TSource[] source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfArray().SelectMany(src => selector(src).OfEnumerable());
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ArrayNode<TSource>, NodeU>> SelectMany<TSource, TResult, NodeU>(this TSource[] source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfArray().SelectMany(selector);
        }

        public static IEnumerable<T> Concat<T>(this T[] first, T[] second) => first.OfArray().Concat(second.OfArray());

        public static TSource ElementAt<TSource>(this TSource[] source, int index) => source.OfArray().ElementAt(index);
        public static TSource ElementAtOrDefault<TSource>(this TSource[] source, int index) => source.OfArray().ElementAtOrDefault(index);

        public static ValueEnumerable<T, SkipNode<T, ArrayNode<T>>> Skip<T>(this T[] source, int count) => source.OfArray().Skip(count);
        public static ValueEnumerable<T, SkipWhileNode<T, ArrayNode<T>>> SkipWhile<T>(this T[] source, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfArray().SkipWhile(predicate);
        }
        public static ValueEnumerable<T, SkipWhileIdxNode<T, ArrayNode<T>>> SkipWhile<T>(this T[] source, Func<T, int, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfArray().SkipWhileIdx(predicate);
        }
        public static ValueEnumerable<T, TakeNode<T, ArrayNode<T>>> Take<T>(this T[] source, int count) => source.OfArray().Take(count);
        public static ValueEnumerable<T, TakeWhileNode<T, ArrayNode<T>>> TakeWhile<T>(this T[] source, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfArray().TakeWhile(predicate);
        }

        // -- Value based select

        public static ValueEnumerable<U, ValueSelectNode<T, U, ArrayNode<T>, IFunc>> Select<T, U, IFunc>(this T[] prior, IFunc selector, U u = default)
            where IFunc : IFunc<T, U> => prior.OfArray().Select(selector, u);

        public static ValueEnumerable<T, ValueWhereNode<T, ArrayNode<T>, Predicate>> Where<T, Predicate>(this T[] inner, Predicate predicate)
            where Predicate : IFunc<T, bool>
            => inner.OfArray().Where(predicate);
    }
}
