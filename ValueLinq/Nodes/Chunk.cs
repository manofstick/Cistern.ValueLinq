namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Containers;
using System;
using System.Runtime.CompilerServices;

public static partial class Chunk
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<T[]>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<T[]>
        , Optimizations.IOptimizationCopyTo<T[]>
        , Optimizations.IOptimizationSkipTake<T[]>
        where NodeT : INode<T>
    {
        // POTENTIAL OPTIMIZATION: Reverse, over a source answering AsSpanSource, as the span's chunks taken from the last (the short one) back to the first; any other source needs its length known when the request is asked to place the chunk boundaries
        internal NodeT _nodeT;
        [Propagate] internal int _size;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            if (info.MaximumLength is long maximumLength)
                info.MaximumLength = ChunkCount(maximumLength, _size);
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            int sourceCount;
            if (!request.DryRun)
                sourceCount = NodeImpl.Count<T, NodeT>(in _nodeT, request.IgnorePotentialSideEffects);
            else if (!NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out sourceCount))
            {
                response = default;

                return false;
            }

            response = (int)ChunkCount(sourceCount, _size);

            return true;
        }

        private bool CopyTo(in Optimizations.CopyTo<T[]> request, out int response)
        {
            if (!Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = default;

                return false;
            }

            var items = source.GetSpan();
            var chunkCount = (int)ChunkCount(items.Length, _size);
            response = chunkCount;
            if (request.IsDryRun)
                return true;

            var destination = request.Span;
            for (var i = 0; i < chunkCount; ++i)
                destination[i] = ChunkAt(items, i, _size);

            return true;
        }

        // the source's slice from a chunk boundary, chunked
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T[]> response)
        {
            var sourceSkip =
                request.Skip > (long.MaxValue / _size)
                    ? long.MaxValue
                    : request.Skip * _size;
            var sourceTake =
                request.Take > (long.MaxValue / _size)
                    ? long.MaxValue
                    : request.Take * _size;

            var sourceSlice = default(NodeContainer<T>);
            if (!Optimizations.SkipTake.Answer<T, NodeT>(ref _nodeT, sourceSkip, sourceTake, ref sourceSlice))
            {
                response = default;

                return false;
            }

            Unsafe.SkipInit(out response);
            if (sourceSlice.Type == NodeType.Empty)
                response.SetEmpty();
            else
                response.SetNode(new Node<T, NodeContainerNode<T>>(new NodeContainerNode<T>(in sourceSlice), _size));

            return true;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T[]) response)
        {
            if (!Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = default;

                return false;
            }

            var items = source.GetSpan();
            var chunkCount = (int)ChunkCount(items.Length, _size);

            response =
                request.OffsetIn(chunkCount) switch
                {
                    < 0 => (false, default!),
                    var chunkIndex => (true, ChunkAt(items, chunkIndex, _size)),
                };

            return true;
        }
    }

    partial struct Push<T, Next>
    {
        private T[]? _chunk;
        private int _count;
        private int _arraySize;

        partial void Start() => _arraySize = Math.Min(_size, InitialArraySize);

        // a cold batch only over a downstream that reads every chunk, as one does that takes cold
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen =>
            BatchKinds.Container
            | BatchKinds.Forwarder
            | ((Next.AllowBatchesWhen & BatchKinds.Cold) != 0
                ? BatchKinds.ColdSpan
                : BatchKinds.None);

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var remaining = span;
            var downstreamTakesMore = true;
            if (_count > 0)
            {
                var topUpLength = Math.Min(_size - _count, remaining.Length);
                var topUp = remaining[..topUpLength];
                Append(topUp);
                remaining = remaining[topUp.Length..];

                downstreamTakesMore =
                    _count < _size
                    || PushChunk();
            }

            while (downstreamTakesMore && remaining.Length >= _size)
            {
                var chunk = remaining[.._size].ToArray();
                downstreamTakesMore = _next.ProcessNext(chunk);
                remaining = remaining[_size..];
            }

            if (downstreamTakesMore
                && !remaining.IsEmpty)
            {
                Append(remaining);
            }

            return downstreamTakesMore;
        }

        public bool ProcessNext(T input)
        {
            EnsureCapacity(_count + 1);
            _chunk[_count++] = input;

            var downstreamTakesMore =
                _count < _size
                || PushChunk();

            return downstreamTakesMore;
        }

        public TPushResult GetResult<TPushResult>()
        {
            if (_count > 0)
                _ = PushChunk();

            return _next.GetResult<TPushResult>();
        }

        private void Append(ReadOnlySpan<T> items)
        {
            EnsureCapacity(_count + items.Length);
            items.CopyTo(_chunk.AsSpan(_count));
            _count += items.Length;
        }

        [MemberNotNull(nameof(_chunk))]
        private void EnsureCapacity(int capacity)
        {
            if (_chunk is null)
            {
                _chunk = new T[Math.Max(_arraySize, capacity)];
            }
            else if (_chunk.Length < capacity)
            {
                _arraySize = Grown(_chunk.Length, capacity, _size);
                Array.Resize(ref _chunk, _arraySize);
            }
        }

        private bool PushChunk()
        {
            var chunk = _chunk!;
            if (_count != chunk.Length)
                Array.Resize(ref chunk, _count);

            _chunk = null;
            _count = 0;

            return _next.ProcessNext(chunk);
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        private T[]? _chunk;
        private int _arraySize;
        private bool _sourceEnded;
        // a source answering a stable AsSpanSource (an array, a memory) is not enumerated: its chunks are copied out of
        // its span, reread each MoveNext from _offset, and _enumerator is never created. A List<T> declines a stable
        // request, as the list can change between chunks
        private bool _fromSpan;
        private Optimizations.SpanSource<T> _spanSource;
        private int _offset;

        partial void Start() => _arraySize = Math.Min(_size, InitialArraySize);

        static Pull<T, NodeT, TInEnumerator> INodeEnumerator<Node<T, NodeT>, Pull<T, NodeT, TInEnumerator>>.Create(in Node<T, NodeT> node)
        {
            var source = node._nodeT;
            if (!Optimizations.AsSpanSource.TryGet<T, NodeT>(ref source, out var spanSource, stable: true))
                return new(TInEnumerator.Create(in node._nodeT), node._size);

            var pull = new Pull<T, NodeT, TInEnumerator>(default, node._size);
            pull._fromSpan = true;
            pull._spanSource = spanSource;

            return pull;
        }

        public void Dispose()
        {
            if (!_fromSpan)
                _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            if (_fromSpan)
                return MoveNextFromSpan();

            if (_sourceEnded)
                return false;

            if (!_enumerator.MoveNext())
            {
                _sourceEnded = true;

                return false;
            }

            var chunk = new T[_arraySize];
            chunk[0] = _enumerator.Current;

            // filled to the end of the array it has (never longer than the size) before it grows, so each store is in
            // bounds by the loop's own test; replaced rather than resized by ref, which would keep it on the stack
            var count = 1;
            while (true)
            {
                while (count < chunk.Length && _enumerator.MoveNext())
                    chunk[count++] = _enumerator.Current;

                if (count < chunk.Length
                    || count == _size)
                {
                    break;
                }

                _arraySize = Grown(chunk.Length, count + 1, _size);
                chunk = Resized(chunk, _arraySize);
            }

            _sourceEnded = count < _size;
            if (count != chunk.Length)
                chunk = Resized(chunk, count);

            _chunk = chunk;

            return true;
        }

        private bool MoveNextFromSpan()
        {
            var rest = _spanSource.GetSpan()[_offset..];
            if (rest.IsEmpty)
                return false;

            var length = Math.Min(_size, rest.Length);
            _chunk = rest[..length].ToArray();
            _offset += length;

            return true;
        }

        public T[] Current => _chunk!;
    }

    // a chunk's array starts small and doubles, so a large size over a short source is never allocated up front
    private const int InitialArraySize = 4;

    private static int Grown(int length, int capacity, int size)
    {
        var grown = (int)Math.Min(size, Math.Max(capacity, 2L * length));

        return grown;
    }

    private static T[] Resized<T>(T[] array, int length)
    {
        Array.Resize(ref array, length);

        return array;
    }

    private static long ChunkCount(long length, int size)
    {
        var chunkCount = (length / size) + Math.Sign(length % size);

        return chunkCount;
    }

    private static T[] ChunkAt<T>(ReadOnlySpan<T> items, int chunkIndex, int size)
    {
        var start = chunkIndex * size;
        var length = Math.Min(size, items.Length - start);
        var chunk = items.Slice(start, length).ToArray();

        return chunk;
    }
}
