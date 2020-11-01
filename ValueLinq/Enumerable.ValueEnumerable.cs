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


namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static T Aggregate<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, T, T> func)
            where Inner : INode
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.Node.CreateObjectViaFastEnumerator<T, T, ReduceForward<T>>(new ReduceForward<T>(func));
        }

        public static TAccumulate Aggregate<T, TAccumulate, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : INode
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.Node.CreateObjectViaFastEnumerator<T, TAccumulate, FoldForward<T, TAccumulate>>(new FoldForward<T, TAccumulate>(func, seed));
        }

        public static TResult Aggregate<T, TAccumulate, TResult, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return resultSelector(source.Node.CreateObjectViaFastEnumerator<T, TAccumulate, FoldForward<T, TAccumulate>>(new FoldForward<T, TAccumulate>(func, seed)));
        }

        public static bool All<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Node.CreateObjectViaFastEnumerator<T, bool, All<T>>(new All<T>(predicate));
        }

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source)
            where Inner : INode
            => source.Node.CreateObjectViaFastEnumerator<T, bool, Anything<T>>(new Anything<T>());

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Node.CreateObjectViaFastEnumerator<T, bool, Any<T>>(new Any<T>(predicate));
        }

        // -- 

        public static ValueEnumerable<U, SelectNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, U> selector) where TPrior : INode
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, SelectNode<T, U, TPrior>>(new SelectNode<T, U, TPrior>(in prior.Node, selector));
        }
        public static ValueEnumerable<U, Select_InNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, U> f) where TPrior : INode => new ValueEnumerable<U, Select_InNode<T, U, TPrior>>(new Select_InNode<T, U, TPrior>(in prior.Node, f));
        public static ValueEnumerable<U, SelectiNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, U> selector) where TPrior : INode
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, SelectiNode<T, U, TPrior>>(new SelectiNode<T, U, TPrior>(in prior.Node, selector));
        }
        public static ValueEnumerable<T, WhereNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate) where TPrior : INode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, WhereNode<T, TPrior>>(new WhereNode<T, TPrior>(in prior.Node, predicate));
        }
        public static ValueEnumerable<T, Where_InNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, bool> f) where TPrior : INode => new ValueEnumerable<T, Where_InNode<T, TPrior>>(new Where_InNode<T, TPrior>(in prior.Node, f));

        public static ValueEnumerable<T, SkipNode<T, TPrior>> Skip<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, int count)
            where TPrior : INode
            => new ValueEnumerable<T, SkipNode<T, TPrior>>(new SkipNode<T, TPrior>(in prior.Node, count));
        public static ValueEnumerable<T, TakeNode<T, TPrior>> Take<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, int count)
            where TPrior : INode
                => new ValueEnumerable<T, TakeNode<T, TPrior>>(new TakeNode<T, TPrior>(in prior.Node, count));


        public static List<T> ToList<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode
            => inner.Node.CreateObjectViaFastEnumerator<T, List<T>, ToListForward<T>>(new ToListForward<T>());

        public static T Last<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode =>
            (inner.Node.CheckForOptimization<T, Optimizations.TryLast, (bool, T)>(new Optimizations.TryLast(), out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                _ => inner.Node.CreateObjectViaFastEnumerator<T, T, Last<T>>(new Last<T>())
            };

        public static T LastOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode =>
            (inner.Node.CheckForOptimization<T, Optimizations.TryLast, (bool, T)>(new Optimizations.TryLast(), out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                (true, (false, _)) => default,
                _ => inner.Node.CreateObjectViaFastEnumerator<T, T, LastOrDefault<T>>(new LastOrDefault<T>())
            };

        public static T ElementAt<T, Inner>(in this ValueEnumerable<T, Inner> inner, int index) where Inner : INode =>
            inner.Node.CreateObjectViaFastEnumerator<T, T, ElementAt<T>>(new ElementAt<T>(index));

        public static T ElementAtOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, int index) where Inner : INode =>
            inner.Node.CreateObjectViaFastEnumerator<T, T, ElementAtOrDefault<T>>(new ElementAtOrDefault<T>(index));


        public static decimal Average<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<decimal, decimal, AverageDecimal>(new AverageDecimal());
        public static double  Average<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<double,  double,  AverageDouble >(new AverageDouble());
        public static float   Average<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<float,   float,   AverageFloat  >(new AverageFloat());
        public static double  Average<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<int,     double,  AverageInt    >(new AverageInt());
        public static double  Average<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<long,    double,  AverageLong   >(new AverageLong());

        public static decimal? Average<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<decimal?, decimal?, AverageDecimalNullable>(new AverageDecimalNullable());
        public static double?  Average<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<double?,  double?,  AverageDoubleNullable >(new AverageDoubleNullable());
        public static float?   Average<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<float?,   float?,   AverageFloatNullable  >(new AverageFloatNullable());
        public static double?  Average<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<int?,     double?,     AverageIntNullable    >(new AverageIntNullable());
        public static double?  Average<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<long?,    double?,    AverageLongNullable   >(new AverageLongNullable());

        public static decimal Min<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<decimal, decimal, MinDecimal>(new MinDecimal());
        public static double  Min<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<double,  double,  MinDouble >(new MinDouble());
        public static float   Min<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<float,   float,   MinFloat  >(new MinFloat());
        public static int     Min<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<int,     int,     MinInt    >(new MinInt());
        public static long    Min<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<long,    long,    MinLong   >(new MinLong());
        public static T       Min<T, Inner>(in this ValueEnumerable<T,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<T,       T,       Min<T>    >(new Min<T>());

        public static decimal? Min<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<decimal?, decimal?, MinDecimalNullable>(new MinDecimalNullable());
        public static double?  Min<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<double?,  double?,  MinDoubleNullable >(new MinDoubleNullable());
        public static float?   Min<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<float?,   float?,   MinFloatNullable  >(new MinFloatNullable());
        public static int?     Min<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<int?,     int?,     MinIntNullable    >(new MinIntNullable());
        public static long?    Min<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<long?,    long?,    MinLongNullable   >(new MinLongNullable());

        public static decimal Max<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<decimal, decimal, MaxDecimal>(new MaxDecimal());
        public static double  Max<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<double,  double,  MaxDouble >(new MaxDouble());
        public static float   Max<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<float,   float,   MaxFloat  >(new MaxFloat());
        public static int     Max<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<int,     int,     MaxInt    >(new MaxInt());
        public static long    Max<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<long,    long,    MaxLong   >(new MaxLong());
        public static T       Max<T, Inner>(in this ValueEnumerable<T,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<T,       T,       Max<T>    >(new Max<T>());

        public static decimal? Max<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<decimal?, decimal?, MaxDecimalNullable>(new MaxDecimalNullable());
        public static double?  Max<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<double?,  double?,  MaxDoubleNullable >(new MaxDoubleNullable());
        public static float?   Max<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<float?,   float?,   MaxFloatNullable  >(new MaxFloatNullable());
        public static int?     Max<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<int?,     int?,     MaxIntNullable    >(new MaxIntNullable());
        public static long?    Max<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<long?,    long?,    MaxLongNullable   >(new MaxLongNullable());

        public static decimal Sum<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<decimal, decimal, SumDecimal>(new SumDecimal());
        public static double  Sum<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<double,  double,  SumDouble >(new SumDouble());
        public static float   Sum<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<float,   float,   SumFloat  >(new SumFloat());
        public static int     Sum<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<int,     int,     SumInt    >(new SumInt());
        public static long    Sum<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<long,    long,    SumLong   >(new SumLong());

        public static decimal? Sum<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<decimal?, decimal, SumDecimalNullable>(new SumDecimalNullable());
        public static double?  Sum<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<double?,  double,  SumDoubleNullable >(new SumDoubleNullable());
        public static float?   Sum<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<float?,   float,   SumFloatNullable  >(new SumFloatNullable());
        public static int?     Sum<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<int?,     int,     SumIntNullable    >(new SumIntNullable());
        public static long?    Sum<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode => inner.Node.CreateObjectViaFastEnumerator<long?,    long,    SumLongNullable   >(new SumLongNullable());

        public static int Count<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode => Enumerable.Count<T, Inner>(in inner.Node);

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>> SelectMany<TSource, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeT : INode
            where NodeU : INode
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>>(new SelectManyNode<TSource, TResult, NodeT, NodeU>(in prior.Node, selector));
        }

        public static ValueEnumerable<T, ConcatNode<T, First, Second>> Concat<T, First, Second>(in this ValueEnumerable<T, First> first, in ValueEnumerable<T, Second> second)
            where First : INode
            where Second : INode
            => new ValueEnumerable<T, ConcatNode<T, First, Second>>(new ConcatNode<T, First, Second>(first.Node, second.Node));

        // -- Value based operators

        public static ValueEnumerable<U, ValueSelectNode<T, U, TPrior, IFunc>> Select<T, U, TPrior, IFunc>(in this ValueEnumerable<T, TPrior> prior, IFunc selector, U u = default)
            where TPrior : INode
            where IFunc : IFunc<T, U>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, ValueSelectNode<T, U, TPrior, IFunc>>(new ValueSelectNode<T, U, TPrior, IFunc>(in prior.Node, selector));
        }
        public static ValueEnumerable<T, ValueWhereNode<T, TPrior, Predicate>> Where<T, TPrior, Predicate>(in this ValueEnumerable<T, TPrior> prior, Predicate predicate)
            where TPrior : INode
            where Predicate : IFunc<T, bool>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, ValueWhereNode<T, TPrior, Predicate>>(new ValueWhereNode<T, TPrior, Predicate>(in prior.Node, predicate));
        }
    }
}
