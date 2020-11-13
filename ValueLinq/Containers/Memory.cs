using System;

namespace Cistern.ValueLinq.Containers
{
    struct MemoryFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly ReadOnlyMemory<T> _memory;
        private int _idx;

        public MemoryFastEnumerator(ReadOnlyMemory<T> memory) => (_memory, _idx) = (memory, -1);

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            var idx = _idx + 1;
            if (idx < _memory.Length)
            {
                current = _memory.Span[idx];
                _idx = idx;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct MemoryNode<T>
        : INode<T>
    {
        private readonly ReadOnlyMemory<T> _memory;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_memory.Length, true);

        public MemoryNode(ReadOnlyMemory<T> Memory) => _memory = Memory;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => MemoryNode.Create<T, Head, Tail, CreationType>(_memory, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => MemoryNode.FastEnumerate<T, TResult, FEnumerator>(_memory, fenum);
    }

    static class MemoryNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(ReadOnlyMemory<T> memory, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new MemoryFastEnumerator<T>(memory);
            return nodes.CreateObject<CreationType, T, MemoryFastEnumerator<T>>(ref enumerator);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(ReadOnlyMemory<TIn> memory, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            if (memory.Length > 20 && fenum.CheckForOptimization<ReadOnlyMemory<TIn>, GetSpan<ReadOnlyMemory<TIn>, TIn>, TResult>(memory, in Optimizations.UseSpan<TIn>.FromMemory, out var result))
                return result;

            return SpanNode.FastEnumerate<TIn, TResult, FEnumerator>(memory.Span, fenum);
        }
    }
}
