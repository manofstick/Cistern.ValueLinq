﻿

using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static System.Linq.ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null) =>
            EnumerableNode.ExecutePush<TSource, System.Linq.ILookup<TKey, TSource>, ToLookup<TSource, TKey>>(source, new ToLookup<TSource, TKey>(comparer, keySelector)); 

        public static System.Linq.ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null) =>
            EnumerableNode.ExecutePush<TSource, System.Linq.ILookup<TKey, TElement>, ToLookup<TSource, TKey, TElement>>(source, new ToLookup<TSource, TKey, TElement>(comparer, keySelector, elementSelector)); 

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null) =>
            EnumerableNode.ExecutePush<TSource, Dictionary<TKey, TSource>, ToDictionary<TSource, TKey>>(source, new ToDictionary<TSource, TKey>(keySelector, comparer)); 

        public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> comparer = null) =>
            EnumerableNode.ExecutePush<TSource, Dictionary<TKey, TValue>, ToDictionary<TSource, TKey, TValue>>(source, new ToDictionary<TSource, TKey, TValue>(keySelector, elementSelector, comparer)); 

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer = null) =>
            EnumerableNode.ExecutePush<TSource, HashSet<TSource>, ToHashSet<TSource>>(source, new ToHashSet<TSource>(comparer)); 

        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) =>
            EnumerableNode.ExecutePush<TSource, TSource, ReduceForward<TSource>>(source, new ReduceForward<TSource>(func)); 

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) =>
            EnumerableNode.ExecutePush<TSource, TAccumulate, FoldForward<TSource, TAccumulate>>(source, new FoldForward<TSource, TAccumulate>(func, seed)); 

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) =>
            resultSelector(EnumerableNode.ExecutePush<TSource, TAccumulate, FoldForward<TSource, TAccumulate>>(source, new FoldForward<TSource, TAccumulate>(func, seed))); 

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.ExecutePush<TSource, bool, All<TSource, FuncToIFunc<TSource, bool>>>(source, new All<TSource, FuncToIFunc<TSource, bool>>(new FuncToIFunc<TSource, bool>(predicate))); 

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.ExecutePush<TSource, bool, Any<TSource>>(source, new Any<TSource>(predicate)); 

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.ExecutePush<TSource, int, CountIf<TSource>>(source, new CountIf<TSource>(predicate)); 

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer) =>
            EnumerableNode.ExecutePush<TSource, bool, ContainsByComparer<TSource>>(source, new ContainsByComparer<TSource>(comparer, value)); 

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.ExecutePush<TSource, TSource, LastPredicate<TSource>>(source, new LastPredicate<TSource>(predicate)); 

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.ExecutePush<TSource, TSource, LastOrDefaultPredicate<TSource>>(source, new LastOrDefaultPredicate<TSource>(predicate)); 

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.ExecutePush<TSource, TSource, FirstPredicate<TSource>>(source, new FirstPredicate<TSource>(predicate)); 

        public static TSource First<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.ExecutePush<TSource, TSource, First<TSource>>(source, new First<TSource>()); 

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.ExecutePush<TSource, TSource, FirstOrDefault<TSource>>(source, new FirstOrDefault<TSource>()); 

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.ExecutePush<TSource, TSource, FirstOrDefaultPredicate<TSource>>(source, new FirstOrDefaultPredicate<TSource>(predicate)); 

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.ExecutePush<TSource, TSource, SinglePredicate<TSource>>(source, new SinglePredicate<TSource>(predicate)); 

        public static TSource Single<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.ExecutePush<TSource, TSource, Single<TSource>>(source, new Single<TSource>()); 

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.ExecutePush<TSource, TSource, SingleOrDefault<TSource>>(source, new SingleOrDefault<TSource>()); 

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.ExecutePush<TSource, TSource, SingleOrDefaultPredicate<TSource>>(source, new SingleOrDefaultPredicate<TSource>(predicate)); 

    }
    public static partial class ValueLinqArray
    {
        public static System.Linq.ILookup<TKey, TSource> ToLookup<TSource, TKey>(this TSource[] source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToLookup<TSource, TKey>(comparer, keySelector);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static System.Linq.ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToLookup<TSource, TKey, TElement>(comparer, keySelector, elementSelector);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this TSource[] source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToDictionary<TSource, TKey>(keySelector, comparer);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(this TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToDictionary<TSource, TKey, TValue>(keySelector, elementSelector, comparer);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static HashSet<TSource> ToHashSet<TSource>(this TSource[] source, IEqualityComparer<TSource> comparer = null)
        {
            var aggregate = new ToHashSet<TSource>(comparer);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Aggregate<TSource>(this TSource[] source, Func<TSource, TSource, TSource> func)
        {
            var aggregate = new ReduceForward<TSource>(func);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this TSource[] source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            var aggregate = new FoldForward<TSource, TAccumulate>(func, seed);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this TSource[] source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            var aggregate = new FoldForward<TSource, TAccumulate>(func, seed);
            ArrayNode.ProcessArray(source, ref aggregate);
            return resultSelector(aggregate.GetResult());
        }

        public static bool All<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            var aggregate = new All<TSource, FuncToIFunc<TSource, bool>>(new FuncToIFunc<TSource, bool>(predicate));
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static bool Any<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            var aggregate = new Any<TSource>(predicate);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Count<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            var aggregate = new CountIf<TSource>(predicate);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static bool Contains<TSource>(this TSource[] source, TSource value, IEqualityComparer<TSource> comparer)
        {
            var aggregate = new ContainsByComparer<TSource>(comparer, value);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Last<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            var aggregate = new LastPredicate<TSource>(predicate);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource LastOrDefault<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            var aggregate = new LastOrDefaultPredicate<TSource>(predicate);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource First<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            var aggregate = new FirstPredicate<TSource>(predicate);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource First<TSource>(this TSource[] source)
        {
            var aggregate = new First<TSource>();
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource FirstOrDefault<TSource>(this TSource[] source)
        {
            var aggregate = new FirstOrDefault<TSource>();
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource FirstOrDefault<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            var aggregate = new FirstOrDefaultPredicate<TSource>(predicate);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Single<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            var aggregate = new SinglePredicate<TSource>(predicate);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Single<TSource>(this TSource[] source)
        {
            var aggregate = new Single<TSource>();
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource SingleOrDefault<TSource>(this TSource[] source)
        {
            var aggregate = new SingleOrDefault<TSource>();
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource SingleOrDefault<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            var aggregate = new SingleOrDefaultPredicate<TSource>(predicate);
            ArrayNode.ProcessArray(source, ref aggregate);
            return aggregate.GetResult();
        }

    }
    public static partial class ValueLinqList
    {
        public static System.Linq.ILookup<TKey, TSource> ToLookup<TSource, TKey>(this List<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToLookup<TSource, TKey>(comparer, keySelector);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static System.Linq.ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this List<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToLookup<TSource, TKey, TElement>(comparer, keySelector, elementSelector);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this List<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToDictionary<TSource, TKey>(keySelector, comparer);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(this List<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToDictionary<TSource, TKey, TValue>(keySelector, elementSelector, comparer);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static HashSet<TSource> ToHashSet<TSource>(this List<TSource> source, IEqualityComparer<TSource> comparer = null)
        {
            var aggregate = new ToHashSet<TSource>(comparer);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Aggregate<TSource>(this List<TSource> source, Func<TSource, TSource, TSource> func)
        {
            var aggregate = new ReduceForward<TSource>(func);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this List<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            var aggregate = new FoldForward<TSource, TAccumulate>(func, seed);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this List<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            var aggregate = new FoldForward<TSource, TAccumulate>(func, seed);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return resultSelector(aggregate.GetResult());
        }

        public static bool All<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new All<TSource, FuncToIFunc<TSource, bool>>(new FuncToIFunc<TSource, bool>(predicate));
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static bool Any<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new Any<TSource>(predicate);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Count<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new CountIf<TSource>(predicate);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static bool Contains<TSource>(this List<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            var aggregate = new ContainsByComparer<TSource>(comparer, value);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Last<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new LastPredicate<TSource>(predicate);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource LastOrDefault<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new LastOrDefaultPredicate<TSource>(predicate);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource First<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new FirstPredicate<TSource>(predicate);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource First<TSource>(this List<TSource> source)
        {
            var aggregate = new First<TSource>();
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource FirstOrDefault<TSource>(this List<TSource> source)
        {
            var aggregate = new FirstOrDefault<TSource>();
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource FirstOrDefault<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new FirstOrDefaultPredicate<TSource>(predicate);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Single<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new SinglePredicate<TSource>(predicate);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Single<TSource>(this List<TSource> source)
        {
            var aggregate = new Single<TSource>();
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource SingleOrDefault<TSource>(this List<TSource> source)
        {
            var aggregate = new SingleOrDefault<TSource>();
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource SingleOrDefault<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new SingleOrDefaultPredicate<TSource>(predicate);
            ListSegmentNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

    }
    public static partial class ValueLinqMemory
    {
        public static System.Linq.ILookup<TKey, TSource> ToLookup<TSource, TKey>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToLookup<TSource, TKey>(comparer, keySelector);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static System.Linq.ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToLookup<TSource, TKey, TElement>(comparer, keySelector, elementSelector);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToDictionary<TSource, TKey>(keySelector, comparer);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(this ReadOnlyMemory<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> comparer = null)
        {
            var aggregate = new ToDictionary<TSource, TKey, TValue>(keySelector, elementSelector, comparer);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static HashSet<TSource> ToHashSet<TSource>(this ReadOnlyMemory<TSource> source, IEqualityComparer<TSource> comparer = null)
        {
            var aggregate = new ToHashSet<TSource>(comparer);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Aggregate<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, TSource, TSource> func)
        {
            var aggregate = new ReduceForward<TSource>(func);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this ReadOnlyMemory<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            var aggregate = new FoldForward<TSource, TAccumulate>(func, seed);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this ReadOnlyMemory<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            var aggregate = new FoldForward<TSource, TAccumulate>(func, seed);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return resultSelector(aggregate.GetResult());
        }

        public static bool All<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new All<TSource, FuncToIFunc<TSource, bool>>(new FuncToIFunc<TSource, bool>(predicate));
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static bool Any<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new Any<TSource>(predicate);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static int Count<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new CountIf<TSource>(predicate);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static bool Contains<TSource>(this ReadOnlyMemory<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            var aggregate = new ContainsByComparer<TSource>(comparer, value);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Last<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new LastPredicate<TSource>(predicate);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource LastOrDefault<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new LastOrDefaultPredicate<TSource>(predicate);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource First<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new FirstPredicate<TSource>(predicate);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource First<TSource>(this ReadOnlyMemory<TSource> source)
        {
            var aggregate = new First<TSource>();
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource FirstOrDefault<TSource>(this ReadOnlyMemory<TSource> source)
        {
            var aggregate = new FirstOrDefault<TSource>();
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource FirstOrDefault<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new FirstOrDefaultPredicate<TSource>(predicate);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Single<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new SinglePredicate<TSource>(predicate);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource Single<TSource>(this ReadOnlyMemory<TSource> source)
        {
            var aggregate = new Single<TSource>();
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource SingleOrDefault<TSource>(this ReadOnlyMemory<TSource> source)
        {
            var aggregate = new SingleOrDefault<TSource>();
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TSource SingleOrDefault<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate)
        {
            var aggregate = new SingleOrDefaultPredicate<TSource>(predicate);
            MemoryNode.ProcessMemory(source, ref aggregate);
            return aggregate.GetResult();
        }

    }
}




