using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

using AverageDecimal = Cistern.ValueLinq.Aggregation.Average<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using AverageDouble  = Cistern.ValueLinq.Aggregation.Average<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using AverageFloat   = Cistern.ValueLinq.Aggregation.Average<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using AverageInt     = Cistern.ValueLinq.Aggregation.Average<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using AverageLong    = Cistern.ValueLinq.Aggregation.Average<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using AverageDecimalNullable = Cistern.ValueLinq.Aggregation.AverageNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using AverageDoubleNullable  = Cistern.ValueLinq.Aggregation.AverageNullable<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using AverageFloatNullable   = Cistern.ValueLinq.Aggregation.AverageNullable<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using AverageIntNullable     = Cistern.ValueLinq.Aggregation.AverageNullable<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using AverageLongNullable    = Cistern.ValueLinq.Aggregation.AverageNullable<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using MinDecimal = Cistern.ValueLinq.Aggregation.Min<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MinDouble  = Cistern.ValueLinq.Aggregation.Min<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using MinFloat   = Cistern.ValueLinq.Aggregation.Min<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using MinInt     = Cistern.ValueLinq.Aggregation.Min<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using MinLong    = Cistern.ValueLinq.Aggregation.Min<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using MinDecimalNullable = Cistern.ValueLinq.Aggregation.MinNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MinDoubleNullable  = Cistern.ValueLinq.Aggregation.MinNullable<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using MinFloatNullable   = Cistern.ValueLinq.Aggregation.MinNullable<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using MinIntNullable     = Cistern.ValueLinq.Aggregation.MinNullable<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using MinLongNullable    = Cistern.ValueLinq.Aggregation.MinNullable<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using MaxDecimal = Cistern.ValueLinq.Aggregation.Max<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MaxDouble  = Cistern.ValueLinq.Aggregation.Max<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using MaxFloat   = Cistern.ValueLinq.Aggregation.Max<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using MaxInt     = Cistern.ValueLinq.Aggregation.Max<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using MaxLong    = Cistern.ValueLinq.Aggregation.Max<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using MaxDecimalNullable = Cistern.ValueLinq.Aggregation.MaxNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MaxDoubleNullable  = Cistern.ValueLinq.Aggregation.MaxNullable<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using MaxFloatNullable   = Cistern.ValueLinq.Aggregation.MaxNullable<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using MaxIntNullable     = Cistern.ValueLinq.Aggregation.MaxNullable<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using MaxLongNullable    = Cistern.ValueLinq.Aggregation.MaxNullable<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using SumDecimal = Cistern.ValueLinq.Aggregation.Sum<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using SumDouble  = Cistern.ValueLinq.Aggregation.Sum<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using SumFloat   = Cistern.ValueLinq.Aggregation.Sum<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using SumInt     = Cistern.ValueLinq.Aggregation.Sum<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using SumLong    = Cistern.ValueLinq.Aggregation.Sum<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using SumDecimalNullable = Cistern.ValueLinq.Aggregation.SumNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using SumDoubleNullable  = Cistern.ValueLinq.Aggregation.SumNullable<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using SumFloatNullable   = Cistern.ValueLinq.Aggregation.SumNullable<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using SumIntNullable     = Cistern.ValueLinq.Aggregation.SumNullable<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using SumLongNullable    = Cistern.ValueLinq.Aggregation.SumNullable<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;
using System.Buffers;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        private static void GetCountInformation<Node>(this Node node, out CountInformation ci) where Node : INode => node.GetCountInformation(out ci);

        public static T Aggregate<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, T, T> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.Node.CreateObjectViaFastEnumerator<T, ReduceForward<T>>(new ReduceForward<T>(func));
        }

        public static void Foreach<T, Inner>(in this ValueEnumerable<T, Inner> source, Action<T> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            source.Node.CreateObjectViaFastEnumerator<T, ForeachForward<T>>(new ForeachForward<T>(func));
        }

        public static TAccumulate Aggregate<T, TAccumulate, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.Node.CreateObjectViaFastEnumerator<TAccumulate, FoldForward<T, TAccumulate>>(new FoldForward<T, TAccumulate>(func, seed));
        }

        public static TResult Aggregate<T, TAccumulate, TResult, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return resultSelector(source.Node.CreateObjectViaFastEnumerator<TAccumulate, FoldForward<T, TAccumulate>>(new FoldForward<T, TAccumulate>(func, seed)));
        }

        public static bool All<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Node.CreateObjectViaFastEnumerator<bool, All<T, FuncToIFunc<T, bool>>>(new All<T, FuncToIFunc<T, bool>>(new FuncToIFunc<T, bool>(predicate)));
        }

        public static bool All<T, Inner, Predicate>(in this ValueEnumerable<T, Inner> source, Predicate predicate)
            where Inner : INode<T>
            where Predicate : IFunc<T, bool>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Node.CreateObjectViaFastEnumerator<bool, All<T, Predicate>>(new All<T, Predicate>(predicate));
        }

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source)
            where Inner : INode<T>
            => source.Node.CreateObjectViaFastEnumerator<bool, Anything<T>>(new Anything<T>());

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Node.CreateObjectViaFastEnumerator<bool, Any<T>>(new Any<T>(predicate));
        }

        // -- 

        public static ValueEnumerable<U, SelectNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, U> selector)
            where TPrior : INode<T>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, SelectNode<T, U, TPrior>>(new SelectNode<T, U, TPrior>(in prior.Node, selector));
        }
        public static ValueEnumerable<U, Select_InNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, U> f)
            where TPrior : INode<T>
            => new ValueEnumerable<U, Select_InNode<T, U, TPrior>>(new Select_InNode<T, U, TPrior>(in prior.Node, f));

        public static ValueEnumerable<U, SelectIdxNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, U> selector)
            where TPrior : INode<T>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, SelectIdxNode<T, U, TPrior>>(new SelectIdxNode<T, U, TPrior>(in prior.Node, selector));
        }
        public static ValueEnumerable<T, WhereNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, WhereNode<T, TPrior>>(new WhereNode<T, TPrior>(in prior.Node, predicate));
        }
        public static ValueEnumerable<T, Where_InNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, bool> f)
            where TPrior : INode<T>
            => new ValueEnumerable<T, Where_InNode<T, TPrior>>(new Where_InNode<T, TPrior>(in prior.Node, f));

        public static ValueEnumerable<T, SkipNode<T, TPrior>> Skip<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, int count)
            where TPrior : INode<T>
            => new ValueEnumerable<T, SkipNode<T, TPrior>>(new SkipNode<T, TPrior>(in prior.Node, count));

        public static ValueEnumerable<T, SkipWhileNode<T, TPrior>> SkipWhile<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, SkipWhileNode<T, TPrior>>(new SkipWhileNode<T, TPrior>(in prior.Node, predicate));
        }
        public static ValueEnumerable<T, SkipWhileIdxNode<T, TPrior>> SkipWhileIdx<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, SkipWhileIdxNode<T, TPrior>>(new SkipWhileIdxNode<T, TPrior>(in prior.Node, predicate));
        }
        public static ValueEnumerable<T, TakeNode<T, TPrior>> Take<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, int count)
            where TPrior : INode<T>
                => new ValueEnumerable<T, TakeNode<T, TPrior>>(new TakeNode<T, TPrior>(in prior.Node, count));

        public static List<T> ToList<T, Inner>(in this ValueEnumerable<T, Inner> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            if (info.ActualLengthIsMaximumLength || info.MaximumLength <= 4)
            {
                if (info.MaximumLength == 0)
                    return new List<T>();

                return inner.Node.CreateObjectViaFastEnumerator<List<T>, ToListForward<T>>(new ToListForward<T>(info.ActualSize));
            }

            if (info.MaximumLength <= maybeMaxCountForStackBasedPath.GetValueOrDefault())
                return Nodes<List<T>>.Aggregation<Inner, ToListViaStack>(in inner.Node);

            if (!arrayPoolInfo.HasValue)
                return inner.Node.CreateObjectViaFastEnumerator<List<T>, ToListForward<T>>(new ToListForward<T>(null));

            return inner.Node.CreateObjectViaFastEnumerator<List<T>, ToListViaArrayPoolForward<T>>(new ToListViaArrayPoolForward<T>(arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers, null));
        }

        public static List<T> ToListUseSharedPool<T, Inner>(in this ValueEnumerable<T, Inner> inner, bool? cleanBuffers = null)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            return inner.Node.CreateObjectViaFastEnumerator<List<T>, ToListViaArrayPoolForward<T>>(new ToListViaArrayPoolForward<T>(ArrayPool<T>.Shared, cleanBuffers ?? !CachedTypeInfo<T>.IsPrimitive, info.ActualSize));
        }

        public static List<T> ToListUseStack<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => Nodes<List<T>>.Aggregation<Inner, ToListViaStack>(in inner.Node);

        public static T Last<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T> =>
            (inner.Node.CheckForOptimization<T, Optimizations.TryLast, (bool, T)>(new Optimizations.TryLast(), out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                _ => inner.Node.CreateObjectViaFastEnumerator<T, Last<T>>(new Last<T>())
            };

        public static T LastOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T> =>
            (inner.Node.CheckForOptimization<T, Optimizations.TryLast, (bool, T)>(new Optimizations.TryLast(), out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                (true, (false, _)) => default,
                _ => inner.Node.CreateObjectViaFastEnumerator<T, LastOrDefault<T>>(new LastOrDefault<T>())
            };

        public static T ElementAt<T, Inner>(in this ValueEnumerable<T, Inner> inner, int index)
            where Inner : INode<T>
        {
            inner.Node.GetCountInformation(out var countInfo);
            return inner.Node.CreateObjectViaFastEnumerator<T, ElementAt<T>>(new ElementAt<T>(index, countInfo.ActualSize));
        }

        public static T ElementAtOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, int index)
            where Inner : INode<T>
        {
            inner.Node.GetCountInformation(out var countInfo);
            return inner.Node.CreateObjectViaFastEnumerator<T, ElementAtOrDefault<T>>(new ElementAtOrDefault<T>(index, countInfo.ActualSize));
        }


        public static decimal Average<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal> => inner.Node.CreateObjectViaFastEnumerator<decimal, AverageDecimal>(new AverageDecimal(true));
        public static double  Average<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>  => inner.Node.CreateObjectViaFastEnumerator<double,  AverageDouble >(new AverageDouble(true));
        public static float   Average<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>   => inner.Node.CreateObjectViaFastEnumerator<float,   AverageFloat  >(new AverageFloat(true));
        public static double  Average<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>     => inner.Node.CreateObjectViaFastEnumerator<double,  AverageInt    >(new AverageInt(true));
        public static double  Average<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>    => inner.Node.CreateObjectViaFastEnumerator<double,  AverageLong   >(new AverageLong(true));

        public static decimal? Average<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => inner.Node.CreateObjectViaFastEnumerator<decimal?, AverageDecimalNullable>(new AverageDecimalNullable(true));
        public static double?  Average<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => inner.Node.CreateObjectViaFastEnumerator<double?,  AverageDoubleNullable >(new AverageDoubleNullable(true));
        public static float?   Average<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => inner.Node.CreateObjectViaFastEnumerator<float?,   AverageFloatNullable  >(new AverageFloatNullable(true));
        public static double?  Average<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => inner.Node.CreateObjectViaFastEnumerator<double?,     AverageIntNullable    >(new AverageIntNullable(true));
        public static double?  Average<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => inner.Node.CreateObjectViaFastEnumerator<double?,    AverageLongNullable   >(new AverageLongNullable(true));

        public static decimal Min<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => inner.Node.CreateObjectViaFastEnumerator<decimal, MinDecimal>(new MinDecimal(true));
        public static double  Min<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>   => inner.Node.CreateObjectViaFastEnumerator<double,  MinDouble >(new MinDouble(true));
        public static float   Min<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>    => inner.Node.CreateObjectViaFastEnumerator<float,   MinFloat  >(new MinFloat(true));
        public static int     Min<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>      => inner.Node.CreateObjectViaFastEnumerator<int,     MinInt    >(new MinInt(true));
        public static long    Min<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>     => inner.Node.CreateObjectViaFastEnumerator<long,    MinLong   >(new MinLong(true));
        public static T       Min<T, Inner>(in this ValueEnumerable<T,    Inner> inner) where Inner : INode<T>        => inner.Node.CreateObjectViaFastEnumerator<T,       Min<T>    >(new Min<T>(true));

        public static decimal? Min<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?>  => inner.Node.CreateObjectViaFastEnumerator<decimal?, MinDecimalNullable>(new MinDecimalNullable(true));
        public static double?  Min<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>   => inner.Node.CreateObjectViaFastEnumerator<double?,  MinDoubleNullable >(new MinDoubleNullable(true));
        public static float?   Min<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>    => inner.Node.CreateObjectViaFastEnumerator<float?,   MinFloatNullable  >(new MinFloatNullable(true));
        public static int?     Min<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>      => inner.Node.CreateObjectViaFastEnumerator<int?,     MinIntNullable    >(new MinIntNullable(true));
        public static long?    Min<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>     => inner.Node.CreateObjectViaFastEnumerator<long?,    MinLongNullable   >(new MinLongNullable(true));

        public static decimal Max<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => inner.Node.CreateObjectViaFastEnumerator<decimal, MaxDecimal>(new MaxDecimal(true));
        public static double  Max<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>   => inner.Node.CreateObjectViaFastEnumerator<double,  MaxDouble >(new MaxDouble(true));
        public static float   Max<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>    => inner.Node.CreateObjectViaFastEnumerator<float,   MaxFloat  >(new MaxFloat(true));
        public static int     Max<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>      => inner.Node.CreateObjectViaFastEnumerator<int,     MaxInt    >(new MaxInt(true));
        public static long    Max<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>     => inner.Node.CreateObjectViaFastEnumerator<long,    MaxLong   >(new MaxLong(true));
        public static T       Max<T, Inner>(in this ValueEnumerable<T,    Inner> inner) where Inner : INode<T>        => inner.Node.CreateObjectViaFastEnumerator<T,       Max<T>    >(new Max<T>(true));

        public static decimal? Max<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => inner.Node.CreateObjectViaFastEnumerator<decimal?, MaxDecimalNullable>(new MaxDecimalNullable(true));
        public static double?  Max<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => inner.Node.CreateObjectViaFastEnumerator<double?,  MaxDoubleNullable >(new MaxDoubleNullable(true));
        public static float?   Max<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => inner.Node.CreateObjectViaFastEnumerator<float?,   MaxFloatNullable  >(new MaxFloatNullable(true));
        public static int?     Max<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => inner.Node.CreateObjectViaFastEnumerator<int?,     MaxIntNullable    >(new MaxIntNullable(true));
        public static long?    Max<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => inner.Node.CreateObjectViaFastEnumerator<long?,    MaxLongNullable   >(new MaxLongNullable(true));

        public static decimal Sum<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal> => inner.Node.CreateObjectViaFastEnumerator<decimal, SumDecimal>(new SumDecimal(true));
        public static double  Sum<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>  => inner.Node.CreateObjectViaFastEnumerator<double,  SumDouble >(new SumDouble(true));
        public static float   Sum<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>   => inner.Node.CreateObjectViaFastEnumerator<float,   SumFloat  >(new SumFloat(true));
        public static int     Sum<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>     => inner.Node.CreateObjectViaFastEnumerator<int,     SumInt    >(new SumInt(true));
        public static long    Sum<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>    => inner.Node.CreateObjectViaFastEnumerator<long,    SumLong   >(new SumLong(true));

        public static decimal? Sum<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => inner.Node.CreateObjectViaFastEnumerator<decimal, SumDecimalNullable>(new SumDecimalNullable(true));
        public static double?  Sum<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => inner.Node.CreateObjectViaFastEnumerator<double,  SumDoubleNullable >(new SumDoubleNullable(true));
        public static float?   Sum<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => inner.Node.CreateObjectViaFastEnumerator<float,   SumFloatNullable  >(new SumFloatNullable(true));
        public static int?     Sum<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => inner.Node.CreateObjectViaFastEnumerator<int,     SumIntNullable    >(new SumIntNullable(true));
        public static long?    Sum<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => inner.Node.CreateObjectViaFastEnumerator<long,    SumLongNullable   >(new SumLongNullable(true));

        public static int Count<T, Inner>(in this ValueEnumerable<T, Inner> inner, bool ignorePotentialSideEffects = false)
            where Inner : INode<T> =>
            Enumerable.Count<T, Inner>(in inner.Node, ignorePotentialSideEffects);

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>> SelectMany<TSource, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>>(new SelectManyNode<TSource, TResult, NodeT, NodeU>(in prior.Node, selector));
        }

        public static ValueEnumerable<T, ConcatNode<T, First, Second>> Concat<T, First, Second>(in this ValueEnumerable<T, First> first, in ValueEnumerable<T, Second> second)
            where First : INode<T>
            where Second : INode<T>
            => new ValueEnumerable<T, ConcatNode<T, First, Second>>(new ConcatNode<T, First, Second>(first.Node, second.Node));

        // -- Value based operators

        public static ValueEnumerable<U, ValueSelectNode<T, U, TPrior, IFunc>> Select<T, U, TPrior, IFunc>(in this ValueEnumerable<T, TPrior> prior, IFunc selector, U u = default)
            where TPrior : INode<T>
            where IFunc : IFunc<T, U>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, ValueSelectNode<T, U, TPrior, IFunc>>(new ValueSelectNode<T, U, TPrior, IFunc>(in prior.Node, selector));
        }
        public static ValueEnumerable<T, ValueWhereNode<T, TPrior, Predicate>> Where<T, TPrior, Predicate>(in this ValueEnumerable<T, TPrior> prior, Predicate predicate)
            where TPrior : INode<T>
            where Predicate : IFunc<T, bool>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, ValueWhereNode<T, TPrior, Predicate>>(new ValueWhereNode<T, TPrior, Predicate>(in prior.Node, predicate));
        }
    }
}
