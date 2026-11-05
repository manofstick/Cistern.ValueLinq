using Cistern.ValueLinq.Containers;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Optimizations
{
    [Optimization]
    internal interface IOptimizationMaterialiseArray<T>
    {
        bool MaterialiseArray(in MaterialiseArray request, [MaybeNullWhen(false)] out T[] response);
    }

    [Optimization]
    internal interface IOptimizationMaterialiseList<T>
    {
        bool MaterialiseList(in MaterialiseList request, [MaybeNullWhen(false)] out List<T> response);
    }

    /// <summary>
    /// ToArray's result built by a node made of parts, asked once CopyTo's dry run has declined and before the push: a
    /// Concat pushes its middle parts and copies its leading collections and counted trailing parts straight into the
    /// result. A node that declines has run nothing.
    /// </summary>
    readonly struct MaterialiseArray
    {
    }

    /// <summary>
    /// ToList's counterpart of MaterialiseArray.
    /// </summary>
    readonly struct MaterialiseList
    {
    }

    static class Materialise
    {
        // only a Concat, and a Where over a span (MaterialiseList), answer these, and a source re-sliced or reversed is
        // never either, so a node forwarding to it so declines them without building it
        internal static bool Is<TRequest>() =>
            typeof(TRequest) == typeof(MaterialiseArray)
            || typeof(TRequest) == typeof(MaterialiseList);
    }

    [Optimization]
    internal interface IOptimizationConcatParts<T>
    {
        bool ConcatParts(in ConcatParts<T> request, out ConcatParts<T> response);
    }

    [Optimization]
    internal interface IOptimizationConcatPushRange<T>
    {
        bool ConcatPushRange(in ConcatPushRange<T> request, out bool response);
    }

    enum ConcatStep : byte
    {
        Leaves,
        Shape,
        Count,
        Write,
    }

    /// <summary>
    /// A step of a typed Concat's materialisation, asked by the Concat above of a part that is itself a Concat, over its
    /// leaves (the parts below it that are not Concats) in order. The answer is the request with its step's fields
    /// filled: Leaves; Shape, the leaves, the index of the first one that is not copied in place (Leaves for none) and
    /// of the last one that is not counted (-1 for none); Count, the length of the leaves from From to To; Write, the
    /// leaves from From to To copied into Destination, advanced past them.
    /// </summary>
    struct ConcatParts<T>
    {
        public ConcatStep Step;
        public int From;
        public int To;
        public int Leaves;
        public int FirstPushed;
        public int LastPushed;
        public long Count;
        public CopyTo<T> Destination;
    }

    /// <summary>
    /// The leaves from From to To of a typed Concat pushed into the sink.
    /// </summary>
    readonly struct ConcatPushRange<T>(int from, int to, ErasedChain.PushSink<T> sink)
    {
        public readonly int From = from;
        public readonly int To = to;
        public readonly ErasedChain.PushSink<T> Sink = sink;
    }
}
