

using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, ReduceForward<TSource>>(source, new ReduceForward<TSource>(func)); 

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TAccumulate, FoldForward<TSource, TAccumulate>>(source, new FoldForward<TSource, TAccumulate>(func, seed)); 

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) =>
            resultSelector(EnumerableNode.FastEnumerateSwitch<TSource, TAccumulate, FoldForward<TSource, TAccumulate>>(source, new FoldForward<TSource, TAccumulate>(func, seed))); 

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.FastEnumerateSwitch<TSource, bool, All<TSource, FuncToIFunc<TSource, bool>>>(source, new All<TSource, FuncToIFunc<TSource, bool>>(new FuncToIFunc<TSource, bool>(predicate))); 

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.FastEnumerateSwitch<TSource, bool, Any<TSource>>(source, new Any<TSource>(predicate)); 

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.FastEnumerateSwitch<TSource, int, CountIf<TSource>>(source, new CountIf<TSource>(predicate)); 

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer) =>
            EnumerableNode.FastEnumerateSwitch<TSource, bool, ContainsByComparer<TSource>>(source, new ContainsByComparer<TSource>(comparer, value)); 

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, LastPredicate<TSource>>(source, new LastPredicate<TSource>(predicate)); 

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, LastOrDefaultPredicate<TSource>>(source, new LastOrDefaultPredicate<TSource>(predicate)); 

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, FirstPredicate<TSource>>(source, new FirstPredicate<TSource>(predicate)); 

        public static TSource First<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, First<TSource>>(source, new First<TSource>()); 

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, FirstOrDefault<TSource>>(source, new FirstOrDefault<TSource>()); 

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, FirstOrDefaultPredicate<TSource>>(source, new FirstOrDefaultPredicate<TSource>(predicate)); 

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, SinglePredicate<TSource>>(source, new SinglePredicate<TSource>(predicate)); 

        public static TSource Single<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, Single<TSource>>(source, new Single<TSource>()); 

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, SingleOrDefault<TSource>>(source, new SingleOrDefault<TSource>()); 

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            EnumerableNode.FastEnumerateSwitch<TSource, TSource, SingleOrDefaultPredicate<TSource>>(source, new SingleOrDefaultPredicate<TSource>(predicate)); 

    }
    public static partial class ValueLinqArray
    {
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




