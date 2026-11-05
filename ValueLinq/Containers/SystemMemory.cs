namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Runtime.CompilerServices;

// A ReadOnlyMemory<T> held as itself: pushed as its span where the chain takes one, else looped by index, and sliced
// in place for Skip, Take and Reverse.
public static partial class SystemMemory
{
    [Answerer]
    public readonly partial struct Node<TSource>
        : INode<TSource>
        , Optimizations.IOptimizationAsSpanSource<TSource>
        , Optimizations.IOptimizationToArray<TSource>
        , Optimizations.IOptimizationReverse<TSource>
        , Optimizations.IOptimizationSkipTake<TSource>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<TSource>
        , Optimizations.IOptimizationCopyTo<TSource>
        , Optimizations.IOptimizationAny
    {
        internal readonly ReadOnlyMemory<TSource> _memory;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_memory.Length, true);

        public Node(ReadOnlyMemory<TSource> Memory) => _memory = Memory;

        private bool AsSpanSource(in Optimizations.AsSpanSource request, out Optimizations.SpanSource<TSource> response)
        {
            response = Optimizations.SpanSource<TSource>.FromMemory(_memory);
            return true;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out TSource[] response)
        {
            response = SystemMemory.ToArray(_memory);
            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<TSource> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(new ReversedNode<TSource>(_memory));
            return true;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<TSource> response)
        {
            Unsafe.SkipInit(out response);
            SystemMemory.Slice(_memory, in request, ref response);
            return true;
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            response = _memory.Length;
            return true;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, TSource) response)
        {
            var offset = request.OffsetIn(_memory.Length);
            response = (offset >= 0, offset < 0 ? default! : _memory.Span[offset]);
            return true;
        }

        private bool Any(in Optimizations.Any request, out bool response)
        {
            response = _memory.Length != 0;
            return true;
        }

        // answered here as well as through AsSpanSource, so a sized copy costs one request descent rather than two
        private bool CopyTo(in Optimizations.CopyTo<TSource> request, out int response)
        {
            response = _memory.Length;
            if (!request.IsDryRun)
                _memory.Span.CopyTo(request.Span);

            return true;
        }

        internal TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<TSource>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemMemory.ExecutePush<TSource, TPushResult, TPushEnumerator>(_memory, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        TPushResult INode<TSource>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    [Puller]
    public partial struct Pull<T>
    {
        private readonly ReadOnlyMemory<T> _memory;
        private int _idx;

        internal Pull(ReadOnlyMemory<T> memory)
        {
            _memory = memory;
            _idx = -1;
        }

        private static Pull<T> Create(in Node<T> node)
            => new(node._memory);

        public bool MoveNext() => ++_idx < _memory.Length;

        public T Current => _memory.Span[_idx];
    }

    [Answerer]
    public readonly partial struct ReversedNode<T>
        : INode<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationToArray<T>
        , Optimizations.IOptimizationReverse<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationCopyTo<T>
    {
        internal readonly ReadOnlyMemory<T> _memory; // array is still in forward order

        public ReversedNode(ReadOnlyMemory<T> memory) => _memory = memory;

        public void GetCountInformation(out CountInformation info) => throw new NotSupportedException("This node is only used in forward context");

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            SystemMemory.ReverseSlice(_memory, in request, ref response);
            return true;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            response = SystemMemory.ToArrayReverse(_memory);
            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(new Node<T>(_memory));
            return true;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var offset = request.OffsetIn(_memory.Length);
            response = (offset >= 0, offset < 0 ? default! : _memory.Span[_memory.Length - 1 - offset]);
            return true;
        }

        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            response = _memory.Length;
            if (request.IsDryRun)
                return true;

            var destination = request.Span[.._memory.Length];
            _memory.Span.CopyTo(destination);
            destination.Reverse();
            return true;
        }

        internal TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemSpan.ExecuteReversePush<T, TPushResult, TPushEnumerator>(_memory.Span, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    internal static T[] ToArray<T>(ReadOnlyMemory<T> memory) =>
        SystemSpan.ToArray(memory.Span);

    public static T[] ToArrayReverse<T>(ReadOnlyMemory<T> memory)
    {
        var array = ToArray(memory);
        Array.Reverse(array);
        return array;
    }

    internal static void Slice<T>(ReadOnlyMemory<T> memory, in Optimizations.SkipTake slice, ref NodeContainer<T> container)
    {
        slice.Of(memory.Length, out var start, out var count);
        if (count == 0)
            container.SetEmpty();
        else
            container.SetNode(new Node<T>(memory.Slice(start, count)));
    }

    // the memory is in forward order and the sequence is its reverse, so a slice from the sequence's start is one
    // from the memory's end
    internal static void ReverseSlice<T>(ReadOnlyMemory<T> memory, in Optimizations.SkipTake slice, ref NodeContainer<T> container)
    {
        slice.Of(memory.Length, out var start, out var count);
        if (count == 0)
            container.SetEmpty();
        else
            container.SetNode(new ReversedNode<T>(memory.Slice(memory.Length - start - count, count)));
    }

    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(ReadOnlyMemory<TSource> memory, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        try
        {
            // the loop is written here rather than in a helper taking fenum by ref, for the reason given at
            // SystemArray.ExecutePush; the threshold is the one it measured, a memory's span being the same shape
            var span = memory.Span;
            if (span.Length < 8
             || !TPushEnumerator.AllowBatchesWhen.Takes(BatchKinds.Container))
            {
                for (var i = 0; i < span.Length; ++i)
                {
                    if (!fenum.ProcessNext(span[i]))
                        break;
                }
            }
            else
            {
                var batch = new Batch<TSource>(span, BatchKinds.Container);
                _ = fenum.ProcessBatch(ref batch);
            }
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }
}
