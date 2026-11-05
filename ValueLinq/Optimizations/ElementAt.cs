using System;

namespace Cistern.ValueLinq.Optimizations
{
    [Optimization]
    internal interface IOptimizationElementAt<T>
    {
        bool ElementAt(in ElementAt request, out (bool, T) response);
    }

    /// <summary>
    /// The element at an index, from the start or the end, answered as (found, element): First is index 0, Last ^1. A
    /// node answers it, passes it to its source translated, or declines. An answer may run user code (OrderBy's
    /// selection); a decline runs nothing, so a node made of parts settles whatever could make it decline before it asks.
    /// </summary>
    readonly struct ElementAt
    {
        public ElementAt(Index index) => Index = index;

        public Index Index { get; }

        // the position of the element in a sequence of the given length, or -1 when it has no such element
        public int OffsetIn(int length)
        {
            var offset = Index.GetOffset(length);
            return (uint)offset < (uint)length ? offset : -1;
        }
    }
}
