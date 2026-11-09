using System;

namespace Cistern.ValueLinq.Optimizations
{
    [PushOptimization]
    internal interface IOptimizationFilteredSpanSource<T>
    {
        bool FilteredSpanSource(in FilteredSpanSource request, out (SpanSource<T> Source, Func<T, bool> Predicate) response);
    }

    /// <summary>
    /// Returns a Where's source as a span and its predicate, for a Select above to filter and project in one pass of
    /// its own (Where's bitmask ToArray, the selector run on the survivors). Answering runs nothing.
    /// </summary>
    readonly struct FilteredSpanSource
    {
        public static bool TryGet<T, Node>(ref Node node, out (SpanSource<T> Source, Func<T, bool> Predicate) filtered)
            where Node : INode<T>
            => node.TryPushOptimization(new FilteredSpanSource(), out filtered);
    }
}
