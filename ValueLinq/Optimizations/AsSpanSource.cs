using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.ValueLinq.Optimizations
{
    [Optimization]
    internal interface IOptimizationAsSpanSource<T>
    {
        bool AsSpanSource(in AsSpanSource request, out SpanSource<T> response);
    }

    /// <summary>
    /// Returns a SpanSource&lt;T&gt;: the container's elements reachable as a span without copying, including a
    /// List&lt;T&gt;'s, whose span comes from CollectionsMarshal and cannot be held as a Memory.
    /// </summary>
    readonly struct AsSpanSource
    {
        private AsSpanSource(bool stable) => Stable = stable;

        /// <summary>
        /// The asker holds the source across calls that return to its caller (a pull enumerator between MoveNexts), so
        /// a source whose span is only valid while it is not mutated (a List&lt;T&gt;'s) declines, to be enumerated and
        /// throw on a change as its enumerator does. Within one call, as a push or a batch, every span is valid.
        /// </summary>
        public bool Stable { get; }

        public static bool TryGet<T, Node>(ref Node node, out SpanSource<T> source, bool stable = false)
            where Node : INode<T>
            => node.TryPushOptimization(new AsSpanSource(stable), out source);
    }

    /// <summary>
    /// An array, a list segment or a memory. The span of a list is only valid while the list is not mutated, which is
    /// the same contract the list containers' own batch processing relies on.
    /// </summary>
    readonly struct SpanSource<T>
    {
        // the kind is a constant at each producer, so once GetSpan is inlined the branch folds and the array arm
        // dereferences the array directly; a null array therefore faults here rather than reading as empty
        private const byte KindArray = 0;
        private const byte KindList = 1;
        private const byte KindMemory = 2;

        private readonly byte _kind;
        private readonly T[]? _array;
        private readonly List<T>? _list;
        private readonly int _start;
        private readonly int _count;
        private readonly ReadOnlyMemory<T> _memory;

        private SpanSource(byte kind, T[]? array, List<T>? list, int start, int count, ReadOnlyMemory<T> memory)
        {
            _kind = kind;
            _array = array;
            _list = list;
            _start = start;
            _count = count;
            _memory = memory;
        }

        public static SpanSource<T> FromArray(T[] array) => new(KindArray, array, null, 0, 0, default);
        public static SpanSource<T> FromList(List<T> list, int start, int count) => new(KindList, null, list, start, count, default);
        public static SpanSource<T> FromMemory(ReadOnlyMemory<T> memory) => new(KindMemory, null, null, 0, 0, memory);

        public ReadOnlySpan<T> GetSpan() =>
            _kind == KindArray
                ? MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetArrayDataReference(_array!), _array!.Length)
                : _kind == KindList
                    ? CollectionsMarshal.AsSpan(_list).Slice(_start, _count)
                    : _memory.Span;
    }
}
