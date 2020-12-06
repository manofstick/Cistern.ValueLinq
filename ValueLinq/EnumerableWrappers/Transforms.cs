

using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static ValueEnumerable<TSource, ReverseNode<TSource, EnumerableNode<TSource>>> Reverse<TSource>(this IEnumerable<TSource> source) =>
            new (NodeImpl.Reverse<TSource, EnumerableNode<TSource>>(Enumerable.ToNode(source)));

        public static ValueEnumerable<TResult, SelectNode<TSource, TResult, EnumerableNode<TSource>>> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, Select_InNode<TSource, TResult, EnumerableNode<TSource>>> Select<TSource, TResult>(this IEnumerable<TSource> source, InFunc<TSource, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, SelectIdxNode<TSource, TResult, EnumerableNode<TSource>>> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, ValueSelectNode<TSource, TResult, EnumerableNode<TSource>, IFunc>> Select<TSource, TResult, IFunc>(this IEnumerable<TSource> source, IFunc selector, TResult forTypeInference) where IFunc : IFunc<TSource, TResult> =>
            new (NodeImpl.Select<TSource, TResult, EnumerableNode<TSource>, IFunc>(Enumerable.ToNode(source), selector, forTypeInference));

        public static ValueEnumerable<TSource, WhereNode<TSource, EnumerableNode<TSource>>> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, WhereIdxNode<TSource, EnumerableNode<TSource>>> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, Where_InNode<TSource, EnumerableNode<TSource>>> Where<TSource>(this IEnumerable<TSource> source, InFunc<TSource, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, ValueWhereNode<TSource, EnumerableNode<TSource>, Predicate>> Where<TSource, Predicate>(this IEnumerable<TSource> source, Predicate predicate) where Predicate : IFunc<TSource, bool> =>
            new (NodeImpl.Where<TSource, EnumerableNode<TSource>, Predicate>(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, TakeNode<TSource, EnumerableNode<TSource>>> Take<TSource>(this IEnumerable<TSource> source, int count) =>
            new (NodeImpl.Take<TSource, EnumerableNode<TSource>>(Enumerable.ToNode(source), count));

        public static ValueEnumerable<TSource, TakeWhileNode<TSource, EnumerableNode<TSource>>> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            new (NodeImpl.TakeWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, TakeWhileIdxNode<TSource, EnumerableNode<TSource>>> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.TakeWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, SkipNode<TSource, EnumerableNode<TSource>>> Skip<TSource>(this IEnumerable<TSource> source, int count) =>
            new (NodeImpl.Skip<TSource, EnumerableNode<TSource>>(Enumerable.ToNode(source), count));

        public static ValueEnumerable<TSource, SkipWhileNode<TSource, EnumerableNode<TSource>>> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            new (NodeImpl.SkipWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, SkipWhileIdxNode<TSource, EnumerableNode<TSource>>> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.SkipWhile(Enumerable.ToNode(source), predicate));

    }
    public static partial class ValueLinqArray
    {
        public static ValueEnumerable<TSource, ReverseNode<TSource, ArrayNode<TSource>>> Reverse<TSource>(this TSource[] source) =>
            new (NodeImpl.Reverse<TSource, ArrayNode<TSource>>(Enumerable.ToNode(source)));

        public static ValueEnumerable<TResult, SelectNode<TSource, TResult, ArrayNode<TSource>>> Select<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, Select_InNode<TSource, TResult, ArrayNode<TSource>>> Select<TSource, TResult>(this TSource[] source, InFunc<TSource, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, SelectIdxNode<TSource, TResult, ArrayNode<TSource>>> Select<TSource, TResult>(this TSource[] source, Func<TSource, int, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, ValueSelectNode<TSource, TResult, ArrayNode<TSource>, IFunc>> Select<TSource, TResult, IFunc>(this TSource[] source, IFunc selector, TResult forTypeInference) where IFunc : IFunc<TSource, TResult> =>
            new (NodeImpl.Select<TSource, TResult, ArrayNode<TSource>, IFunc>(Enumerable.ToNode(source), selector, forTypeInference));

        public static ValueEnumerable<TSource, WhereNode<TSource, ArrayNode<TSource>>> Where<TSource>(this TSource[] source, Func<TSource, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, WhereIdxNode<TSource, ArrayNode<TSource>>> Where<TSource>(this TSource[] source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, Where_InNode<TSource, ArrayNode<TSource>>> Where<TSource>(this TSource[] source, InFunc<TSource, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, ValueWhereNode<TSource, ArrayNode<TSource>, Predicate>> Where<TSource, Predicate>(this TSource[] source, Predicate predicate) where Predicate : IFunc<TSource, bool> =>
            new (NodeImpl.Where<TSource, ArrayNode<TSource>, Predicate>(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, TakeNode<TSource, ArrayNode<TSource>>> Take<TSource>(this TSource[] source, int count) =>
            new (NodeImpl.Take<TSource, ArrayNode<TSource>>(Enumerable.ToNode(source), count));

        public static ValueEnumerable<TSource, TakeWhileNode<TSource, ArrayNode<TSource>>> TakeWhile<TSource>(this TSource[] source, Func<TSource, bool> predicate) =>
            new (NodeImpl.TakeWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, TakeWhileIdxNode<TSource, ArrayNode<TSource>>> TakeWhile<TSource>(this TSource[] source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.TakeWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, SkipNode<TSource, ArrayNode<TSource>>> Skip<TSource>(this TSource[] source, int count) =>
            new (NodeImpl.Skip<TSource, ArrayNode<TSource>>(Enumerable.ToNode(source), count));

        public static ValueEnumerable<TSource, SkipWhileNode<TSource, ArrayNode<TSource>>> SkipWhile<TSource>(this TSource[] source, Func<TSource, bool> predicate) =>
            new (NodeImpl.SkipWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, SkipWhileIdxNode<TSource, ArrayNode<TSource>>> SkipWhile<TSource>(this TSource[] source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.SkipWhile(Enumerable.ToNode(source), predicate));

    }
    public static partial class ValueLinqList
    {
        public static ValueEnumerable<TSource, ReverseNode<TSource, ListNode<TSource>>> Reverse<TSource>(this List<TSource> source) =>
            new (NodeImpl.Reverse<TSource, ListNode<TSource>>(Enumerable.ToNode(source)));

        public static ValueEnumerable<TResult, SelectNode<TSource, TResult, ListNode<TSource>>> Select<TSource, TResult>(this List<TSource> source, Func<TSource, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, Select_InNode<TSource, TResult, ListNode<TSource>>> Select<TSource, TResult>(this List<TSource> source, InFunc<TSource, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, SelectIdxNode<TSource, TResult, ListNode<TSource>>> Select<TSource, TResult>(this List<TSource> source, Func<TSource, int, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, ValueSelectNode<TSource, TResult, ListNode<TSource>, IFunc>> Select<TSource, TResult, IFunc>(this List<TSource> source, IFunc selector, TResult forTypeInference) where IFunc : IFunc<TSource, TResult> =>
            new (NodeImpl.Select<TSource, TResult, ListNode<TSource>, IFunc>(Enumerable.ToNode(source), selector, forTypeInference));

        public static ValueEnumerable<TSource, WhereNode<TSource, ListNode<TSource>>> Where<TSource>(this List<TSource> source, Func<TSource, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, WhereIdxNode<TSource, ListNode<TSource>>> Where<TSource>(this List<TSource> source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, Where_InNode<TSource, ListNode<TSource>>> Where<TSource>(this List<TSource> source, InFunc<TSource, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, ValueWhereNode<TSource, ListNode<TSource>, Predicate>> Where<TSource, Predicate>(this List<TSource> source, Predicate predicate) where Predicate : IFunc<TSource, bool> =>
            new (NodeImpl.Where<TSource, ListNode<TSource>, Predicate>(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, TakeNode<TSource, ListNode<TSource>>> Take<TSource>(this List<TSource> source, int count) =>
            new (NodeImpl.Take<TSource, ListNode<TSource>>(Enumerable.ToNode(source), count));

        public static ValueEnumerable<TSource, TakeWhileNode<TSource, ListNode<TSource>>> TakeWhile<TSource>(this List<TSource> source, Func<TSource, bool> predicate) =>
            new (NodeImpl.TakeWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, TakeWhileIdxNode<TSource, ListNode<TSource>>> TakeWhile<TSource>(this List<TSource> source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.TakeWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, SkipNode<TSource, ListNode<TSource>>> Skip<TSource>(this List<TSource> source, int count) =>
            new (NodeImpl.Skip<TSource, ListNode<TSource>>(Enumerable.ToNode(source), count));

        public static ValueEnumerable<TSource, SkipWhileNode<TSource, ListNode<TSource>>> SkipWhile<TSource>(this List<TSource> source, Func<TSource, bool> predicate) =>
            new (NodeImpl.SkipWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, SkipWhileIdxNode<TSource, ListNode<TSource>>> SkipWhile<TSource>(this List<TSource> source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.SkipWhile(Enumerable.ToNode(source), predicate));

    }
    public static partial class ValueLinqMemory
    {
        public static ValueEnumerable<TSource, ReverseNode<TSource, MemoryNode<TSource>>> Reverse<TSource>(this ReadOnlyMemory<TSource> source) =>
            new (NodeImpl.Reverse<TSource, MemoryNode<TSource>>(Enumerable.ToNode(source)));

        public static ValueEnumerable<TResult, SelectNode<TSource, TResult, MemoryNode<TSource>>> Select<TSource, TResult>(this ReadOnlyMemory<TSource> source, Func<TSource, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, Select_InNode<TSource, TResult, MemoryNode<TSource>>> Select<TSource, TResult>(this ReadOnlyMemory<TSource> source, InFunc<TSource, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, SelectIdxNode<TSource, TResult, MemoryNode<TSource>>> Select<TSource, TResult>(this ReadOnlyMemory<TSource> source, Func<TSource, int, TResult> selector) =>
            new (NodeImpl.Select(Enumerable.ToNode(source), selector));

        public static ValueEnumerable<TResult, ValueSelectNode<TSource, TResult, MemoryNode<TSource>, IFunc>> Select<TSource, TResult, IFunc>(this ReadOnlyMemory<TSource> source, IFunc selector, TResult forTypeInference) where IFunc : IFunc<TSource, TResult> =>
            new (NodeImpl.Select<TSource, TResult, MemoryNode<TSource>, IFunc>(Enumerable.ToNode(source), selector, forTypeInference));

        public static ValueEnumerable<TSource, WhereNode<TSource, MemoryNode<TSource>>> Where<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, WhereIdxNode<TSource, MemoryNode<TSource>>> Where<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, Where_InNode<TSource, MemoryNode<TSource>>> Where<TSource>(this ReadOnlyMemory<TSource> source, InFunc<TSource, bool> predicate) =>
            new (NodeImpl.Where(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, ValueWhereNode<TSource, MemoryNode<TSource>, Predicate>> Where<TSource, Predicate>(this ReadOnlyMemory<TSource> source, Predicate predicate) where Predicate : IFunc<TSource, bool> =>
            new (NodeImpl.Where<TSource, MemoryNode<TSource>, Predicate>(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, TakeNode<TSource, MemoryNode<TSource>>> Take<TSource>(this ReadOnlyMemory<TSource> source, int count) =>
            new (NodeImpl.Take<TSource, MemoryNode<TSource>>(Enumerable.ToNode(source), count));

        public static ValueEnumerable<TSource, TakeWhileNode<TSource, MemoryNode<TSource>>> TakeWhile<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate) =>
            new (NodeImpl.TakeWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, TakeWhileIdxNode<TSource, MemoryNode<TSource>>> TakeWhile<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.TakeWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, SkipNode<TSource, MemoryNode<TSource>>> Skip<TSource>(this ReadOnlyMemory<TSource> source, int count) =>
            new (NodeImpl.Skip<TSource, MemoryNode<TSource>>(Enumerable.ToNode(source), count));

        public static ValueEnumerable<TSource, SkipWhileNode<TSource, MemoryNode<TSource>>> SkipWhile<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, bool> predicate) =>
            new (NodeImpl.SkipWhile(Enumerable.ToNode(source), predicate));

        public static ValueEnumerable<TSource, SkipWhileIdxNode<TSource, MemoryNode<TSource>>> SkipWhile<TSource>(this ReadOnlyMemory<TSource> source, Func<TSource, int, bool> predicate) =>
            new (NodeImpl.SkipWhile(Enumerable.ToNode(source), predicate));

    }
}




