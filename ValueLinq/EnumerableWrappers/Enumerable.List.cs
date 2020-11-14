﻿using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

// TODO: Probably create template to create (List/Array/Memory) forwards

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource>(this List<TSource> source, Func<TSource, TSource, TSource> func)
            => source.OfListByIndex().Aggregate(func);

        public static TAccumulate Aggregate<TSource, TAccumulate>(this List<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => source.OfListByIndex().Aggregate(seed, func);

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this List<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => source.OfListByIndex().Aggregate(seed, func, resultSelector);

        public static bool All<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
            => source.OfListByIndex().All(predicate);

        public static bool Any<TSource>(this List<TSource> source) => source.Count() > 0;

        public static bool Any<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source switch
            {
                ICollection<TSource> { Count: 0 } => false,
                IReadOnlyCollection<TSource> { Count: 0 } => false,
                _ => source.OfListByIndex().Any(predicate),
            };
        }

        // --

        public static ValueEnumerable<U, SelectNode<T, U, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>> Select<T, U>(this List<T> inner, Func<T, U> f) => inner.OfListByEnumerator().Select(f);

        public static ValueEnumerable<U, Select_InNode<T, U, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>> Select<T, U>(this List<T> inner, InFunc<T, U> f) => inner.OfListByEnumerator().Select(f);

        public static ValueEnumerable<U, SelectIdxNode<T, U, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>> Select<T, U>(this List<T> inner, Func<T, int, U> f) => inner.OfListByEnumerator().Select(f);

        public static ValueEnumerable<T, WhereNode<T, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>> Where<T>(this List<T> inner, Func<T, bool> f) => inner.OfListByEnumerator().Where(f);

        public static ValueEnumerable<T, Where_InNode<T, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>> Where<T>(this List<T> inner, InFunc<T, bool> f) => inner.OfListByEnumerator().Where(f);

        public static TSource Last<TSource>(this List<TSource> source) => source.OfListByIndex().Last();
        public static TSource LastOrDefault<TSource>(this List<TSource> source) => source.OfListByIndex().LastOrDefault();

        public static List<T> ToList<T>(this List<T> source) => source.OfListByIndex().ToList();

        public static decimal Average(this List<decimal> inner) => inner.OfListByIndex().Average();
        public static double  Average(this List<double>  inner) => inner.OfListByIndex().Average();
        public static double  Average(this List<int>     inner) => inner.OfListByIndex().Average();
        public static float   Average(this List<float>   inner) => inner.OfListByIndex().Average();
        public static double  Average(this List<long>    inner) => inner.OfListByIndex().Average();

        public static decimal? Average(this List<decimal?> inner) => inner.OfListByIndex().Average();
        public static double?  Average(this List<double?>  inner) => inner.OfListByIndex().Average();
        public static double?  Average(this List<int?>     inner) => inner.OfListByIndex().Average();
        public static float?   Average(this List<float?>   inner) => inner.OfListByIndex().Average();
        public static double?  Average(this List<long?>    inner) => inner.OfListByIndex().Average();

        public static decimal Average<TSource>(this List<TSource> inner, Func<TSource, decimal> selector) => inner.OfListByIndex().Select(selector).Average();
        public static double  Average<TSource>(this List<TSource> inner, Func<TSource, double > selector) => inner.OfListByIndex().Select(selector).Average();
        public static double  Average<TSource>(this List<TSource> inner, Func<TSource, int    > selector) => inner.OfListByIndex().Select(selector).Average();
        public static double  Average<TSource>(this List<TSource> inner, Func<TSource, float  > selector) => inner.OfListByIndex().Select(selector).Average();
        public static double  Average<TSource>(this List<TSource> inner, Func<TSource, long   > selector) => inner.OfListByIndex().Select(selector).Average();

        public static decimal? Average<TSource>(this List<TSource> inner, Func<TSource, decimal?> selector) => inner.OfListByIndex().Select(selector).Average();
        public static double?  Average<TSource>(this List<TSource> inner, Func<TSource, double? > selector) => inner.OfListByIndex().Select(selector).Average();
        public static double?  Average<TSource>(this List<TSource> inner, Func<TSource, int?    > selector) => inner.OfListByIndex().Select(selector).Average();
        public static float?   Average<TSource>(this List<TSource> inner, Func<TSource, float?  > selector) => inner.OfListByIndex().Select(selector).Average();
        public static double?  Average<TSource>(this List<TSource> inner, Func<TSource, long?   > selector) => inner.OfListByIndex().Select(selector).Average();

        public static decimal Min(this List<decimal> inner) => inner.OfListByIndex().Min();
        public static double  Min(this List<double>  inner) => inner.OfListByIndex().Min();
        public static int     Min(this List<int>     inner) => inner.OfListByIndex().Min();
        public static float   Min(this List<float>   inner) => inner.OfListByIndex().Min();
        public static long    Min(this List<long>    inner) => inner.OfListByIndex().Min();
        public static T       Min<T>(this List<T>    inner) => inner.OfListByIndex().Min();

        public static decimal? Min(this List<decimal?> inner) => inner.OfListByIndex().Min();
        public static double?  Min(this List<double?>  inner) => inner.OfListByIndex().Min();
        public static int?     Min(this List<int?>     inner) => inner.OfListByIndex().Min();
        public static float?   Min(this List<float?>   inner) => inner.OfListByIndex().Min();
        public static long?    Min(this List<long?>    inner) => inner.OfListByIndex().Min();

        public static decimal Min<TSource>(this List<TSource> inner, Func<TSource, decimal> selector) => inner.OfListByIndex().Select(selector).Min();
        public static double  Min<TSource>(this List<TSource> inner, Func<TSource, double > selector) => inner.OfListByIndex().Select(selector).Min();
        public static int     Min<TSource>(this List<TSource> inner, Func<TSource, int    > selector) => inner.OfListByIndex().Select(selector).Min();
        public static float   Min<TSource>(this List<TSource> inner, Func<TSource, float  > selector) => inner.OfListByIndex().Select(selector).Min();
        public static long    Min<TSource>(this List<TSource> inner, Func<TSource, long   > selector) => inner.OfListByIndex().Select(selector).Min();
        public static T       Min<TSource, T>(this List<TSource> inner, Func<TSource, T> selector) => inner.OfListByIndex().Select(selector).Min();

        public static decimal? Min<TSource>(this List<TSource> inner, Func<TSource, decimal?> selector) => inner.OfListByIndex().Select(selector).Min();
        public static double?  Min<TSource>(this List<TSource> inner, Func<TSource, double? > selector) => inner.OfListByIndex().Select(selector).Min();
        public static int?     Min<TSource>(this List<TSource> inner, Func<TSource, int?    > selector) => inner.OfListByIndex().Select(selector).Min();
        public static float?   Min<TSource>(this List<TSource> inner, Func<TSource, float?  > selector) => inner.OfListByIndex().Select(selector).Min();
        public static long?    Min<TSource>(this List<TSource> inner, Func<TSource, long?   > selector) => inner.OfListByIndex().Select(selector).Min();


        public static decimal Max(this List<decimal> inner) => inner.OfListByIndex().Max();
        public static double  Max(this List<double>  inner) => inner.OfListByIndex().Max();
        public static int     Max(this List<int>     inner) => inner.OfListByIndex().Max();
        public static float   Max(this List<float>   inner) => inner.OfListByIndex().Max();
        public static long    Max(this List<long>    inner) => inner.OfListByIndex().Max();
        public static T       Max<T>(this List<T>    inner) => inner.OfListByIndex().Max();

        public static decimal? Max(this List<decimal?> inner) => inner.OfListByIndex().Max();
        public static double?  Max(this List<double?>  inner) => inner.OfListByIndex().Max();
        public static int?     Max(this List<int?>     inner) => inner.OfListByIndex().Max();
        public static float?   Max(this List<float?>   inner) => inner.OfListByIndex().Max();
        public static long?    Max(this List<long?>    inner) => inner.OfListByIndex().Max();

        public static decimal Max<TSource>(this List<TSource> inner, Func<TSource, decimal> selector) => inner.OfListByIndex().Select(selector).Max();
        public static double  Max<TSource>(this List<TSource> inner, Func<TSource, double > selector) => inner.OfListByIndex().Select(selector).Max();
        public static int     Max<TSource>(this List<TSource> inner, Func<TSource, int    > selector) => inner.OfListByIndex().Select(selector).Max();
        public static float   Max<TSource>(this List<TSource> inner, Func<TSource, float  > selector) => inner.OfListByIndex().Select(selector).Max();
        public static long    Max<TSource>(this List<TSource> inner, Func<TSource, long   > selector) => inner.OfListByIndex().Select(selector).Max();
        public static T       Max<TSource, T>(this List<TSource> inner, Func<TSource, T> selector) => inner.OfListByIndex().Select(selector).Max();

        public static decimal? Max<TSource>(this List<TSource> inner, Func<TSource, decimal?> selector) => inner.OfListByIndex().Select(selector).Max();
        public static double?  Max<TSource>(this List<TSource> inner, Func<TSource, double? > selector) => inner.OfListByIndex().Select(selector).Max();
        public static int?     Max<TSource>(this List<TSource> inner, Func<TSource, int?    > selector) => inner.OfListByIndex().Select(selector).Max();
        public static float?   Max<TSource>(this List<TSource> inner, Func<TSource, float?  > selector) => inner.OfListByIndex().Select(selector).Max();
        public static long?    Max<TSource>(this List<TSource> inner, Func<TSource, long?   > selector) => inner.OfListByIndex().Select(selector).Max();

        public static decimal Sum(this List<decimal> inner) => inner.OfListByIndex().Sum();
        public static double  Sum(this List<double>  inner) => inner.OfListByIndex().Sum();
        public static int     Sum(this List<int>     inner) => inner.OfListByIndex().Sum();
        public static float   Sum(this List<float>   inner) => inner.OfListByIndex().Sum();
        public static long    Sum(this List<long>    inner) => inner.OfListByIndex().Sum();

        public static decimal? Sum(this List<decimal?> inner) => inner.OfListByIndex().Sum();
        public static double?  Sum(this List<double?>  inner) => inner.OfListByIndex().Sum();
        public static int?     Sum(this List<int?>     inner) => inner.OfListByIndex().Sum();
        public static float?   Sum(this List<float?>   inner) => inner.OfListByIndex().Sum();
        public static long?    Sum(this List<long?>    inner) => inner.OfListByIndex().Sum();

        public static decimal Sum<TSource>(this List<TSource> inner, Func<TSource, decimal> selector) => inner.OfListByIndex().Select(selector).Sum();
        public static double  Sum<TSource>(this List<TSource> inner, Func<TSource, double > selector) => inner.OfListByIndex().Select(selector).Sum();
        public static int     Sum<TSource>(this List<TSource> inner, Func<TSource, int    > selector) => inner.OfListByIndex().Select(selector).Sum();
        public static float   Sum<TSource>(this List<TSource> inner, Func<TSource, float  > selector) => inner.OfListByIndex().Select(selector).Sum();
        public static long    Sum<TSource>(this List<TSource> inner, Func<TSource, long   > selector) => inner.OfListByIndex().Select(selector).Sum();

        public static decimal? Sum<TSource>(this List<TSource> inner, Func<TSource, decimal?> selector) => inner.OfListByIndex().Select(selector).Sum();
        public static double?  Sum<TSource>(this List<TSource> inner, Func<TSource, double? > selector) => inner.OfListByIndex().Select(selector).Sum();
        public static int?     Sum<TSource>(this List<TSource> inner, Func<TSource, int?    > selector) => inner.OfListByIndex().Select(selector).Sum();
        public static float?   Sum<TSource>(this List<TSource> inner, Func<TSource, float?  > selector) => inner.OfListByIndex().Select(selector).Sum();
        public static long?    Sum<TSource>(this List<TSource> inner, Func<TSource, long?   > selector) => inner.OfListByIndex().Select(selector).Sum();


        public static int Count<T>(this List<T> inner) => inner.OfListByIndex().Count();

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, GenericEnumeratorNode<TSource, List<TSource>, List<TSource>.Enumerator>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this List<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfListByEnumerator().SelectMany(src => selector(src).OfEnumerable());
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, GenericEnumeratorNode<TSource, List<TSource>, List<TSource>.Enumerator>, NodeU>> SelectMany<TSource, TResult, NodeU>(this List<TSource> source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfListByEnumerator().SelectMany(selector);
        }

        public static IEnumerable<T> Concat<T>(this List<T> first, List<T> second) => first.OfListByIndex().Concat(second.OfListByIndex());

        public static TSource ElementAt<TSource>(this List<TSource> source, int index) => source.OfListByIndex().ElementAt(index);
        public static TSource ElementAtOrDefault<TSource>(this List<TSource> source, int index) => source.OfListByIndex().ElementAtOrDefault(index);

        public static ValueEnumerable<T, SkipNode<T, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>> Skip<T>(this List<T> source, int count) => source.OfListByEnumerator().Skip(count);
        public static ValueEnumerable<T, SkipWhileNode<T, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>> SkipWhile<T>(this List<T> source, Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfListByEnumerator().SkipWhile(predicate);
        }
        public static ValueEnumerable<T, SkipWhileIdxNode<T, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>> SkipWhile<T>(this List<T> source, Func<T, int, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.OfListByEnumerator().SkipWhileIdx(predicate);
        }
        public static ValueEnumerable<T, TakeNode<T, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>> Take<T>(this List<T> source, int count) => source.OfListByEnumerator().Take(count);

        // -- Value based select

        public static ValueEnumerable<U, ValueSelectNode<T, U, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>, IFunc>> Select<T, U, IFunc>(this List<T> prior, IFunc selector, U u = default)
            where IFunc : IFunc<T, U> => prior.OfListByEnumerator().Select(selector, u);

        public static ValueEnumerable<T, ValueWhereNode<T, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>, Predicate>> Where<T, Predicate>(this List<T> inner, Predicate predicate)
            where Predicate : IFunc<T, bool>
            => inner.OfListByEnumerator().Where(predicate);
    }
}