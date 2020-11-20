﻿using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        //        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) => System.Linq.Enumerable.Aggregate<TSource>(source, func);
        //        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) => System.Linq.Enumerable.Aggregate<TSource, TAccumulate>(source, seed, func);
        //        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) => System.Linq.Enumerable.Aggregate<TSource, TAccumulate, TResult>(source, seed, func, resultSelector);
        //        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.All<TSource>(source, predicate);
        //        public static bool Any<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.Any<TSource>(source);
        //        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.Any<TSource>(source, predicate);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element) => System.Linq.Enumerable.Append<TSource>(source, element);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.AsEnumerable<TSource>(source);
        //        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => System.Linq.Enumerable.Average<TSource>(source, selector);
        //        public static float? Average(this IEnumerable<float?> source) => System.Linq.Enumerable.Average(source);
        //        public static decimal Average(this IEnumerable<decimal> source) => System.Linq.Enumerable.Average(source);
        //        public static double Average(this IEnumerable<int> source) => System.Linq.Enumerable.Average(source);
        //        public static double Average(this IEnumerable<long> source) => System.Linq.Enumerable.Average(source);
        //        public static double Average(this IEnumerable<double> source) => System.Linq.Enumerable.Average(source);
        //        public static double? Average(this IEnumerable<double?> source) => System.Linq.Enumerable.Average(source);
        //        public static double? Average(this IEnumerable<int?> source) => System.Linq.Enumerable.Average(source);
        //        public static double? Average(this IEnumerable<long?> source) => System.Linq.Enumerable.Average(source);
        //        public static decimal? Average(this IEnumerable<decimal?> source) => System.Linq.Enumerable.Average(source);
        //        public static float Average(this IEnumerable<float> source) => System.Linq.Enumerable.Average(source);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> Cast<TResult>(this System.Collections.IEnumerable source) => System.Linq.Enumerable.Cast<TResult>(source);
        //        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => System.Linq.Enumerable.Concat<TSource>(first, second);
        [Obsolete("Not converted to ValueLinq yet")] public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) => System.Linq.Enumerable.Contains<TSource>(source, value);
        [Obsolete("Not converted to ValueLinq yet")] public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer) => System.Linq.Enumerable.Contains<TSource>(source, value, comparer);
        //        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.Count<TSource>(source, predicate);
        //        public static int Count<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.Count<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue) => System.Linq.Enumerable.DefaultIfEmpty<TSource>(source, defaultValue);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.DefaultIfEmpty<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.Distinct<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) => System.Linq.Enumerable.Distinct<TSource>(source, comparer);
        //        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) => System.Linq.Enumerable.ElementAt<TSource>(source, index);
        //        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index) => System.Linq.Enumerable.ElementAtOrDefault<TSource>(source, index);
        //        public static IEnumerable<TResult> Empty<TResult>() => System.Linq.Enumerable.Empty<TResult>();
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => System.Linq.Enumerable.Except<TSource>(first, second);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => System.Linq.Enumerable.Except<TSource>(first, second, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource First<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.First<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.First<TSource>(source, predicate);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.FirstOrDefault<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.FirstOrDefault<TSource>(source, predicate);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector) => System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(source, keySelector, resultSelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector) => System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(source, keySelector, resultSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<System.Linq.IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) => System.Linq.Enumerable.GroupBy<TSource, TKey, TElement>(source, keySelector, elementSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<System.Linq.IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.GroupBy<TSource, TKey>(source, keySelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<System.Linq.IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => System.Linq.Enumerable.GroupBy<TSource, TKey>(source, keySelector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<System.Linq.IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.GroupBy<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector) => System.Linq.Enumerable.GroupJoin<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.GroupJoin<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => System.Linq.Enumerable.Intersect<TSource>(first, second);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => System.Linq.Enumerable.Intersect<TSource>(first, second, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector) => System.Linq.Enumerable.Join<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.Join<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.Last<TSource>(source, predicate);
        //        public static TSource Last<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.Last<TSource>(source);
        //        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.LastOrDefault<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.LastOrDefault<TSource>(source, predicate);
        [Obsolete("Not converted to ValueLinq yet")] public static long LongCount<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.LongCount<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.LongCount<TSource>(source, predicate);
        //        public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static float? Max(this IEnumerable<float?> source) => System.Linq.Enumerable.Max(source);
        //        public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => System.Linq.Enumerable.Max<TSource, TResult>(source, selector);
        //        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static double? Max(this IEnumerable<double?> source) => System.Linq.Enumerable.Max(source);
        //        public static TSource Max<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.Max<TSource>(source);
        //        public static float Max(this IEnumerable<float> source) => System.Linq.Enumerable.Max(source);
        //        public static long? Max(this IEnumerable<long?> source) => System.Linq.Enumerable.Max(source);
        //        public static int? Max(this IEnumerable<int?> source) => System.Linq.Enumerable.Max(source);
        //        public static decimal? Max(this IEnumerable<decimal?> source) => System.Linq.Enumerable.Max(source);
        //        public static long Max(this IEnumerable<long> source) => System.Linq.Enumerable.Max(source);
        //        public static int Max(this IEnumerable<int> source) => System.Linq.Enumerable.Max(source);
        //        public static double Max(this IEnumerable<double> source) => System.Linq.Enumerable.Max(source);
        //        public static decimal Max(this IEnumerable<decimal> source) => System.Linq.Enumerable.Max(source);
        //        public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => System.Linq.Enumerable.Max<TSource>(source, selector);
        //        public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => System.Linq.Enumerable.Min<TSource, TResult>(source, selector);
        //        public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => System.Linq.Enumerable.Min<TSource>(source, selector);
        //        public static double Min(this IEnumerable<double> source) => System.Linq.Enumerable.Min(source);
        //        public static float Min(this IEnumerable<float> source) => System.Linq.Enumerable.Min(source);
        //        public static float? Min(this IEnumerable<float?> source) => System.Linq.Enumerable.Min(source);
        //        public static long? Min(this IEnumerable<long?> source) => System.Linq.Enumerable.Min(source);
        //        public static int? Min(this IEnumerable<int?> source) => System.Linq.Enumerable.Min(source);
        //        public static double? Min(this IEnumerable<double?> source) => System.Linq.Enumerable.Min(source);
        //        public static decimal? Min(this IEnumerable<decimal?> source) => System.Linq.Enumerable.Min(source);
        //        public static long Min(this IEnumerable<long> source) => System.Linq.Enumerable.Min(source);
        //        public static int Min(this IEnumerable<int> source) => System.Linq.Enumerable.Min(source);
        //        public static TSource Min<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.Min<TSource>(source);
        //        public static decimal Min(this IEnumerable<decimal> source) => System.Linq.Enumerable.Min(source);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> OfType<TResult>(this System.Collections.IEnumerable source) => System.Linq.Enumerable.OfType<TResult>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => System.Linq.Enumerable.OrderBy<TSource, TKey>(source, keySelector);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) => System.Linq.Enumerable.OrderBy<TSource, TKey>(source, keySelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => System.Linq.Enumerable.OrderByDescending<TSource, TKey>(source, keySelector);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) => System.Linq.Enumerable.OrderByDescending<TSource, TKey>(source, keySelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element) => System.Linq.Enumerable.Prepend<TSource>(source, element);
        //        public static IEnumerable<int> Range(int start, int count) => System.Linq.Enumerable.Range(start, count);
        //        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count) => System.Linq.Enumerable.Repeat(element, count);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.Reverse<TSource>(source);
        //        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => System.Linq.Enumerable.Select<TSource, TResult>(source, selector);
        //        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector) => System.Linq.Enumerable.Select<TSource, TResult>(source, selector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) => System.Linq.Enumerable.SelectMany<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector) => System.Linq.Enumerable.SelectMany<TSource, TResult>(source, selector);
        //        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) => System.Linq.Enumerable.SelectMany<TSource, TResult>(source, selector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) => System.Linq.Enumerable.SelectMany<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => System.Linq.Enumerable.SequenceEqual<TSource>(first, second);
        [Obsolete("Not converted to ValueLinq yet")] public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => System.Linq.Enumerable.SequenceEqual<TSource>(first, second, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.Single<TSource>(source, predicate);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource Single<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.Single<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.SingleOrDefault<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.SingleOrDefault<TSource>(source, predicate);
        //        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count) => System.Linq.Enumerable.Skip<TSource>(source, count);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count) => System.Linq.Enumerable.SkipLast<TSource>(source, count);
        //        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) => System.Linq.Enumerable.SkipWhile<TSource>(source, predicate);
        //        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.SkipWhile<TSource>(source, predicate);
        //        public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => System.Linq.Enumerable.Sum<TSource>(source, selector);
        //        public static double? Sum(this IEnumerable<double?> source) => System.Linq.Enumerable.Sum(source);
        //        public static float? Sum(this IEnumerable<float?> source) => System.Linq.Enumerable.Sum(source);
        //        public static decimal Sum(this IEnumerable<decimal> source) => System.Linq.Enumerable.Sum(source);
        //        public static double Sum(this IEnumerable<double> source) => System.Linq.Enumerable.Sum(source);
        //        public static int Sum(this IEnumerable<int> source) => System.Linq.Enumerable.Sum(source);
        //        public static float Sum(this IEnumerable<float> source) => System.Linq.Enumerable.Sum(source);
        //        public static decimal? Sum(this IEnumerable<decimal?> source) => System.Linq.Enumerable.Sum(source);
        //        public static int? Sum(this IEnumerable<int?> source) => System.Linq.Enumerable.Sum(source);
        //        public static long? Sum(this IEnumerable<long?> source) => System.Linq.Enumerable.Sum(source);
        //        public static long Sum(this IEnumerable<long> source) => System.Linq.Enumerable.Sum(source);
        //        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count) => System.Linq.Enumerable.Take<TSource>(source, count);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source, int count) => System.Linq.Enumerable.TakeLast<TSource>(source, count);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.TakeWhile<TSource>(source, predicate);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) => System.Linq.Enumerable.TakeWhile<TSource>(source, predicate);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this System.Linq.IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) => System.Linq.Enumerable.ThenBy<TSource, TKey>(source, keySelector);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this System.Linq.IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) => System.Linq.Enumerable.ThenBy<TSource, TKey>(source, keySelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this System.Linq.IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) => System.Linq.Enumerable.ThenByDescending<TSource, TKey>(source, keySelector);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this System.Linq.IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) => System.Linq.Enumerable.ThenByDescending<TSource, TKey>(source, keySelector, comparer);
        //        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.ToArray<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => System.Linq.Enumerable.ToDictionary<TSource, TKey>(source, keySelector);
        [Obsolete("Not converted to ValueLinq yet")] public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.ToDictionary<TSource, TKey>(source, keySelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) => System.Linq.Enumerable.ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.ToHashSet<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) => System.Linq.Enumerable.ToHashSet<TSource>(source, comparer);
        //        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source) => System.Linq.Enumerable.ToList<TSource>(source);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) => System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(source, keySelector, elementSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) => System.Linq.Enumerable.ToLookup<TSource, TKey>(source, keySelector, comparer);
        [Obsolete("Not converted to ValueLinq yet")] public static System.Linq.ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => System.Linq.Enumerable.ToLookup<TSource, TKey>(source, keySelector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => System.Linq.Enumerable.Union<TSource>(first, second);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => System.Linq.Enumerable.Union<TSource>(first, second, comparer);
        //        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => System.Linq.Enumerable.Where<TSource>(source, predicate);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) => System.Linq.Enumerable.Where<TSource>(source, predicate);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) => System.Linq.Enumerable.Zip<TFirst, TSecond, TResult>(first, second, resultSelector);
        [Obsolete("Not converted to ValueLinq yet")] public static IEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second) => System.Linq.Enumerable.Zip<TFirst, TSecond, (TFirst, TSecond)>(first, second, (f, s) => (f, s));
    }
}
