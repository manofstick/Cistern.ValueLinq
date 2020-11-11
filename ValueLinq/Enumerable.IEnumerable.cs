using Cistern.ValueLinq.Containers;
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

        public static ValueEnumerable<U, SelectNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, Func<T, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, Select_InNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, InFunc<T, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, SelectIdxNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, Func<T, int, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<T, WhereNode<T, EnumerableNode<T>>> Where<T>(this IEnumerable<T> inner, Func<T, bool> f) => inner.OfEnumerable().Where(f);

        public static ValueEnumerable<T, Where_InNode<T, EnumerableNode<T>>> Where<T>(this IEnumerable<T> inner, InFunc<T, bool> f) => inner.OfEnumerable().Where(f);

        public static TSource Last<TSource>(this IEnumerable<TSource> source) => source.OfEnumerable().Last();
        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) => source.OfEnumerable().LastOrDefault();

        public static List<T> ToList<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is INode
                ? source.OfEnumerable().ToList()
                : new List<T>(source);
        }

        public static decimal Average(this IEnumerable<decimal> inner) => inner.OfEnumerable().Average();
        public static double  Average(this IEnumerable<double>  inner) => inner.OfEnumerable().Average();
        public static double  Average(this IEnumerable<int>     inner) => inner.OfEnumerable().Average();
        public static float   Average(this IEnumerable<float>   inner) => inner.OfEnumerable().Average();
        public static double  Average(this IEnumerable<long>    inner) => inner.OfEnumerable().Average();

        public static decimal? Average(this IEnumerable<decimal?> inner) => inner.OfEnumerable().Average();
        public static double?  Average(this IEnumerable<double?>  inner) => inner.OfEnumerable().Average();
        public static double?  Average(this IEnumerable<int?>     inner) => inner.OfEnumerable().Average();
        public static float?   Average(this IEnumerable<float?>   inner) => inner.OfEnumerable().Average();
        public static double?  Average(this IEnumerable<long?>    inner) => inner.OfEnumerable().Average();

        public static decimal Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, decimal> selector) => inner.OfEnumerable().Select(selector).Average();
        public static double  Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, double > selector) => inner.OfEnumerable().Select(selector).Average();
        public static double  Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, int    > selector) => inner.OfEnumerable().Select(selector).Average();
        public static double  Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, float  > selector) => inner.OfEnumerable().Select(selector).Average();
        public static double  Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, long   > selector) => inner.OfEnumerable().Select(selector).Average();

        public static decimal? Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, decimal?> selector) => inner.OfEnumerable().Select(selector).Average();
        public static double?  Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, double? > selector) => inner.OfEnumerable().Select(selector).Average();
        public static double?  Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, int?    > selector) => inner.OfEnumerable().Select(selector).Average();
        public static float?   Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, float?  > selector) => inner.OfEnumerable().Select(selector).Average();
        public static double?  Average<TSource>(this IEnumerable<TSource> inner, Func<TSource, long?   > selector) => inner.OfEnumerable().Select(selector).Average();

        public static decimal Min(this IEnumerable<decimal> inner) => inner.OfEnumerable().Min();
        public static double  Min(this IEnumerable<double>  inner) => inner.OfEnumerable().Min();
        public static int     Min(this IEnumerable<int>     inner) => inner.OfEnumerable().Min();
        public static float   Min(this IEnumerable<float>   inner) => inner.OfEnumerable().Min();
        public static long    Min(this IEnumerable<long>    inner) => inner.OfEnumerable().Min();
        public static T       Min<T>(this IEnumerable<T>    inner) => inner.OfEnumerable().Min();

        public static decimal? Min(this IEnumerable<decimal?> inner) => inner.OfEnumerable().Min();
        public static double?  Min(this IEnumerable<double?>  inner) => inner.OfEnumerable().Min();
        public static int?     Min(this IEnumerable<int?>     inner) => inner.OfEnumerable().Min();
        public static float?   Min(this IEnumerable<float?>   inner) => inner.OfEnumerable().Min();
        public static long?    Min(this IEnumerable<long?>    inner) => inner.OfEnumerable().Min();

        public static decimal Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, decimal> selector) => inner.OfEnumerable().Select(selector).Min();
        public static double  Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, double > selector) => inner.OfEnumerable().Select(selector).Min();
        public static int     Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, int    > selector) => inner.OfEnumerable().Select(selector).Min();
        public static float   Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, float  > selector) => inner.OfEnumerable().Select(selector).Min();
        public static long    Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, long   > selector) => inner.OfEnumerable().Select(selector).Min();
        public static T       Min<TSource, T>(this IEnumerable<TSource> inner, Func<TSource, T> selector) => inner.OfEnumerable().Select(selector).Min();

        public static decimal? Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, decimal?> selector) => inner.OfEnumerable().Select(selector).Min();
        public static double?  Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, double? > selector) => inner.OfEnumerable().Select(selector).Min();
        public static int?     Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, int?    > selector) => inner.OfEnumerable().Select(selector).Min();
        public static float?   Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, float?  > selector) => inner.OfEnumerable().Select(selector).Min();
        public static long?    Min<TSource>(this IEnumerable<TSource> inner, Func<TSource, long?   > selector) => inner.OfEnumerable().Select(selector).Min();


        public static decimal Max(this IEnumerable<decimal> inner) => inner.OfEnumerable().Max();
        public static double  Max(this IEnumerable<double>  inner) => inner.OfEnumerable().Max();
        public static int     Max(this IEnumerable<int>     inner) => inner.OfEnumerable().Max();
        public static float   Max(this IEnumerable<float>   inner) => inner.OfEnumerable().Max();
        public static long    Max(this IEnumerable<long>    inner) => inner.OfEnumerable().Max();
        public static T       Max<T>(this IEnumerable<T>    inner) => inner.OfEnumerable().Max();

        public static decimal? Max(this IEnumerable<decimal?> inner) => inner.OfEnumerable().Max();
        public static double?  Max(this IEnumerable<double?>  inner) => inner.OfEnumerable().Max();
        public static int?     Max(this IEnumerable<int?>     inner) => inner.OfEnumerable().Max();
        public static float?   Max(this IEnumerable<float?>   inner) => inner.OfEnumerable().Max();
        public static long?    Max(this IEnumerable<long?>    inner) => inner.OfEnumerable().Max();

        public static decimal Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, decimal> selector) => inner.OfEnumerable().Select(selector).Max();
        public static double  Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, double > selector) => inner.OfEnumerable().Select(selector).Max();
        public static int     Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, int    > selector) => inner.OfEnumerable().Select(selector).Max();
        public static float   Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, float  > selector) => inner.OfEnumerable().Select(selector).Max();
        public static long    Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, long   > selector) => inner.OfEnumerable().Select(selector).Max();
        public static T       Max<TSource, T>(this IEnumerable<TSource> inner, Func<TSource, T> selector) => inner.OfEnumerable().Select(selector).Max();

        public static decimal? Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, decimal?> selector) => inner.OfEnumerable().Select(selector).Max();
        public static double?  Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, double? > selector) => inner.OfEnumerable().Select(selector).Max();
        public static int?     Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, int?    > selector) => inner.OfEnumerable().Select(selector).Max();
        public static float?   Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, float?  > selector) => inner.OfEnumerable().Select(selector).Max();
        public static long?    Max<TSource>(this IEnumerable<TSource> inner, Func<TSource, long?   > selector) => inner.OfEnumerable().Select(selector).Max();

        public static decimal Sum(this IEnumerable<decimal> inner) => inner.OfEnumerable().Sum();
        public static double  Sum(this IEnumerable<double>  inner) => inner.OfEnumerable().Sum();
        public static int     Sum(this IEnumerable<int>     inner) => inner.OfEnumerable().Sum();
        public static float   Sum(this IEnumerable<float>   inner) => inner.OfEnumerable().Sum();
        public static long    Sum(this IEnumerable<long>    inner) => inner.OfEnumerable().Sum();

        public static decimal? Sum(this IEnumerable<decimal?> inner) => inner.OfEnumerable().Sum();
        public static double?  Sum(this IEnumerable<double?>  inner) => inner.OfEnumerable().Sum();
        public static int?     Sum(this IEnumerable<int?>     inner) => inner.OfEnumerable().Sum();
        public static float?   Sum(this IEnumerable<float?>   inner) => inner.OfEnumerable().Sum();
        public static long?    Sum(this IEnumerable<long?>    inner) => inner.OfEnumerable().Sum();

        public static decimal Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, decimal> selector) => inner.OfEnumerable().Select(selector).Sum();
        public static double  Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, double > selector) => inner.OfEnumerable().Select(selector).Sum();
        public static int     Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, int    > selector) => inner.OfEnumerable().Select(selector).Sum();
        public static float   Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, float  > selector) => inner.OfEnumerable().Select(selector).Sum();
        public static long    Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, long   > selector) => inner.OfEnumerable().Select(selector).Sum();

        public static decimal? Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, decimal?> selector) => inner.OfEnumerable().Select(selector).Sum();
        public static double?  Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, double? > selector) => inner.OfEnumerable().Select(selector).Sum();
        public static int?     Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, int?    > selector) => inner.OfEnumerable().Select(selector).Sum();
        public static float?   Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, float?  > selector) => inner.OfEnumerable().Select(selector).Sum();
        public static long?    Sum<TSource>(this IEnumerable<TSource> inner, Func<TSource, long?   > selector) => inner.OfEnumerable().Select(selector).Sum();


        public static int Count<T>(this IEnumerable<T> inner, bool ignorePotentialSideEffects = false) => inner.OfEnumerable().Count(ignorePotentialSideEffects);

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

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) => source.OfEnumerable().ElementAt(index);
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
