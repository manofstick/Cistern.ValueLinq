﻿using System;

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
        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotSupportedException();
        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();
        #endregion

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        public bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                NodeContainer<T> container = default;
                MemoryNode.ReverseSkip(_memory, skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var take = (Optimizations.Take)(object)request;
                NodeContainer<T> container = default;
                MemoryNode.ReverseTake(_memory, take.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)MemoryNode.ToArrayReverse(_memory);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                NodeContainer<T> container = default;
                container.SetNode(new MemoryNode<T>(_memory));
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.TryLast))
            {
                result = (TResult)(object)(_memory.Length > 0, _memory.Length == 0 ? default : _memory.Span[0]);
                return true;
            }

            result = default;
            return false;
        }

        public TResult CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<T>
            => SpanNode.FastReverseEnumerate<T, TResult, FEnumerator>(_memory.Span, fenum);
    }

    public struct MemoryNode<T>
        : INode<T>
    {
        private readonly ReadOnlyMemory<T> _memory;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_memory.Length, true);

        public MemoryNode(ReadOnlyMemory<T> Memory) => _memory = Memory;

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => MemoryNode.Create<T, Head, Tail, CreationType>(_memory, ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        public bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => MemoryNode.CheckForOptimization<T, TRequest, TResult>(_memory, in request, out result);

        public TResult CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<T>
            => MemoryNode.FastEnumerate<T, TResult, FEnumerator>(_memory, fenum);
    }

    static class MemoryNode
    {
        internal static bool CheckForOptimization<T, TRequest, TResult>(ReadOnlyMemory<T> memory, in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.AsMemory))
            {
                var requestAsMemory = (Optimizations.AsMemory)(object)request;
                if (requestAsMemory.Probe)
                {
                    result = default;
                    return true;
                }
                else
                {
                    result = (TResult)(object)memory;
                    return true;
                }
            }

            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)MemoryNode.ToArray(memory);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                NodeContainer<T> container = default;
                container.SetNode(new ReversedMemoryNode<T>(memory));
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                NodeContainer<T> container = default;
                MemoryNode.Skip(memory, skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var take = (Optimizations.Take)(object)request;
                NodeContainer<T> container = default;
                MemoryNode.Take(memory, take.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)memory.Length;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.TryLast))
            {
                result = (TResult)(object)(memory.Length > 0, memory.Length == 0 ? default : memory.Span[memory.Length-1]);
                return true;
            }

            result = default;
            return false;
        }

        internal static T[] ToArray<T>(ReadOnlyMemory<T> memory)
        {
            if (memory.Length == 0)
                return Array.Empty<T>();

            return memory.Span.ToArray();
        }

        public static T[] ToArrayReverse<T>(ReadOnlyMemory<T> memory)
        {
            var array = ToArray(memory);
            Array.Reverse(array);
            return array;
        }

        internal static void Skip<T>(ReadOnlyMemory<T> memory, int count, ref NodeContainer<T> container)
        {
            if (count >= memory.Length)
            {
                container.SetEmpty();
            }
            else
            {
                container.SetNode(new MemoryNode<T>(memory.Slice(count, memory.Length - count)));
            }
        }
        internal static void Take<T>(ReadOnlyMemory<T> memory, int count, ref NodeContainer<T> container)
        {
            if (count <= 0)
            {
                container.SetEmpty();
            }
            else if (count >= memory.Length)
            {
                container.SetNode(new MemoryNode<T>(memory));
            }
            else
            {
                container.SetNode(new MemoryNode<T>(memory.Slice(0, count)));
            }

        }

        internal static void ReverseSkip<T>(ReadOnlyMemory<T> memory, int count, ref NodeContainer<T> container)
        {
            if (count >= memory.Length)
            {
                container.SetEmpty();
            }
            else
            {
                container.SetNode(new ReversedMemoryNode<T>(memory.Slice(0, memory.Length - count)));
            }
        }

        internal static void ReverseTake<T>(ReadOnlyMemory<T> memory, int count, ref NodeContainer<T> container)
        {
            if (count <= 0)
            {
                container.SetEmpty();
            }
            else
            {
                var length = Math.Min(memory.Length, count);
                container.SetNode(new ReversedMemoryNode<T>(memory.Slice(memory.Length - length, length)));
            }
        }

        public static CreationType Create<T, Head, Tail, CreationType>(ReadOnlyMemory<T> memory, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new MemoryFastEnumerator<T>(memory);
            return nodes.CreateObject<CreationType, T, MemoryFastEnumerator<T>>(ref enumerator);
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
