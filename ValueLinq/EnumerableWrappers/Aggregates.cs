
using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
             => EnumerableNode.FastEnumerateSwitch<TSource, TSource, ReduceForward<TSource>>(source, new ReduceForward<TSource>(func));

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
             => EnumerableNode.FastEnumerateSwitch<TSource, TAccumulate, FoldForward<TSource, TAccumulate>>(source, new FoldForward<TSource, TAccumulate>(func, seed));
        
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
        
    }
    public static partial class ValueLinqList
    {
        public static TSource Aggregate<TSource>(this List<TSource> source, Func<TSource, TSource, TSource> func)
        {
            var aggregate = new ReduceForward<TSource>(func);
            ListByIndexNode.ProcessList(source, ref aggregate);
            return aggregate.GetResult();
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this List<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            var aggregate = new FoldForward<TSource, TAccumulate>(func, seed);
            ListByIndexNode.ProcessList(source, ref aggregate);
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
        
    }
}




