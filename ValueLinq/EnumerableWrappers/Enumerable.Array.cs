using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource>(this TSource[] source, Func<TSource, TSource, TSource> func)
            => source.OfArray().Aggregate(func);

        public static TAccumulate Aggregate<TSource, TAccumulate>(this TSource[] source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => source.OfArray().Aggregate(seed, func);

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this TSource[] source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => source.OfArray().Aggregate(seed, func, resultSelector);

        public static bool All<TSource>(this TSource[] source, Func<TSource, bool> predicate)
            => source.OfArray().All(predicate);

        public static bool Any<TSource>(this TSource[] source) => source.Length > 0;

        public static bool Any<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source switch
            {
                ICollection<TSource> { Count: 0 } => false,
                IReadOnlyCollection<TSource> { Count: 0 } => false,
                _ => source.OfArray().Any(predicate),
            };
        }

        // --

        public static ValueEnumerable<U, SelectNode<T, U, ArrayNode<T>>> Select<T, U>(this T[] inner, Func<T, U> f) => inner.OfArray().Select(f);

        public static ValueEnumerable<U, Select_InNode<T, U, ArrayNode<T>>> Select<T, U>(this T[] inner, InFunc<T, U> f) => inner.OfArray().Select(f);

        public static ValueEnumerable<U, SelectIdxNode<T, U, ArrayNode<T>>> Select<T, U>(this T[] inner, Func<T, int, U> f) => inner.OfArray().Select(f);

        public static ValueEnumerable<T, WhereNode<T, ArrayNode<T>>> Where<T>(this T[] inner, Func<T, bool> f) => inner.OfArray().Where(f);

        public static ValueEnumerable<T, Where_InNode<T, ArrayNode<T>>> Where<T>(this T[] inner, InFunc<T, bool> f) => inner.OfArray().Where(f);

        public static TSource Last<TSource>(this TSource[] source) => source.OfArray().Last();
        public static TSource LastOrDefault<TSource>(this TSource[] source) => source.OfArray().LastOrDefault();

        public static List<T> ToList<T>(this T[] source) => source.OfArray().ToList();

        public static decimal Average(this decimal[] inner) => inner.OfArray().Average();
        public static double  Average(this double[]  inner) => inner.OfArray().Average();
        public static double  Average(this int[]     inner) => inner.OfArray().Average();
        public static float   Average(this float[]   inner) => inner.OfArray().Average();
        public static double  Average(this long[]    inner) => inner.OfArray().Average();

        public static decimal? Average(this decimal?[] inner) => inner.OfArray().Average();
        public static double?  Average(this double?[]  inner) => inner.OfArray().Average();
        public static double?  Average(this int?[]     inner) => inner.OfArray().Average();
        public static float?   Average(this float?[]   inner) => inner.OfArray().Average();
        public static double?  Average(this long?[]    inner) => inner.OfArray().Average();

        public static decimal Average<TSource>(this TSource[] inner, Func<TSource, decimal> selector) => inner.OfArray().Select(selector).Average();
        public static double  Average<TSource>(this TSource[] inner, Func<TSource, double > selector) => inner.OfArray().Select(selector).Average();
        public static double  Average<TSource>(this TSource[] inner, Func<TSource, int    > selector) => inner.OfArray().Select(selector).Average();
        public static double  Average<TSource>(this TSource[] inner, Func<TSource, float  > selector) => inner.OfArray().Select(selector).Average();
        public static double  Average<TSource>(this TSource[] inner, Func<TSource, long   > selector) => inner.OfArray().Select(selector).Average();

        public static decimal? Average<TSource>(this TSource[] inner, Func<TSource, decimal?> selector) => inner.OfArray().Select(selector).Average();
        public static double?  Average<TSource>(this TSource[] inner, Func<TSource, double? > selector) => inner.OfArray().Select(selector).Average();
        public static double?  Average<TSource>(this TSource[] inner, Func<TSource, int?    > selector) => inner.OfArray().Select(selector).Average();
        public static float?   Average<TSource>(this TSource[] inner, Func<TSource, float?  > selector) => inner.OfArray().Select(selector).Average();
        public static double?  Average<TSource>(this TSource[] inner, Func<TSource, long?   > selector) => inner.OfArray().Select(selector).Average();

        public static decimal Min(this decimal[] inner) => inner.OfArray().Min();
        public static double  Min(this double[]  inner) => inner.OfArray().Min();
        public static int     Min(this int[]     inner) => inner.OfArray().Min();
        public static float   Min(this float[]   inner) => inner.OfArray().Min();
        public static long    Min(this long[]    inner) => inner.OfArray().Min();
        public static T       Min<T>(this T[]    inner) => inner.OfArray().Min();

        public static decimal? Min(this decimal?[] inner) => inner.OfArray().Min();
        public static double?  Min(this double?[]  inner) => inner.OfArray().Min();
        public static int?     Min(this int?[]     inner) => inner.OfArray().Min();
        public static float?   Min(this float?[]   inner) => inner.OfArray().Min();
        public static long?    Min(this long?[]    inner) => inner.OfArray().Min();

        public static decimal Min<TSource>(this TSource[] inner, Func<TSource, decimal> selector) => inner.OfArray().Select(selector).Min();
        public static double  Min<TSource>(this TSource[] inner, Func<TSource, double > selector) => inner.OfArray().Select(selector).Min();
        public static int     Min<TSource>(this TSource[] inner, Func<TSource, int    > selector) => inner.OfArray().Select(selector).Min();
        public static float   Min<TSource>(this TSource[] inner, Func<TSource, float  > selector) => inner.OfArray().Select(selector).Min();
        public static long    Min<TSource>(this TSource[] inner, Func<TSource, long   > selector) => inner.OfArray().Select(selector).Min();
        public static T       Min<TSource, T>(this TSource[] inner, Func<TSource, T> selector) => inner.OfArray().Select(selector).Min();

        public static decimal? Min<TSource>(this TSource[] inner, Func<TSource, decimal?> selector) => inner.OfArray().Select(selector).Min();
        public static double?  Min<TSource>(this TSource[] inner, Func<TSource, double? > selector) => inner.OfArray().Select(selector).Min();
        public static int?     Min<TSource>(this TSource[] inner, Func<TSource, int?    > selector) => inner.OfArray().Select(selector).Min();
        public static float?   Min<TSource>(this TSource[] inner, Func<TSource, float?  > selector) => inner.OfArray().Select(selector).Min();
        public static long?    Min<TSource>(this TSource[] inner, Func<TSource, long?   > selector) => inner.OfArray().Select(selector).Min();


        public static decimal Max(this decimal[] inner) => inner.OfArray().Max();
        public static double  Max(this double[]  inner) => inner.OfArray().Max();
        public static int     Max(this int[]     inner) => inner.OfArray().Max();
        public static float   Max(this float[]   inner) => inner.OfArray().Max();
        public static long    Max(this long[]    inner) => inner.OfArray().Max();
        public static T       Max<T>(this T[]    inner) => inner.OfArray().Max();

        public static decimal? Max(this decimal?[] inner) => inner.OfArray().Max();
        public static double?  Max(this double?[]  inner) => inner.OfArray().Max();
        public static int?     Max(this int?[]     inner) => inner.OfArray().Max();
        public static float?   Max(this float?[]   inner) => inner.OfArray().Max();
        public static long?    Max(this long?[]    inner) => inner.OfArray().Max();

        public static decimal Max<TSource>(this TSource[] inner, Func<TSource, decimal> selector) => inner.OfArray().Select(selector).Max();
        public static double  Max<TSource>(this TSource[] inner, Func<TSource, double > selector) => inner.OfArray().Select(selector).Max();
        public static int     Max<TSource>(this TSource[] inner, Func<TSource, int    > selector) => inner.OfArray().Select(selector).Max();
        public static float   Max<TSource>(this TSource[] inner, Func<TSource, float  > selector) => inner.OfArray().Select(selector).Max();
        public static long    Max<TSource>(this TSource[] inner, Func<TSource, long   > selector) => inner.OfArray().Select(selector).Max();
        public static T       Max<TSource, T>(this TSource[] inner, Func<TSource, T> selector) => inner.OfArray().Select(selector).Max();

        public static decimal? Max<TSource>(this TSource[] inner, Func<TSource, decimal?> selector) => inner.OfArray().Select(selector).Max();
        public static double?  Max<TSource>(this TSource[] inner, Func<TSource, double? > selector) => inner.OfArray().Select(selector).Max();
        public static int?     Max<TSource>(this TSource[] inner, Func<TSource, int?    > selector) => inner.OfArray().Select(selector).Max();
        public static float?   Max<TSource>(this TSource[] inner, Func<TSource, float?  > selector) => inner.OfArray().Select(selector).Max();
        public static long?    Max<TSource>(this TSource[] inner, Func<TSource, long?   > selector) => inner.OfArray().Select(selector).Max();

        public static decimal Sum(this decimal[] inner) => inner.OfArray().Sum();
        public static double  Sum(this double[]  inner) => inner.OfArray().Sum();
        public static int     Sum(this int[]     inner) => inner.OfArray().Sum();
        public static float   Sum(this float[]   inner) => inner.OfArray().Sum();
        public static long    Sum(this long[]    inner) => inner.OfArray().Sum();

        public static decimal? Sum(this decimal?[] inner) => inner.OfArray().Sum();
        public static double?  Sum(this double?[]  inner) => inner.OfArray().Sum();
        public static int?     Sum(this int?[]     inner) => inner.OfArray().Sum();
        public static float?   Sum(this float?[]   inner) => inner.OfArray().Sum();
        public static long?    Sum(this long?[]    inner) => inner.OfArray().Sum();

        public static decimal Sum<TSource>(this TSource[] inner, Func<TSource, decimal> selector) => inner.OfArray().Select(selector).Sum();
        public static double  Sum<TSource>(this TSource[] inner, Func<TSource, double > selector) => inner.OfArray().Select(selector).Sum();
        public static int     Sum<TSource>(this TSource[] inner, Func<TSource, int    > selector) => inner.OfArray().Select(selector).Sum();
        public static float   Sum<TSource>(this TSource[] inner, Func<TSource, float  > selector) => inner.OfArray().Select(selector).Sum();
        public static long    Sum<TSource>(this TSource[] inner, Func<TSource, long   > selector) => inner.OfArray().Select(selector).Sum();

        public static decimal? Sum<TSource>(this TSource[] inner, Func<TSource, decimal?> selector) => inner.OfArray().Select(selector).Sum();
        public static double?  Sum<TSource>(this TSource[] inner, Func<TSource, double? > selector) => inner.OfArray().Select(selector).Sum();
        public static int?     Sum<TSource>(this TSource[] inner, Func<TSource, int?    > selector) => inner.OfArray().Select(selector).Sum();
        public static float?   Sum<TSource>(this TSource[] inner, Func<TSource, float?  > selector) => inner.OfArray().Select(selector).Sum();
        public static long?    Sum<TSource>(this TSource[] inner, Func<TSource, long?   > selector) => inner.OfArray().Select(selector).Sum();


        public static int Count<T>(this T[] inner) => inner.OfArray().Count();

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

        // -- Value based select

        public static ValueEnumerable<U, ValueSelectNode<T, U, ArrayNode<T>, IFunc>> Select<T, U, IFunc>(this T[] prior, IFunc selector, U u = default)
            where IFunc : IFunc<T, U> => prior.OfArray().Select(selector, u);

        public static ValueEnumerable<T, ValueWhereNode<T, ArrayNode<T>, Predicate>> Where<T, Predicate>(this T[] inner, Predicate predicate)
            where Predicate : IFunc<T, bool>
            => inner.OfArray().Where(predicate);
    }
}
