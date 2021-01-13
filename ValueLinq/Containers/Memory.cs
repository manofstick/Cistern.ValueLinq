using System;

namespace Cistern.ValueLinq.Containers
{
    struct MemoryPullEnumerator<T>
        : IPullEnumerator<T>
    {
        private readonly ReadOnlyMemory<T> _memory;
        private int _idx;

        public MemoryPullEnumerator(ReadOnlyMemory<T> memory) => (_memory, _idx) = (memory, -1);

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
        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes) => throw new NotSupportedException();
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

        public TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
            => SpanNode.ExecuteReversePush<T, TResult, TPushEnumerator>(_memory.Span, fenum);
    }

    public struct MemoryNode<TSource>
        : INode<TSource>
    {
        private readonly ReadOnlyMemory<TSource> _memory;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_memory.Length, true);

        public MemoryNode(ReadOnlyMemory<TSource> Memory) => _memory = Memory;

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => MemoryNode.Create<TSource, TNodes, CreationType>(_memory, ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) 
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        public bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => MemoryNode.TryPushOptimization<TSource, TRequest, TResult>(_memory, in request, out result);

        public TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
            => MemoryNode.ExecutePush<TSource, TResult, TPushEnumerator>(_memory, fenum);
    }

    static class MemoryNode
    {
        internal static bool TryPushOptimization<TSource, TRequest, TResult>(ReadOnlyMemory<TSource> memory, in TRequest request, out TResult result)
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
                NodeContainer<TSource> container = default;
                container.SetNode(new ReversedMemoryNode<TSource>(memory));
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                NodeContainer<TSource> container = default;
                MemoryNode.Skip(memory, skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var take = (Optimizations.Take)(object)request;
                NodeContainer<TSource> container = default;
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

        public static CreationType Create<T, TNodes, CreationType>(ReadOnlyMemory<T> memory, ref TNodes nodes)
            where TNodes : INodes
        {
            var enumerator = new MemoryPullEnumerator<T>(memory);
            return nodes.CreateObject<CreationType, T, MemoryPullEnumerator<T>>(ref enumerator);
        }

        internal static void ProcessMemory<TSource, TPushEnumerator>(ReadOnlyMemory<TSource> memory, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            if (memory.Length < 20
             || BatchProcessResult.Unavailable == fenum.TryProcessBatch<ReadOnlyMemory<TSource>, GetSpan<ReadOnlyMemory<TSource>, TSource>>(memory, in Optimizations.UseSpan<TSource>.FromMemory))
            {
                SpanNode.Loop<TSource, TPushEnumerator>(memory.Span, ref fenum);
            }
        }

        internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(ReadOnlyMemory<TSource> memory, TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
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
