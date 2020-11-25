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

    public struct ReversedMemoryNode<T>
        : INode<T>
    {
        private readonly ReadOnlyMemory<T> _memory; // array is still in forward order

        public ReversedMemoryNode(ReadOnlyMemory<T> memory) => _memory = memory;

#region "This node is only used in forward context, so most of interface is not supported"
        public void GetCountInformation(out CountInformation info) => throw new NotSupportedException();
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotSupportedException();
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();
#endregion

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                result = (TResult)(object)MemoryNode.ReverseSkip(_memory, skip.Count);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => SpanNode.FastReverseEnumerate<T, TResult, FEnumerator>(_memory.Span, fenum);
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
            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)MemoryNode.ToArray(_memory);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                result = (TResult)(object)new ReversedMemoryNode<T>(_memory);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                result = (TResult)(object)MemoryNode.Skip(_memory, skip.Count);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)_memory.Length;
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => MemoryNode.FastEnumerate<T, TResult, FEnumerator>(_memory, fenum);
    }

    static class MemoryNode
    {
        internal static T[] ToArray<T>(ReadOnlyMemory<T> memory)
        {
            if (memory.Length == 0)
                return Array.Empty<T>();

            return memory.Span.ToArray();
        }

        internal static INode<T> Skip<T>(ReadOnlyMemory<T> memory, int count)
        {
            if (count >= memory.Length)
                return EmptyNode<T>.Empty;

            return new MemoryNode<T>(memory.Slice(count, memory.Length - count));
        }
        internal static INode<T> ReverseSkip<T>(ReadOnlyMemory<T> memory, int count)
        {
            if (count >= memory.Length)
                return EmptyNode<T>.Empty;

            return new MemoryNode<T>(memory.Slice(0, memory.Length - count));
        }

        public static CreationType Create<T, Head, Tail, CreationType>(ReadOnlyMemory<T> memory, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new MemoryFastEnumerator<T>(memory);
            return nodes.CreateObject<CreationType, T, MemoryFastEnumerator<T>>(0, ref enumerator);
        }

        internal static void ProcessMemory<TIn, FEnumerator>(ReadOnlyMemory<TIn> memory, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            if (memory.Length < 20
             || BatchProcessResult.Unavailable == fenum.TryProcessBatch<ReadOnlyMemory<TIn>, GetSpan<ReadOnlyMemory<TIn>, TIn>>(memory, in Optimizations.UseSpan<TIn>.FromMemory))
            {
                SpanNode.Loop<TIn, FEnumerator>(memory.Span, ref fenum);
            }
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(ReadOnlyMemory<TIn> memory, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                ProcessMemory(memory, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }
    }
}
