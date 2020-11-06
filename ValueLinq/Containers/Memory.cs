using System;

namespace Cistern.ValueLinq.Containers
{
    struct MemoryFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly ReadOnlyMemory<T> _memory;
        private int _idx;

        public MemoryFastEnumerator(ReadOnlyMemory<T> memory) => (_memory, _idx) = (memory, -1);

        public (bool, int)? InitialSize => (true, _memory.Length);

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
        : INode
    {
        private readonly ReadOnlyMemory<T> _memory;

        public int? MaximumLength => _memory.Length;

        public MemoryNode(ReadOnlyMemory<T> Memory) => _memory = Memory;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => MemoryNode.Create<T, Head, Tail, CreationType>(_memory, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            => MemoryNode.FastEnumerate<TIn, TResult, FEnumerator>((ReadOnlyMemory<TIn>)(object)_memory, fenum);
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
            fenum.Init(memory.Length);

            Loop(memory, ref fenum);

            return fenum.GetResult<TResult>();
        }

        private static void Loop<TIn, FEnumerator>(ReadOnlyMemory<TIn> memory, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            var span = memory.Span;
            for (var i = 0; i < span.Length; ++i)
            {
                if (!fenum.ProcessNext(span[i]))
                    break;
            }
        }
    }
}
