using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static bool Any<TSource>(this List<TSource> source) => source.Count() > 0;

        public static bool Any<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Count == 0 ? false : source.OfList().Any(predicate);
        }

        // --

        public static ValueEnumerable<U, SelectNode<T, U, ListNode<T>>> Select<T, U>(this List<T> inner, Func<T, U> f) => inner.OfList().Select(f);

        public static ValueEnumerable<U, Select_InNode<T, U, ListNode<T>>> Select<T, U>(this List<T> inner, InFunc<T, U> f) => inner.OfList().Select(f);

        public static ValueEnumerable<U, SelectIdxNode<T, U, ListNode<T>>> Select<T, U>(this List<T> inner, Func<T, int, U> f) => inner.OfList().Select(f);

        public static ValueEnumerable<T, WhereNode<T, ListNode<T>>> Where<T>(this List<T> inner, Func<T, bool> f) => inner.OfList().Where(f);

        public static ValueEnumerable<T, Where_InNode<T, ListNode<T>>> Where<T>(this List<T> inner, InFunc<T, bool> f) => inner.OfList().Where(f);

        public static TSource Last<TSource>(this List<TSource> source) => source.OfList().Last();
        public static TSource LastOrDefault<TSource>(this List<TSource> source) => source.OfList().LastOrDefault();

        public static List<T> ToList<T>(this List<T> source) => source.OfList().ToList();

        public static int Count<T>(this List<T> inner) => inner.OfList().Count();

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

        public static IEnumerable<T> Concat<T>(this List<T> first, List<T> second) => first.OfList().Concat(second.OfList());

        public static TSource ElementAt<TSource>(this List<TSource> source, int index) => source.OfList().ElementAt(index);
        public static TSource ElementAtOrDefault<TSource>(this List<TSource> source, int index) => source.OfList().ElementAtOrDefault(index);

        public static ValueEnumerable<T, SkipNode<T, ListNode<T>>> Skip<T>(this List<T> source, int count) => source.OfList().Skip(count);
        public static ValueEnumerable<T, SkipWhileNode<T, ListNode<T>>> SkipWhile<T>(this List<T> source, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfList().SkipWhile(predicate);
        }
        public static ValueEnumerable<T, SkipWhileIdxNode<T, ListNode<T>>> SkipWhile<T>(this List<T> source, Func<T, int, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfList().SkipWhileIdx(predicate);
        }
        public static ValueEnumerable<T, TakeNode<T, ListNode<T>>> Take<T>(this List<T> source, int count) => source.OfList().Take(count);

        // -- Value based select

        public static ValueEnumerable<U, ValueSelectNode<T, U, ListNode<T>, IFunc>> Select<T, U, IFunc>(this List<T> prior, IFunc selector, U u = default)
            where IFunc : IFunc<T, U> => prior.OfList().Select(selector, u);

        public static ValueEnumerable<T, ValueWhereNode<T, ListNode<T>, Predicate>> Where<T, Predicate>(this List<T> inner, Predicate predicate)
            where Predicate : IFunc<T, bool>
            => inner.OfList().Where(predicate);
    }
}
