﻿using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
            => source.OfEnumerable().Aggregate(func);

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => source.OfEnumerable().Aggregate(seed, func);

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => source.OfEnumerable().Aggregate(seed, func, resultSelector);

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => source.OfEnumerable().All(predicate);

        public static bool Any<TSource>(this IEnumerable<TSource> source)
            => source switch
            {
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

        public static ValueEnumerable<U, SelectNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, Func<T, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, Select_InNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, InFunc<T, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, SelectiNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, Func<T, int, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<T, WhereNode<T, EnumerableNode<T>>> Where<T>(this IEnumerable<T> inner, Func<T, bool> f) => inner.OfEnumerable().Where(f);

        public static ValueEnumerable<T, Where_InNode<T, EnumerableNode<T>>> Where<T>(this IEnumerable<T> inner, InFunc<T, bool> f) => inner.OfEnumerable().Where(f);

        public static TSource Last<TSource>(this IEnumerable<TSource> source) => source.OfEnumerable().Last();

        public static List<T> ToList<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is INode
                ? source.OfEnumerable().ToList()
                : new List<T>(source);
        }

        public static int Sum(this IEnumerable<int> inner) => inner.OfEnumerable().Sum();
        public static double Sum(this IEnumerable<double> inner) => inner.OfEnumerable().Sum();

        public static int Count<T>(this IEnumerable<T> inner) => inner.OfEnumerable().Count();

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfEnumerable().SelectMany(src => selector(src).OfEnumerable());
        }


        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return first.OfEnumerable().Concat(second.OfEnumerable());
        }
    }
}