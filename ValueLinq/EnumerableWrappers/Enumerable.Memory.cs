using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

// TODO: Probably create template to create (List/Array/Memory) forwards

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, TSource, TSource> func)
            => source.OfMemory().Aggregate(func);

        public static TAccumulate Aggregate<TSource, TAccumulate>(this ReadOnlyMemory<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => source.OfMemory().Aggregate(seed, func);

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this ReadOnlyMemory<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => source.OfMemory().Aggregate(seed, func, resultSelector);

        public static bool All<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
            => source.OfMemory().All(predicate);

        public static bool Any<TSource>(this ReadOnlyMemory<TSource> source)
            => source switch
            {
                System.Collections.ICollection c => c.Count > 0,
                ICollection<TSource> c => c.Count > 0,
                IReadOnlyCollection<TSource> c => c.Count > 0,
                _ => source.OfMemory().Any()
            };

        public static bool Any<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source switch
            {
                ICollection<TSource> { Count: 0 } => false,
                IReadOnlyCollection<TSource> { Count: 0 } => false,
                _ => source.OfMemory().Any(predicate),
            };
        }

        // --

        public static ValueEnumerable<U, SelectNode<T, U, MemoryNode<T>>> Select<T, U>(this ReadOnlyMemory<T> inner, Func<T, U> f) => inner.OfMemory().Select(f);

        public static ValueEnumerable<U, Select_InNode<T, U, MemoryNode<T>>> Select<T, U>(this ReadOnlyMemory<T> inner, InFunc<T, U> f) => inner.OfMemory().Select(f);

        public static ValueEnumerable<U, SelectIdxNode<T, U, MemoryNode<T>>> Select<T, U>(this ReadOnlyMemory<T> inner, Func<T, int, U> f) => inner.OfMemory().Select(f);

        public static ValueEnumerable<T, WhereNode<T, MemoryNode<T>>> Where<T>(this ReadOnlyMemory<T> inner, Func<T, bool> f) => inner.OfMemory().Where(f);

        public static ValueEnumerable<T, Where_InNode<T, MemoryNode<T>>> Where<T>(this ReadOnlyMemory<T> inner, InFunc<T, bool> f) => inner.OfMemory().Where(f);

        public static TSource Last<TSource>(this ReadOnlyMemory<TSource> source) => source.OfMemory().Last();
        public static TSource LastOrDefault<TSource>(this ReadOnlyMemory<TSource> source) => source.OfMemory().LastOrDefault();

        public static List<T> ToList<T>(this ReadOnlyMemory<T> source) => source.OfMemory().ToList();

        public static decimal Average(this ReadOnlyMemory<decimal> inner) => inner.OfMemory().Average();
        public static double  Average(this ReadOnlyMemory<double>  inner) => inner.OfMemory().Average();
        public static double  Average(this ReadOnlyMemory<int>     inner) => inner.OfMemory().Average();
        public static float   Average(this ReadOnlyMemory<float>   inner) => inner.OfMemory().Average();
        public static double  Average(this ReadOnlyMemory<long>    inner) => inner.OfMemory().Average();

        public static decimal? Average(this ReadOnlyMemory<decimal?> inner) => inner.OfMemory().Average();
        public static double?  Average(this ReadOnlyMemory<double?>  inner) => inner.OfMemory().Average();
        public static double?  Average(this ReadOnlyMemory<int?>     inner) => inner.OfMemory().Average();
        public static float?   Average(this ReadOnlyMemory<float?>   inner) => inner.OfMemory().Average();
        public static double?  Average(this ReadOnlyMemory<long?>    inner) => inner.OfMemory().Average();

        public static decimal Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, decimal> selector) => inner.OfMemory().Select(selector).Average();
        public static double  Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, double > selector) => inner.OfMemory().Select(selector).Average();
        public static double  Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, int    > selector) => inner.OfMemory().Select(selector).Average();
        public static double  Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, float  > selector) => inner.OfMemory().Select(selector).Average();
        public static double  Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, long   > selector) => inner.OfMemory().Select(selector).Average();

        public static decimal? Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, decimal?> selector) => inner.OfMemory().Select(selector).Average();
        public static double?  Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, double? > selector) => inner.OfMemory().Select(selector).Average();
        public static double?  Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, int?    > selector) => inner.OfMemory().Select(selector).Average();
        public static float?   Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, float?  > selector) => inner.OfMemory().Select(selector).Average();
        public static double?  Average<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, long?   > selector) => inner.OfMemory().Select(selector).Average();

        public static decimal Min(this ReadOnlyMemory<decimal> inner) => inner.OfMemory().Min();
        public static double  Min(this ReadOnlyMemory<double>  inner) => inner.OfMemory().Min();
        public static int     Min(this ReadOnlyMemory<int>     inner) => inner.OfMemory().Min();
        public static float   Min(this ReadOnlyMemory<float>   inner) => inner.OfMemory().Min();
        public static long    Min(this ReadOnlyMemory<long>    inner) => inner.OfMemory().Min();
        public static T       Min<T>(this ReadOnlyMemory<T>    inner) => inner.OfMemory().Min();

        public static decimal? Min(this ReadOnlyMemory<decimal?> inner) => inner.OfMemory().Min();
        public static double?  Min(this ReadOnlyMemory<double?>  inner) => inner.OfMemory().Min();
        public static int?     Min(this ReadOnlyMemory<int?>     inner) => inner.OfMemory().Min();
        public static float?   Min(this ReadOnlyMemory<float?>   inner) => inner.OfMemory().Min();
        public static long?    Min(this ReadOnlyMemory<long?>    inner) => inner.OfMemory().Min();

        public static decimal Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, decimal> selector) => inner.OfMemory().Select(selector).Min();
        public static double  Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, double > selector) => inner.OfMemory().Select(selector).Min();
        public static int     Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, int    > selector) => inner.OfMemory().Select(selector).Min();
        public static float   Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, float  > selector) => inner.OfMemory().Select(selector).Min();
        public static long    Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, long   > selector) => inner.OfMemory().Select(selector).Min();
        public static T       Min<TSource, T>(this ReadOnlyMemory<TSource> inner, Func<TSource, T> selector) => inner.OfMemory().Select(selector).Min();

        public static decimal? Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, decimal?> selector) => inner.OfMemory().Select(selector).Min();
        public static double?  Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, double? > selector) => inner.OfMemory().Select(selector).Min();
        public static int?     Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, int?    > selector) => inner.OfMemory().Select(selector).Min();
        public static float?   Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, float?  > selector) => inner.OfMemory().Select(selector).Min();
        public static long?    Min<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, long?   > selector) => inner.OfMemory().Select(selector).Min();


        public static decimal Max(this ReadOnlyMemory<decimal> inner) => inner.OfMemory().Max();
        public static double  Max(this ReadOnlyMemory<double>  inner) => inner.OfMemory().Max();
        public static int     Max(this ReadOnlyMemory<int>     inner) => inner.OfMemory().Max();
        public static float   Max(this ReadOnlyMemory<float>   inner) => inner.OfMemory().Max();
        public static long    Max(this ReadOnlyMemory<long>    inner) => inner.OfMemory().Max();
        public static T       Max<T>(this ReadOnlyMemory<T>    inner) => inner.OfMemory().Max();

        public static decimal? Max(this ReadOnlyMemory<decimal?> inner) => inner.OfMemory().Max();
        public static double?  Max(this ReadOnlyMemory<double?>  inner) => inner.OfMemory().Max();
        public static int?     Max(this ReadOnlyMemory<int?>     inner) => inner.OfMemory().Max();
        public static float?   Max(this ReadOnlyMemory<float?>   inner) => inner.OfMemory().Max();
        public static long?    Max(this ReadOnlyMemory<long?>    inner) => inner.OfMemory().Max();

        public static decimal Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, decimal> selector) => inner.OfMemory().Select(selector).Max();
        public static double  Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, double > selector) => inner.OfMemory().Select(selector).Max();
        public static int     Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, int    > selector) => inner.OfMemory().Select(selector).Max();
        public static float   Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, float  > selector) => inner.OfMemory().Select(selector).Max();
        public static long    Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, long   > selector) => inner.OfMemory().Select(selector).Max();
        public static T       Max<TSource, T>(this ReadOnlyMemory<TSource> inner, Func<TSource, T> selector) => inner.OfMemory().Select(selector).Max();

        public static decimal? Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, decimal?> selector) => inner.OfMemory().Select(selector).Max();
        public static double?  Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, double? > selector) => inner.OfMemory().Select(selector).Max();
        public static int?     Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, int?    > selector) => inner.OfMemory().Select(selector).Max();
        public static float?   Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, float?  > selector) => inner.OfMemory().Select(selector).Max();
        public static long?    Max<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, long?   > selector) => inner.OfMemory().Select(selector).Max();

        public static decimal Sum(this ReadOnlyMemory<decimal> inner) => inner.OfMemory().Sum();
        public static double  Sum(this ReadOnlyMemory<double>  inner) => inner.OfMemory().Sum();
        public static int     Sum(this ReadOnlyMemory<int>     inner) => inner.OfMemory().Sum();
        public static float   Sum(this ReadOnlyMemory<float>   inner) => inner.OfMemory().Sum();
        public static long    Sum(this ReadOnlyMemory<long>    inner) => inner.OfMemory().Sum();

        public static decimal? Sum(this ReadOnlyMemory<decimal?> inner) => inner.OfMemory().Sum();
        public static double?  Sum(this ReadOnlyMemory<double?>  inner) => inner.OfMemory().Sum();
        public static int?     Sum(this ReadOnlyMemory<int?>     inner) => inner.OfMemory().Sum();
        public static float?   Sum(this ReadOnlyMemory<float?>   inner) => inner.OfMemory().Sum();
        public static long?    Sum(this ReadOnlyMemory<long?>    inner) => inner.OfMemory().Sum();

        public static decimal Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, decimal> selector) => inner.OfMemory().Select(selector).Sum();
        public static double  Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, double > selector) => inner.OfMemory().Select(selector).Sum();
        public static int     Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, int    > selector) => inner.OfMemory().Select(selector).Sum();
        public static float   Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, float  > selector) => inner.OfMemory().Select(selector).Sum();
        public static long    Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, long   > selector) => inner.OfMemory().Select(selector).Sum();

        public static decimal? Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, decimal?> selector) => inner.OfMemory().Select(selector).Sum();
        public static double?  Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, double? > selector) => inner.OfMemory().Select(selector).Sum();
        public static int?     Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, int?    > selector) => inner.OfMemory().Select(selector).Sum();
        public static float?   Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, float?  > selector) => inner.OfMemory().Select(selector).Sum();
        public static long?    Sum<TSource>(this ReadOnlyMemory<TSource> inner, Func<TSource, long?   > selector) => inner.OfMemory().Select(selector).Sum();


        public static int Count<T>(this ReadOnlyMemory<T> inner) => inner.OfMemory().Count();

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

        // -- Value based select

        public static ValueEnumerable<U, ValueSelectNode<T, U, MemoryNode<T>, IFunc>> Select<T, U, IFunc>(this ReadOnlyMemory<T> prior, IFunc selector, U u = default)
            where IFunc : IFunc<T, U> => prior.OfMemory().Select(selector, u);

        public static ValueEnumerable<T, ValueWhereNode<T, MemoryNode<T>, Predicate>> Where<T, Predicate>(this ReadOnlyMemory<T> inner, Predicate predicate)
            where Predicate : IFunc<T, bool>
            => inner.OfMemory().Where(predicate);
    }
}
