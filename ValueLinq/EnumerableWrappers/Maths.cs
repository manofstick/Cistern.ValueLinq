using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using System;
using System.Collections.Generic;

using AverageDecimal = Cistern.ValueLinq.Aggregation.Average<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using AverageDecimalNullable = Cistern.ValueLinq.Aggregation.AverageNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using AverageDouble = Cistern.ValueLinq.Aggregation.Average<double, double, double, Cistern.ValueLinq.Maths.OpsDouble>;
using AverageDoubleNullable = Cistern.ValueLinq.Aggregation.AverageNullable<double, double, double, Cistern.ValueLinq.Maths.OpsDouble>;
using AverageFloat = Cistern.ValueLinq.Aggregation.Average<float, double, float, Cistern.ValueLinq.Maths.OpsFloat>;
using AverageFloatNullable = Cistern.ValueLinq.Aggregation.AverageNullable<float, double, float, Cistern.ValueLinq.Maths.OpsFloat>;
using AverageInt = Cistern.ValueLinq.Aggregation.Average<int, int, double, Cistern.ValueLinq.Maths.OpsInt>;
using AverageIntNullable = Cistern.ValueLinq.Aggregation.AverageNullable<int, int, double, Cistern.ValueLinq.Maths.OpsInt>;
using AverageLong = Cistern.ValueLinq.Aggregation.Average<long, long, double, Cistern.ValueLinq.Maths.OpsLong>;
using AverageLongNullable = Cistern.ValueLinq.Aggregation.AverageNullable<long, long, double, Cistern.ValueLinq.Maths.OpsLong>;
using MinDecimal = Cistern.ValueLinq.Aggregation.Min<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MinDecimalNullable = Cistern.ValueLinq.Aggregation.MinNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MinDouble = Cistern.ValueLinq.Aggregation.Min<double, double, double, Cistern.ValueLinq.Maths.OpsDouble>;
using MinDoubleNullable = Cistern.ValueLinq.Aggregation.MinNullable<double, double, double, Cistern.ValueLinq.Maths.OpsDouble>;
using MinFloat = Cistern.ValueLinq.Aggregation.Min<float, double, float, Cistern.ValueLinq.Maths.OpsFloat>;
using MinFloatNullable = Cistern.ValueLinq.Aggregation.MinNullable<float, double, float, Cistern.ValueLinq.Maths.OpsFloat>;
using MinInt = Cistern.ValueLinq.Aggregation.Min<int, int, double, Cistern.ValueLinq.Maths.OpsInt>;
using MinIntNullable = Cistern.ValueLinq.Aggregation.MinNullable<int, int, double, Cistern.ValueLinq.Maths.OpsInt>;
using MinLong = Cistern.ValueLinq.Aggregation.Min<long, long, double, Cistern.ValueLinq.Maths.OpsLong>;
using MinLongNullable = Cistern.ValueLinq.Aggregation.MinNullable<long, long, double, Cistern.ValueLinq.Maths.OpsLong>;
using MaxDecimal = Cistern.ValueLinq.Aggregation.Max<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MaxDecimalNullable = Cistern.ValueLinq.Aggregation.MaxNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MaxDouble = Cistern.ValueLinq.Aggregation.Max<double, double, double, Cistern.ValueLinq.Maths.OpsDouble>;
using MaxDoubleNullable = Cistern.ValueLinq.Aggregation.MaxNullable<double, double, double, Cistern.ValueLinq.Maths.OpsDouble>;
using MaxFloat = Cistern.ValueLinq.Aggregation.Max<float, double, float, Cistern.ValueLinq.Maths.OpsFloat>;
using MaxFloatNullable = Cistern.ValueLinq.Aggregation.MaxNullable<float, double, float, Cistern.ValueLinq.Maths.OpsFloat>;
using MaxInt = Cistern.ValueLinq.Aggregation.Max<int, int, double, Cistern.ValueLinq.Maths.OpsInt>;
using MaxIntNullable = Cistern.ValueLinq.Aggregation.MaxNullable<int, int, double, Cistern.ValueLinq.Maths.OpsInt>;
using MaxLong = Cistern.ValueLinq.Aggregation.Max<long, long, double, Cistern.ValueLinq.Maths.OpsLong>;
using MaxLongNullable = Cistern.ValueLinq.Aggregation.MaxNullable<long, long, double, Cistern.ValueLinq.Maths.OpsLong>;
using SumDecimal = Cistern.ValueLinq.Aggregation.Sum<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using SumDecimalNullable = Cistern.ValueLinq.Aggregation.SumNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using SumDouble = Cistern.ValueLinq.Aggregation.Sum<double, double, double, Cistern.ValueLinq.Maths.OpsDouble>;
using SumDoubleNullable = Cistern.ValueLinq.Aggregation.SumNullable<double, double, double, Cistern.ValueLinq.Maths.OpsDouble>;
using SumFloat = Cistern.ValueLinq.Aggregation.Sum<float, double, float, Cistern.ValueLinq.Maths.OpsFloat>;
using SumFloatNullable = Cistern.ValueLinq.Aggregation.SumNullable<float, double, float, Cistern.ValueLinq.Maths.OpsFloat>;
using SumInt = Cistern.ValueLinq.Aggregation.Sum<int, int, double, Cistern.ValueLinq.Maths.OpsInt>;
using SumIntNullable = Cistern.ValueLinq.Aggregation.SumNullable<int, int, double, Cistern.ValueLinq.Maths.OpsInt>;
using SumLong = Cistern.ValueLinq.Aggregation.Sum<long, long, double, Cistern.ValueLinq.Maths.OpsLong>;
using SumLongNullable = Cistern.ValueLinq.Aggregation.SumNullable<long, long, double, Cistern.ValueLinq.Maths.OpsLong>;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static decimal Average(this IEnumerable<decimal> source) =>
            EnumerableNode.FastEnumerateSwitch<decimal, decimal, AverageDecimal>(source, new AverageDecimal(true));

        public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, decimal, SelectFoward<TSource, decimal, AverageDecimal>>(source, new SelectFoward<TSource, decimal, AverageDecimal>(new AverageDecimal(true), selector));

        public static decimal? Average(this IEnumerable<decimal?> source) =>
            EnumerableNode.FastEnumerateSwitch<decimal?, decimal?, AverageDecimalNullable>(source, new AverageDecimalNullable(true));

        public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, decimal?, SelectFoward<TSource, decimal?, AverageDecimalNullable>>(source, new SelectFoward<TSource, decimal?, AverageDecimalNullable>(new AverageDecimalNullable(true), selector));

        public static double Average(this IEnumerable<double> source) =>
            EnumerableNode.FastEnumerateSwitch<double, double, AverageDouble>(source, new AverageDouble(true));

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double, SelectFoward<TSource, double, AverageDouble>>(source, new SelectFoward<TSource, double, AverageDouble>(new AverageDouble(true), selector));

        public static double? Average(this IEnumerable<double?> source) =>
            EnumerableNode.FastEnumerateSwitch<double?, double?, AverageDoubleNullable>(source, new AverageDoubleNullable(true));

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double?, SelectFoward<TSource, double?, AverageDoubleNullable>>(source, new SelectFoward<TSource, double?, AverageDoubleNullable>(new AverageDoubleNullable(true), selector));

        public static float Average(this IEnumerable<float> source) =>
            EnumerableNode.FastEnumerateSwitch<float, float, AverageFloat>(source, new AverageFloat(true));

        public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, float, SelectFoward<TSource, float, AverageFloat>>(source, new SelectFoward<TSource, float, AverageFloat>(new AverageFloat(true), selector));

        public static float? Average(this IEnumerable<float?> source) =>
            EnumerableNode.FastEnumerateSwitch<float?, float?, AverageFloatNullable>(source, new AverageFloatNullable(true));

        public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, float?, SelectFoward<TSource, float?, AverageFloatNullable>>(source, new SelectFoward<TSource, float?, AverageFloatNullable>(new AverageFloatNullable(true), selector));

        public static double Average(this IEnumerable<int> source) =>
            EnumerableNode.FastEnumerateSwitch<int, double, AverageInt>(source, new AverageInt(true));

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double, SelectFoward<TSource, int, AverageInt>>(source, new SelectFoward<TSource, int, AverageInt>(new AverageInt(true), selector));

        public static double? Average(this IEnumerable<int?> source) =>
            EnumerableNode.FastEnumerateSwitch<int?, double?, AverageIntNullable>(source, new AverageIntNullable(true));

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double?, SelectFoward<TSource, int?, AverageIntNullable>>(source, new SelectFoward<TSource, int?, AverageIntNullable>(new AverageIntNullable(true), selector));

        public static double Average(this IEnumerable<long> source) =>
            EnumerableNode.FastEnumerateSwitch<long, double, AverageLong>(source, new AverageLong(true));

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double, SelectFoward<TSource, long, AverageLong>>(source, new SelectFoward<TSource, long, AverageLong>(new AverageLong(true), selector));

        public static double? Average(this IEnumerable<long?> source) =>
            EnumerableNode.FastEnumerateSwitch<long?, double?, AverageLongNullable>(source, new AverageLongNullable(true));

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double?, SelectFoward<TSource, long?, AverageLongNullable>>(source, new SelectFoward<TSource, long?, AverageLongNullable>(new AverageLongNullable(true), selector));

        public static decimal Min(this IEnumerable<decimal> source) =>
            EnumerableNode.FastEnumerateSwitch<decimal, decimal, MinDecimal>(source, new MinDecimal(true));

        public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, decimal, SelectFoward<TSource, decimal, MinDecimal>>(source, new SelectFoward<TSource, decimal, MinDecimal>(new MinDecimal(true), selector));

        public static decimal? Min(this IEnumerable<decimal?> source) =>
            EnumerableNode.FastEnumerateSwitch<decimal?, decimal?, MinDecimalNullable>(source, new MinDecimalNullable(true));

        public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, decimal?, SelectFoward<TSource, decimal?, MinDecimalNullable>>(source, new SelectFoward<TSource, decimal?, MinDecimalNullable>(new MinDecimalNullable(true), selector));

        public static double Min(this IEnumerable<double> source) =>
            EnumerableNode.FastEnumerateSwitch<double, double, MinDouble>(source, new MinDouble(true));

        public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double, SelectFoward<TSource, double, MinDouble>>(source, new SelectFoward<TSource, double, MinDouble>(new MinDouble(true), selector));

        public static double? Min(this IEnumerable<double?> source) =>
            EnumerableNode.FastEnumerateSwitch<double?, double?, MinDoubleNullable>(source, new MinDoubleNullable(true));

        public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double?, SelectFoward<TSource, double?, MinDoubleNullable>>(source, new SelectFoward<TSource, double?, MinDoubleNullable>(new MinDoubleNullable(true), selector));

        public static float Min(this IEnumerable<float> source) =>
            EnumerableNode.FastEnumerateSwitch<float, float, MinFloat>(source, new MinFloat(true));

        public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, float, SelectFoward<TSource, float, MinFloat>>(source, new SelectFoward<TSource, float, MinFloat>(new MinFloat(true), selector));

        public static float? Min(this IEnumerable<float?> source) =>
            EnumerableNode.FastEnumerateSwitch<float?, float?, MinFloatNullable>(source, new MinFloatNullable(true));

        public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, float?, SelectFoward<TSource, float?, MinFloatNullable>>(source, new SelectFoward<TSource, float?, MinFloatNullable>(new MinFloatNullable(true), selector));

        public static int Min(this IEnumerable<int> source) =>
            EnumerableNode.FastEnumerateSwitch<int, int, MinInt>(source, new MinInt(true));

        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, int, SelectFoward<TSource, int, MinInt>>(source, new SelectFoward<TSource, int, MinInt>(new MinInt(true), selector));

        public static int? Min(this IEnumerable<int?> source) =>
            EnumerableNode.FastEnumerateSwitch<int?, int?, MinIntNullable>(source, new MinIntNullable(true));

        public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, int?, SelectFoward<TSource, int?, MinIntNullable>>(source, new SelectFoward<TSource, int?, MinIntNullable>(new MinIntNullable(true), selector));

        public static long Min(this IEnumerable<long> source) =>
            EnumerableNode.FastEnumerateSwitch<long, long, MinLong>(source, new MinLong(true));

        public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, long, SelectFoward<TSource, long, MinLong>>(source, new SelectFoward<TSource, long, MinLong>(new MinLong(true), selector));

        public static long? Min(this IEnumerable<long?> source) =>
            EnumerableNode.FastEnumerateSwitch<long?, long?, MinLongNullable>(source, new MinLongNullable(true));

        public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, long?, SelectFoward<TSource, long?, MinLongNullable>>(source, new SelectFoward<TSource, long?, MinLongNullable>(new MinLongNullable(true), selector));

        public static TSource Min<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, Min<TSource>>(source, new Min<TSource>(true));

        public static T Min<TSource, T>(this IEnumerable<TSource> source, Func<TSource, T> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, T, SelectFoward<TSource, T, Min<T>>>(source, new SelectFoward<TSource, T, Min<T>>(new Min<T>(true), selector));

        public static decimal Max(this IEnumerable<decimal> source) =>
            EnumerableNode.FastEnumerateSwitch<decimal, decimal, MaxDecimal>(source, new MaxDecimal(true));

        public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, decimal, SelectFoward<TSource, decimal, MaxDecimal>>(source, new SelectFoward<TSource, decimal, MaxDecimal>(new MaxDecimal(true), selector));

        public static decimal? Max(this IEnumerable<decimal?> source) =>
            EnumerableNode.FastEnumerateSwitch<decimal?, decimal?, MaxDecimalNullable>(source, new MaxDecimalNullable(true));

        public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, decimal?, SelectFoward<TSource, decimal?, MaxDecimalNullable>>(source, new SelectFoward<TSource, decimal?, MaxDecimalNullable>(new MaxDecimalNullable(true), selector));

        public static double Max(this IEnumerable<double> source) =>
            EnumerableNode.FastEnumerateSwitch<double, double, MaxDouble>(source, new MaxDouble(true));

        public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double, SelectFoward<TSource, double, MaxDouble>>(source, new SelectFoward<TSource, double, MaxDouble>(new MaxDouble(true), selector));

        public static double? Max(this IEnumerable<double?> source) =>
            EnumerableNode.FastEnumerateSwitch<double?, double?, MaxDoubleNullable>(source, new MaxDoubleNullable(true));

        public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double?, SelectFoward<TSource, double?, MaxDoubleNullable>>(source, new SelectFoward<TSource, double?, MaxDoubleNullable>(new MaxDoubleNullable(true), selector));

        public static float Max(this IEnumerable<float> source) =>
            EnumerableNode.FastEnumerateSwitch<float, float, MaxFloat>(source, new MaxFloat(true));

        public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, float, SelectFoward<TSource, float, MaxFloat>>(source, new SelectFoward<TSource, float, MaxFloat>(new MaxFloat(true), selector));

        public static float? Max(this IEnumerable<float?> source) =>
            EnumerableNode.FastEnumerateSwitch<float?, float?, MaxFloatNullable>(source, new MaxFloatNullable(true));

        public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, float?, SelectFoward<TSource, float?, MaxFloatNullable>>(source, new SelectFoward<TSource, float?, MaxFloatNullable>(new MaxFloatNullable(true), selector));

        public static int Max(this IEnumerable<int> source) =>
            EnumerableNode.FastEnumerateSwitch<int, int, MaxInt>(source, new MaxInt(true));

        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, int, SelectFoward<TSource, int, MaxInt>>(source, new SelectFoward<TSource, int, MaxInt>(new MaxInt(true), selector));

        public static int? Max(this IEnumerable<int?> source) =>
            EnumerableNode.FastEnumerateSwitch<int?, int?, MaxIntNullable>(source, new MaxIntNullable(true));

        public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, int?, SelectFoward<TSource, int?, MaxIntNullable>>(source, new SelectFoward<TSource, int?, MaxIntNullable>(new MaxIntNullable(true), selector));

        public static long Max(this IEnumerable<long> source) =>
            EnumerableNode.FastEnumerateSwitch<long, long, MaxLong>(source, new MaxLong(true));

        public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, long, SelectFoward<TSource, long, MaxLong>>(source, new SelectFoward<TSource, long, MaxLong>(new MaxLong(true), selector));

        public static long? Max(this IEnumerable<long?> source) =>
            EnumerableNode.FastEnumerateSwitch<long?, long?, MaxLongNullable>(source, new MaxLongNullable(true));

        public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, long?, SelectFoward<TSource, long?, MaxLongNullable>>(source, new SelectFoward<TSource, long?, MaxLongNullable>(new MaxLongNullable(true), selector));

        public static TSource Max<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, Max<TSource>>(source, new Max<TSource>(true));

        public static T Max<TSource, T>(this IEnumerable<TSource> source, Func<TSource, T> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, T, SelectFoward<TSource, T, Max<T>>>(source, new SelectFoward<TSource, T, Max<T>>(new Max<T>(true), selector));

        public static decimal Sum(this IEnumerable<decimal> source) =>
            EnumerableNode.FastEnumerateSwitch<decimal, decimal, SumDecimal>(source, new SumDecimal(true));

        public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, decimal, SelectFoward<TSource, decimal, SumDecimal>>(source, new SelectFoward<TSource, decimal, SumDecimal>(new SumDecimal(true), selector));

        public static decimal? Sum(this IEnumerable<decimal?> source) =>
            EnumerableNode.FastEnumerateSwitch<decimal?, decimal?, SumDecimalNullable>(source, new SumDecimalNullable(true));

        public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, decimal?, SelectFoward<TSource, decimal?, SumDecimalNullable>>(source, new SelectFoward<TSource, decimal?, SumDecimalNullable>(new SumDecimalNullable(true), selector));

        public static double Sum(this IEnumerable<double> source) =>
            EnumerableNode.FastEnumerateSwitch<double, double, SumDouble>(source, new SumDouble(true));

        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double, SelectFoward<TSource, double, SumDouble>>(source, new SelectFoward<TSource, double, SumDouble>(new SumDouble(true), selector));

        public static double? Sum(this IEnumerable<double?> source) =>
            EnumerableNode.FastEnumerateSwitch<double?, double?, SumDoubleNullable>(source, new SumDoubleNullable(true));

        public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, double?, SelectFoward<TSource, double?, SumDoubleNullable>>(source, new SelectFoward<TSource, double?, SumDoubleNullable>(new SumDoubleNullable(true), selector));

        public static float Sum(this IEnumerable<float> source) =>
            EnumerableNode.FastEnumerateSwitch<float, float, SumFloat>(source, new SumFloat(true));

        public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, float, SelectFoward<TSource, float, SumFloat>>(source, new SelectFoward<TSource, float, SumFloat>(new SumFloat(true), selector));

        public static float? Sum(this IEnumerable<float?> source) =>
            EnumerableNode.FastEnumerateSwitch<float?, float?, SumFloatNullable>(source, new SumFloatNullable(true));

        public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, float?, SelectFoward<TSource, float?, SumFloatNullable>>(source, new SelectFoward<TSource, float?, SumFloatNullable>(new SumFloatNullable(true), selector));

        public static int Sum(this IEnumerable<int> source) =>
            EnumerableNode.FastEnumerateSwitch<int, int, SumInt>(source, new SumInt(true));

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, int, SelectFoward<TSource, int, SumInt>>(source, new SelectFoward<TSource, int, SumInt>(new SumInt(true), selector));

        public static int? Sum(this IEnumerable<int?> source) =>
            EnumerableNode.FastEnumerateSwitch<int?, int?, SumIntNullable>(source, new SumIntNullable(true));

        public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, int?, SelectFoward<TSource, int?, SumIntNullable>>(source, new SelectFoward<TSource, int?, SumIntNullable>(new SumIntNullable(true), selector));

        public static long Sum(this IEnumerable<long> source) =>
            EnumerableNode.FastEnumerateSwitch<long, long, SumLong>(source, new SumLong(true));

        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, long, SelectFoward<TSource, long, SumLong>>(source, new SelectFoward<TSource, long, SumLong>(new SumLong(true), selector));

        public static long? Sum(this IEnumerable<long?> source) =>
            EnumerableNode.FastEnumerateSwitch<long?, long?, SumLongNullable>(source, new SumLongNullable(true));

        public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) =>
            EnumerableNode.FastEnumerateSwitch<TSource, long?, SelectFoward<TSource, long?, SumLongNullable>>(source, new SelectFoward<TSource, long?, SumLongNullable>(new SumLongNullable(true), selector));

    }

    public static partial class ValueLinqArray
    {
        public static decimal Average(this decimal[] source)
        {
            var aggregate = new AverageDecimal(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Average<TSource>(this TSource[] source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, AverageDecimal>(new AverageDecimal(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Average(this decimal?[] source)
        {
            var aggregate = new AverageDecimalNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Average<TSource>(this TSource[] source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, AverageDecimalNullable>(new AverageDecimalNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double Average(this double[] source)
        {
            var aggregate = new AverageDouble(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Average<TSource>(this TSource[] source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, AverageDouble>(new AverageDouble(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double? Average(this double?[] source)
        {
            var aggregate = new AverageDoubleNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Average<TSource>(this TSource[] source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, AverageDoubleNullable>(new AverageDoubleNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static float Average(this float[] source)
        {
            var aggregate = new AverageFloat(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Average<TSource>(this TSource[] source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, AverageFloat>(new AverageFloat(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static float? Average(this float?[] source)
        {
            var aggregate = new AverageFloatNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Average<TSource>(this TSource[] source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, AverageFloatNullable>(new AverageFloatNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double Average(this int[] source)
        {
            var aggregate = new AverageInt(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Average<TSource>(this TSource[] source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, AverageInt>(new AverageInt(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double? Average(this int?[] source)
        {
            var aggregate = new AverageIntNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Average<TSource>(this TSource[] source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, AverageIntNullable>(new AverageIntNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double Average(this long[] source)
        {
            var aggregate = new AverageLong(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Average<TSource>(this TSource[] source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, AverageLong>(new AverageLong(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double? Average(this long?[] source)
        {
            var aggregate = new AverageLongNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Average<TSource>(this TSource[] source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, AverageLongNullable>(new AverageLongNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static decimal Min(this decimal[] source)
        {
            var aggregate = new MinDecimal(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Min<TSource>(this TSource[] source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, MinDecimal>(new MinDecimal(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Min(this decimal?[] source)
        {
            var aggregate = new MinDecimalNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Min<TSource>(this TSource[] source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, MinDecimalNullable>(new MinDecimalNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double Min(this double[] source)
        {
            var aggregate = new MinDouble(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Min<TSource>(this TSource[] source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, MinDouble>(new MinDouble(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double? Min(this double?[] source)
        {
            var aggregate = new MinDoubleNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Min<TSource>(this TSource[] source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, MinDoubleNullable>(new MinDoubleNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static float Min(this float[] source)
        {
            var aggregate = new MinFloat(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Min<TSource>(this TSource[] source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, MinFloat>(new MinFloat(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static float? Min(this float?[] source)
        {
            var aggregate = new MinFloatNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Min<TSource>(this TSource[] source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, MinFloatNullable>(new MinFloatNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static int Min(this int[] source)
        {
            var aggregate = new MinInt(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Min<TSource>(this TSource[] source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, MinInt>(new MinInt(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static int? Min(this int?[] source)
        {
            var aggregate = new MinIntNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int? Min<TSource>(this TSource[] source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, MinIntNullable>(new MinIntNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static long Min(this long[] source)
        {
            var aggregate = new MinLong(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long Min<TSource>(this TSource[] source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, MinLong>(new MinLong(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static long? Min(this long?[] source)
        {
            var aggregate = new MinLongNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long? Min<TSource>(this TSource[] source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, MinLongNullable>(new MinLongNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static TSource Min<TSource>(this TSource[] source)
        {
            var aggregate = new Min<TSource>(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static T Min<TSource, T>(this TSource[] source, Func<TSource, T> selector)
        {
            var select = new SelectFoward<TSource, T, Min<T>>(new Min<T>(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }
        public static decimal Max(this decimal[] source)
        {
            var aggregate = new MaxDecimal(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Max<TSource>(this TSource[] source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, MaxDecimal>(new MaxDecimal(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Max(this decimal?[] source)
        {
            var aggregate = new MaxDecimalNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Max<TSource>(this TSource[] source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, MaxDecimalNullable>(new MaxDecimalNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double Max(this double[] source)
        {
            var aggregate = new MaxDouble(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Max<TSource>(this TSource[] source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, MaxDouble>(new MaxDouble(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double? Max(this double?[] source)
        {
            var aggregate = new MaxDoubleNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Max<TSource>(this TSource[] source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, MaxDoubleNullable>(new MaxDoubleNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static float Max(this float[] source)
        {
            var aggregate = new MaxFloat(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Max<TSource>(this TSource[] source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, MaxFloat>(new MaxFloat(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static float? Max(this float?[] source)
        {
            var aggregate = new MaxFloatNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Max<TSource>(this TSource[] source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, MaxFloatNullable>(new MaxFloatNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static int Max(this int[] source)
        {
            var aggregate = new MaxInt(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Max<TSource>(this TSource[] source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, MaxInt>(new MaxInt(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static int? Max(this int?[] source)
        {
            var aggregate = new MaxIntNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int? Max<TSource>(this TSource[] source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, MaxIntNullable>(new MaxIntNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static long Max(this long[] source)
        {
            var aggregate = new MaxLong(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long Max<TSource>(this TSource[] source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, MaxLong>(new MaxLong(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static long? Max(this long?[] source)
        {
            var aggregate = new MaxLongNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long? Max<TSource>(this TSource[] source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, MaxLongNullable>(new MaxLongNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static TSource Max<TSource>(this TSource[] source)
        {
            var aggregate = new Max<TSource>(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static T Max<TSource, T>(this TSource[] source, Func<TSource, T> selector)
        {
            var select = new SelectFoward<TSource, T, Max<T>>(new Max<T>(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }
        public static decimal Sum(this decimal[] source)
        {
            var aggregate = new SumDecimal(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Sum<TSource>(this TSource[] source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, SumDecimal>(new SumDecimal(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Sum(this decimal?[] source)
        {
            var aggregate = new SumDecimalNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Sum<TSource>(this TSource[] source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, SumDecimalNullable>(new SumDecimalNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double Sum(this double[] source)
        {
            var aggregate = new SumDouble(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Sum<TSource>(this TSource[] source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, SumDouble>(new SumDouble(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static double? Sum(this double?[] source)
        {
            var aggregate = new SumDoubleNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Sum<TSource>(this TSource[] source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, SumDoubleNullable>(new SumDoubleNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static float Sum(this float[] source)
        {
            var aggregate = new SumFloat(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Sum<TSource>(this TSource[] source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, SumFloat>(new SumFloat(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static float? Sum(this float?[] source)
        {
            var aggregate = new SumFloatNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Sum<TSource>(this TSource[] source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, SumFloatNullable>(new SumFloatNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static int Sum(this int[] source)
        {
            var aggregate = new SumInt(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Sum<TSource>(this TSource[] source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, SumInt>(new SumInt(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static int? Sum(this int?[] source)
        {
            var aggregate = new SumIntNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int? Sum<TSource>(this TSource[] source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, SumIntNullable>(new SumIntNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static long Sum(this long[] source)
        {
            var aggregate = new SumLong(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long Sum<TSource>(this TSource[] source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, SumLong>(new SumLong(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

        public static long? Sum(this long?[] source)
        {
            var aggregate = new SumLongNullable(true);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long? Sum<TSource>(this TSource[] source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, SumLongNullable>(new SumLongNullable(true), selector);
            ArrayNode.ProcessArray(source, ref select);
            return select._next.GetResult();
        }

    }

    public static partial class ValueLinqList
    {
        public static decimal Average(this List<decimal> source)
        {
            var aggregate = new AverageDecimal(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Average<TSource>(this List<TSource> source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, AverageDecimal>(new AverageDecimal(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Average(this List<decimal?> source)
        {
            var aggregate = new AverageDecimalNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Average<TSource>(this List<TSource> source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, AverageDecimalNullable>(new AverageDecimalNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double Average(this List<double> source)
        {
            var aggregate = new AverageDouble(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Average<TSource>(this List<TSource> source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, AverageDouble>(new AverageDouble(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double? Average(this List<double?> source)
        {
            var aggregate = new AverageDoubleNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Average<TSource>(this List<TSource> source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, AverageDoubleNullable>(new AverageDoubleNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static float Average(this List<float> source)
        {
            var aggregate = new AverageFloat(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Average<TSource>(this List<TSource> source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, AverageFloat>(new AverageFloat(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static float? Average(this List<float?> source)
        {
            var aggregate = new AverageFloatNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Average<TSource>(this List<TSource> source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, AverageFloatNullable>(new AverageFloatNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double Average(this List<int> source)
        {
            var aggregate = new AverageInt(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Average<TSource>(this List<TSource> source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, AverageInt>(new AverageInt(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double? Average(this List<int?> source)
        {
            var aggregate = new AverageIntNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Average<TSource>(this List<TSource> source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, AverageIntNullable>(new AverageIntNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double Average(this List<long> source)
        {
            var aggregate = new AverageLong(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Average<TSource>(this List<TSource> source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, AverageLong>(new AverageLong(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double? Average(this List<long?> source)
        {
            var aggregate = new AverageLongNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Average<TSource>(this List<TSource> source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, AverageLongNullable>(new AverageLongNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static decimal Min(this List<decimal> source)
        {
            var aggregate = new MinDecimal(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Min<TSource>(this List<TSource> source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, MinDecimal>(new MinDecimal(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Min(this List<decimal?> source)
        {
            var aggregate = new MinDecimalNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Min<TSource>(this List<TSource> source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, MinDecimalNullable>(new MinDecimalNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double Min(this List<double> source)
        {
            var aggregate = new MinDouble(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Min<TSource>(this List<TSource> source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, MinDouble>(new MinDouble(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double? Min(this List<double?> source)
        {
            var aggregate = new MinDoubleNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Min<TSource>(this List<TSource> source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, MinDoubleNullable>(new MinDoubleNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static float Min(this List<float> source)
        {
            var aggregate = new MinFloat(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Min<TSource>(this List<TSource> source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, MinFloat>(new MinFloat(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static float? Min(this List<float?> source)
        {
            var aggregate = new MinFloatNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Min<TSource>(this List<TSource> source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, MinFloatNullable>(new MinFloatNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static int Min(this List<int> source)
        {
            var aggregate = new MinInt(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Min<TSource>(this List<TSource> source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, MinInt>(new MinInt(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static int? Min(this List<int?> source)
        {
            var aggregate = new MinIntNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int? Min<TSource>(this List<TSource> source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, MinIntNullable>(new MinIntNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static long Min(this List<long> source)
        {
            var aggregate = new MinLong(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long Min<TSource>(this List<TSource> source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, MinLong>(new MinLong(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static long? Min(this List<long?> source)
        {
            var aggregate = new MinLongNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long? Min<TSource>(this List<TSource> source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, MinLongNullable>(new MinLongNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static TSource Min<TSource>(this List<TSource> source)
        {
            var aggregate = new Min<TSource>(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static T Min<TSource, T>(this List<TSource> source, Func<TSource, T> selector)
        {
            var select = new SelectFoward<TSource, T, Min<T>>(new Min<T>(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }
        public static decimal Max(this List<decimal> source)
        {
            var aggregate = new MaxDecimal(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Max<TSource>(this List<TSource> source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, MaxDecimal>(new MaxDecimal(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Max(this List<decimal?> source)
        {
            var aggregate = new MaxDecimalNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Max<TSource>(this List<TSource> source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, MaxDecimalNullable>(new MaxDecimalNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double Max(this List<double> source)
        {
            var aggregate = new MaxDouble(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Max<TSource>(this List<TSource> source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, MaxDouble>(new MaxDouble(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double? Max(this List<double?> source)
        {
            var aggregate = new MaxDoubleNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Max<TSource>(this List<TSource> source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, MaxDoubleNullable>(new MaxDoubleNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static float Max(this List<float> source)
        {
            var aggregate = new MaxFloat(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Max<TSource>(this List<TSource> source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, MaxFloat>(new MaxFloat(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static float? Max(this List<float?> source)
        {
            var aggregate = new MaxFloatNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Max<TSource>(this List<TSource> source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, MaxFloatNullable>(new MaxFloatNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static int Max(this List<int> source)
        {
            var aggregate = new MaxInt(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Max<TSource>(this List<TSource> source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, MaxInt>(new MaxInt(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static int? Max(this List<int?> source)
        {
            var aggregate = new MaxIntNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int? Max<TSource>(this List<TSource> source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, MaxIntNullable>(new MaxIntNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static long Max(this List<long> source)
        {
            var aggregate = new MaxLong(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long Max<TSource>(this List<TSource> source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, MaxLong>(new MaxLong(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static long? Max(this List<long?> source)
        {
            var aggregate = new MaxLongNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long? Max<TSource>(this List<TSource> source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, MaxLongNullable>(new MaxLongNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static TSource Max<TSource>(this List<TSource> source)
        {
            var aggregate = new Max<TSource>(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static T Max<TSource, T>(this List<TSource> source, Func<TSource, T> selector)
        {
            var select = new SelectFoward<TSource, T, Max<T>>(new Max<T>(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }
        public static decimal Sum(this List<decimal> source)
        {
            var aggregate = new SumDecimal(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Sum<TSource>(this List<TSource> source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, SumDecimal>(new SumDecimal(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Sum(this List<decimal?> source)
        {
            var aggregate = new SumDecimalNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Sum<TSource>(this List<TSource> source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, SumDecimalNullable>(new SumDecimalNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double Sum(this List<double> source)
        {
            var aggregate = new SumDouble(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Sum<TSource>(this List<TSource> source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, SumDouble>(new SumDouble(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static double? Sum(this List<double?> source)
        {
            var aggregate = new SumDoubleNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Sum<TSource>(this List<TSource> source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, SumDoubleNullable>(new SumDoubleNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static float Sum(this List<float> source)
        {
            var aggregate = new SumFloat(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Sum<TSource>(this List<TSource> source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, SumFloat>(new SumFloat(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static float? Sum(this List<float?> source)
        {
            var aggregate = new SumFloatNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Sum<TSource>(this List<TSource> source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, SumFloatNullable>(new SumFloatNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static int Sum(this List<int> source)
        {
            var aggregate = new SumInt(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Sum<TSource>(this List<TSource> source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, SumInt>(new SumInt(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static int? Sum(this List<int?> source)
        {
            var aggregate = new SumIntNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int? Sum<TSource>(this List<TSource> source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, SumIntNullable>(new SumIntNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static long Sum(this List<long> source)
        {
            var aggregate = new SumLong(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long Sum<TSource>(this List<TSource> source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, SumLong>(new SumLong(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

        public static long? Sum(this List<long?> source)
        {
            var aggregate = new SumLongNullable(true);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long? Sum<TSource>(this List<TSource> source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, SumLongNullable>(new SumLongNullable(true), selector);
            ListByIndexNode.ProcessList(source, ref select);
            return select._next.GetResult();
        }

    }

    public static partial class ValueLinqMemory
    {
        public static decimal Average(this ReadOnlyMemory<decimal> source)
        {
            var aggregate = new AverageDecimal(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, AverageDecimal>(new AverageDecimal(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Average(this ReadOnlyMemory<decimal?> source)
        {
            var aggregate = new AverageDecimalNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, AverageDecimalNullable>(new AverageDecimalNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double Average(this ReadOnlyMemory<double> source)
        {
            var aggregate = new AverageDouble(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, AverageDouble>(new AverageDouble(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double? Average(this ReadOnlyMemory<double?> source)
        {
            var aggregate = new AverageDoubleNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, AverageDoubleNullable>(new AverageDoubleNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static float Average(this ReadOnlyMemory<float> source)
        {
            var aggregate = new AverageFloat(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, AverageFloat>(new AverageFloat(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static float? Average(this ReadOnlyMemory<float?> source)
        {
            var aggregate = new AverageFloatNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, AverageFloatNullable>(new AverageFloatNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double Average(this ReadOnlyMemory<int> source)
        {
            var aggregate = new AverageInt(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, AverageInt>(new AverageInt(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double? Average(this ReadOnlyMemory<int?> source)
        {
            var aggregate = new AverageIntNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, AverageIntNullable>(new AverageIntNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double Average(this ReadOnlyMemory<long> source)
        {
            var aggregate = new AverageLong(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, AverageLong>(new AverageLong(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double? Average(this ReadOnlyMemory<long?> source)
        {
            var aggregate = new AverageLongNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Average<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, AverageLongNullable>(new AverageLongNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static decimal Min(this ReadOnlyMemory<decimal> source)
        {
            var aggregate = new MinDecimal(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, MinDecimal>(new MinDecimal(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Min(this ReadOnlyMemory<decimal?> source)
        {
            var aggregate = new MinDecimalNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, MinDecimalNullable>(new MinDecimalNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double Min(this ReadOnlyMemory<double> source)
        {
            var aggregate = new MinDouble(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, MinDouble>(new MinDouble(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double? Min(this ReadOnlyMemory<double?> source)
        {
            var aggregate = new MinDoubleNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, MinDoubleNullable>(new MinDoubleNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static float Min(this ReadOnlyMemory<float> source)
        {
            var aggregate = new MinFloat(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, MinFloat>(new MinFloat(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static float? Min(this ReadOnlyMemory<float?> source)
        {
            var aggregate = new MinFloatNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, MinFloatNullable>(new MinFloatNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static int Min(this ReadOnlyMemory<int> source)
        {
            var aggregate = new MinInt(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, MinInt>(new MinInt(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static int? Min(this ReadOnlyMemory<int?> source)
        {
            var aggregate = new MinIntNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int? Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, MinIntNullable>(new MinIntNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static long Min(this ReadOnlyMemory<long> source)
        {
            var aggregate = new MinLong(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, MinLong>(new MinLong(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static long? Min(this ReadOnlyMemory<long?> source)
        {
            var aggregate = new MinLongNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long? Min<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, MinLongNullable>(new MinLongNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static TSource Min<TSource>(this ReadOnlyMemory<TSource> source)
        {
            var aggregate = new Min<TSource>(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static T Min<TSource, T>(this ReadOnlyMemory<TSource> source, Func<TSource, T> selector)
        {
            var select = new SelectFoward<TSource, T, Min<T>>(new Min<T>(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }
        public static decimal Max(this ReadOnlyMemory<decimal> source)
        {
            var aggregate = new MaxDecimal(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, MaxDecimal>(new MaxDecimal(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Max(this ReadOnlyMemory<decimal?> source)
        {
            var aggregate = new MaxDecimalNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, MaxDecimalNullable>(new MaxDecimalNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double Max(this ReadOnlyMemory<double> source)
        {
            var aggregate = new MaxDouble(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, MaxDouble>(new MaxDouble(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double? Max(this ReadOnlyMemory<double?> source)
        {
            var aggregate = new MaxDoubleNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, MaxDoubleNullable>(new MaxDoubleNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static float Max(this ReadOnlyMemory<float> source)
        {
            var aggregate = new MaxFloat(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, MaxFloat>(new MaxFloat(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static float? Max(this ReadOnlyMemory<float?> source)
        {
            var aggregate = new MaxFloatNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, MaxFloatNullable>(new MaxFloatNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static int Max(this ReadOnlyMemory<int> source)
        {
            var aggregate = new MaxInt(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, MaxInt>(new MaxInt(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static int? Max(this ReadOnlyMemory<int?> source)
        {
            var aggregate = new MaxIntNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int? Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, MaxIntNullable>(new MaxIntNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static long Max(this ReadOnlyMemory<long> source)
        {
            var aggregate = new MaxLong(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, MaxLong>(new MaxLong(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static long? Max(this ReadOnlyMemory<long?> source)
        {
            var aggregate = new MaxLongNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long? Max<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, MaxLongNullable>(new MaxLongNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static TSource Max<TSource>(this ReadOnlyMemory<TSource> source)
        {
            var aggregate = new Max<TSource>(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static T Max<TSource, T>(this ReadOnlyMemory<TSource> source, Func<TSource, T> selector)
        {
            var select = new SelectFoward<TSource, T, Max<T>>(new Max<T>(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }
        public static decimal Sum(this ReadOnlyMemory<decimal> source)
        {
            var aggregate = new SumDecimal(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, decimal> selector)
        {
            var select = new SelectFoward<TSource, decimal, SumDecimal>(new SumDecimal(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static decimal? Sum(this ReadOnlyMemory<decimal?> source)
        {
            var aggregate = new SumDecimalNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static decimal? Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, decimal?> selector)
        {
            var select = new SelectFoward<TSource, decimal?, SumDecimalNullable>(new SumDecimalNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double Sum(this ReadOnlyMemory<double> source)
        {
            var aggregate = new SumDouble(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, double> selector)
        {
            var select = new SelectFoward<TSource, double, SumDouble>(new SumDouble(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static double? Sum(this ReadOnlyMemory<double?> source)
        {
            var aggregate = new SumDoubleNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static double? Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, double?> selector)
        {
            var select = new SelectFoward<TSource, double?, SumDoubleNullable>(new SumDoubleNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static float Sum(this ReadOnlyMemory<float> source)
        {
            var aggregate = new SumFloat(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, float> selector)
        {
            var select = new SelectFoward<TSource, float, SumFloat>(new SumFloat(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static float? Sum(this ReadOnlyMemory<float?> source)
        {
            var aggregate = new SumFloatNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static float? Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, float?> selector)
        {
            var select = new SelectFoward<TSource, float?, SumFloatNullable>(new SumFloatNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static int Sum(this ReadOnlyMemory<int> source)
        {
            var aggregate = new SumInt(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int> selector)
        {
            var select = new SelectFoward<TSource, int, SumInt>(new SumInt(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static int? Sum(this ReadOnlyMemory<int?> source)
        {
            var aggregate = new SumIntNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int? Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int?> selector)
        {
            var select = new SelectFoward<TSource, int?, SumIntNullable>(new SumIntNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static long Sum(this ReadOnlyMemory<long> source)
        {
            var aggregate = new SumLong(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, long> selector)
        {
            var select = new SelectFoward<TSource, long, SumLong>(new SumLong(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

        public static long? Sum(this ReadOnlyMemory<long?> source)
        {
            var aggregate = new SumLongNullable(true);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static long? Sum<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, long?> selector)
        {
            var select = new SelectFoward<TSource, long?, SumLongNullable>(new SumLongNullable(true), selector);
            MemoryNode.ProcessMemory(source, ref select);
            return select._next.GetResult();
        }

    }

}