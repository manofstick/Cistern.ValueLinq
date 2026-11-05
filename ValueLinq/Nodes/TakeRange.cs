namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Utils;
using System;

public static partial class TakeRange
{
    public struct Node<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        internal readonly int _start;
        internal readonly bool _startFromEnd;
        internal readonly int _end;
        internal readonly bool _endFromEnd;

        public Node(in NodeT nodeT, Range range)
        {
            _nodeT = nodeT;
            _start = range.Start.Value;
            _startFromEnd = range.Start.IsFromEnd;
            _end = range.End.Value;
            _endFromEnd = range.End.IsFromEnd;
        }

        // The range as a Skip then a Take, when it can be: both ends from the start, a range that is empty whatever the
        // length, or from-end indices against a length known now. The length is asked for each time, never held, as the
        // source may have changed since construction.
        internal bool TryResolve(out int skip, out int take)
        {
            if (TryResolveAtAnyLength(out skip, out take))
                return true;

            _nodeT.GetCountInformation(out var info);
            return TryResolveAtLength(in info, out skip, out take);
        }

        private readonly bool TryResolveAtAnyLength(out int skip, out int take)
        {
            if (!_startFromEnd && !_endFromEnd)
            {
                (skip, take) = (_start, _end - _start);
                return true;
            }

            if (_startFromEnd && (_start == 0 || (_endFromEnd && _end >= _start)))
            {
                (skip, take) = (0, 0);
                return true;
            }

            (skip, take) = (0, 0);
            return false;
        }

        private readonly bool TryResolveAtLength(in CountInformation info, out int skip, out int take)
        {
            if (!info.IsStale && info.ActualSize is int count)
            {
                var start = Math.Max(0, _startFromEnd ? count - _start : _start);
                var end = Math.Min(count, _endFromEnd ? count - _end : _end);
                (skip, take) = (start, end - start);
                return true;
            }

            (skip, take) = (0, 0);
            return false;
        }

        // the range as a Take over a Skip, the skipped node reached through this node's source
        private Take.Node<T, Skip.Node<T, NodeT>> Resolved(int skip, int take) =>
            new(new Skip.Node<T, NodeT>(in _nodeT, skip), take);

        // the source's information read once, and adjusted as the Take over a Skip it resolves to would adjust it
        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);

            if (TryResolveAtAnyLength(out var skip, out var take)
                || TryResolveAtLength(in info, out skip, out take))
            {
                Skip.CountInformationAfter(ref info, skip);
                Take.CountInformationAfter(ref info, take);
                return;
            }

            long? maximum =
                _startFromEnd
                    ? _endFromEnd ? _start - _end : Math.Min(_start, _end)
                    : info.MaximumLength is long length ? Math.Max(0, length - _start - _end) : null;

            if (maximum.HasValue)
                info.MaximumLength = Math.Min(info.MaximumLength ?? maximum.Value, maximum.Value);
            info.ActualLengthIsMaximumLength = false;
        }

        bool INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
        {
            if (TryResolve(out var skip, out var take))
            {
                var resolved = Resolved(skip, take);
                return TakeRange.TryPushOptimization<Take.Node<T, Skip.Node<T, NodeT>>, TRequest, TRequestResult>(ref resolved, in request, out result);
            }

            result = default;
            return false;
        }

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            if (TryResolve(out var skip, out var take))
            {
                if (SystemArray.TryPushSlice<T, NodeT, TPushResult, TPushEnumerator, TPushFiller>(in _nodeT, skip, take, in filler, out var sliced))
                    return sliced;

                var resolved = Resolved(skip, take);
                return TakeRange.CreateViaPush<T, Take.Node<T, Skip.Node<T, NodeT>>, TPushResult, TPushEnumerator, TPushFiller>(in options, ref resolved, in filler);
            }

            _nodeT.GetCountInformation(out var info);

            // an unresolved start from the start has its end from the end, which holds elements back as they arrive
            if (!_startFromEnd)
                return _nodeT.CreateViaPush<TPushResult, HoldBackPush<T, TPushEnumerator>, HoldBackFiller<T, TPushEnumerator, TPushFiller>>(in options, new HoldBackFiller<T, TPushEnumerator, TPushFiller>(_start, _end, info.MaximumLength, in filler));

            // a start from the end keeps the last start elements to the end of the source, whose push has disposed it
            // before the slice of them is pushed on, as System.Linq's TakeLast disposes before it yields
            var (items, count) = _nodeT.CreateViaPush<(T[]?, int), TakeLastCollector<T>, Construct<TakeLastCollector<T>, TakeLastCollector<T>.Args>>(in options, new Construct<TakeLastCollector<T>, TakeLastCollector<T>.Args>(new TakeLastCollector<T>.Args(_start, info.MaximumLength)));
            var length = FromEndLength(count, _start, _end, _endFromEnd);
            return SystemMemory.ExecutePush<T, TPushResult, TPushEnumerator>(new ReadOnlyMemory<T>(items, 0, length), PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
        }
    }

    // start..^end: after skipping start elements, each element is passed on once end more have arrived
    [Forwarder]
    partial struct HoldBackPush<T, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<T>
    {
        int _skip;
        RingBuffer<T> _buffer;

        // writes this forwarder's own fields into the chain being filled, and hands back the slot for its downstream
        [UnscopedRef]
        internal ref Next Fill(int skip, int holdBack, long? maximumLength)
        {
            _skip = skip;
            _buffer = new RingBuffer<T>(holdBack, maximumLength);
            return ref _next;
        }

        public bool ProcessNext(T input)
        {
            if (_skip > 0)
            {
                --_skip;
                return true;
            }

            return !_buffer.Add(input, out var evicted) || _next.ProcessNext(evicted);
        }
    }

    // A range that resolves (Node.TryResolve) is a skip then a take, and one that is empty whatever the source holds
    // never creates the source. One that does not resolve, a from-end index against an unknown length, buffers the
    // source as System.Linq buffers it.
    [Puller]
    public partial struct Pull<TIn, NodeT, TInEnumerator>
        where NodeT : INode<TIn>
        where TInEnumerator : struct, IPullEnumerator<TIn>, INodeEnumerator<NodeT, TInEnumerator>
    {
        private enum Mode : byte
        {
            Nothing,
            SkipTake,
            FromEnd,
            HoldingBackEnd,
        }

        private TInEnumerator _enumerator;
        private readonly Mode _mode;
        private readonly int _start;
        private readonly int _end;
        private readonly bool _endFromEnd;
        private readonly long? _maximumLength;
        private RingBuffer<TIn> _buffer;
        private TIn[]? _items;
        private int _index;
        private int _remaining;
        private bool _started;
        private bool _exhausted;
        private bool _disposed;

        // a range resolved to a skip then a take, or (take of 0) to nothing
        private Pull(in TInEnumerator enumerator, int skip, int take)
        {
            _enumerator = enumerator;
            _mode = take > 0 ? Mode.SkipTake : Mode.Nothing;
            _start = skip;
            _end = 0;
            _endFromEnd = false;
            _maximumLength = null;
            _buffer = default;
            _items = null;
            _index = 0;
            _remaining = take;
            _started = false;
            _exhausted = false;
            _disposed = _mode == Mode.Nothing;
        }

        private Pull(in TInEnumerator enumerator, int start, bool startFromEnd, int end, bool endFromEnd, long? maximumLength)
        {
            _enumerator = enumerator;
            _mode = startFromEnd ? Mode.FromEnd : Mode.HoldingBackEnd;
            _start = start;
            _end = end;
            _endFromEnd = endFromEnd;
            _maximumLength = maximumLength;
            _buffer = default;
            _items = null;
            _index = 0;
            _remaining = 0;
            _started = false;
            _exhausted = false;
            _disposed = false;
        }

        private static Pull<TIn, NodeT, TInEnumerator> Create(in Node<TIn, NodeT> node)
        {
            if (node.TryResolve(out var skip, out var take))
            {
                // as Take over Skip would: a take of nothing, or a skip past a source known to be no longer, never
                // creates the source
                var skipped = new Skip.Node<TIn, NodeT>(in node._nodeT, skip);
                if (take <= 0 || skipped.SkipsEverything())
                    return new(default, 0, 0);

                return new(TInEnumerator.Create(in node._nodeT), (int)skipped._count, take);
            }

            var enumerator = TInEnumerator.Create(in node._nodeT);
            node._nodeT.GetCountInformation(out var info);
            return new(in enumerator, node._start, node._startFromEnd, node._end, node._endFromEnd, info.MaximumLength);
        }

        static bool IPullEnumerator<TIn>.RebuildChanges => TInEnumerator.RebuildChanges;

        static TBuilt IPullEnumerator<TIn>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
        {
            ref readonly var takeRange = ref ChainBuilder.As<TOwnNode, Node<TIn, NodeT>>(in node);
            var start = new Index(takeRange._start, takeRange._startFromEnd);
            var end = new Index(takeRange._end, takeRange._endFromEnd);
            var above = new ChainBuilder<TIn, TBuilt, TBuilder>(new Range(start, end), in builder);
            return TInEnumerator.Rebuild<NodeT, TBuilt, ChainBuilder<TIn, TBuilt, TBuilder>>(in takeRange._nodeT, in above);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _enumerator.Dispose();
        }

        [AllowNull] private TIn _current;

        public bool MoveNext() =>
            _mode switch
            {
                Mode.SkipTake => MoveNextSkipTake(),
                Mode.FromEnd => MoveNextFromEnd(),
                Mode.HoldingBackEnd => MoveNextHoldingBackEnd(),
                _ => false,
            };

        // a resolved range is read from the source on read, so a selector below runs once, on read
        public TIn Current => _mode == Mode.SkipTake ? _enumerator.Current : _current;

        // skip..skip+take: the skipped elements are still produced, so a selector below runs (and throws) for them
        private bool MoveNextSkipTake()
        {
            if (!_started)
            {
                _started = true;
                for (var i = 0; i < _start; ++i)
                {
                    if (!_enumerator.MoveNext())
                    {
                        _remaining = 0;
                        return false;
                    }
                    _ = _enumerator.Current;
                }
            }

            if (_remaining <= 0)
                return false;
            --_remaining;
            return _enumerator.MoveNext();
        }

        // ^start..end: only the last start elements can be in range, so they are buffered to the end of the source
        // before the slice of them that end allows is yielded; the source is disposed once buffered, before the first
        // element is yielded, as System.Linq's TakeLast does
        private bool MoveNextFromEnd()
        {
            if (!_started)
            {
                _started = true;

                var buffer = new RingBuffer<TIn>(_start, _maximumLength);
                var count = 0;
                while (_enumerator.MoveNext())
                {
                    buffer.Add(_enumerator.Current, out _);
                    checked { ++count; }
                }
                Dispose();

                _remaining = FromEndLength(count, _start, _end, _endFromEnd);
                _items = buffer.Linearize();
            }

            if (_remaining > 0)
            {
                --_remaining;
                _current = _items![_index++];
                return true;
            }

            return false;
        }

        // start..^end: after skipping start elements, each element is yielded once end more have been seen
        private bool MoveNextHoldingBackEnd()
        {
            if (!_started)
            {
                _started = true;

                for (var i = 0; i < _start; ++i)
                {
                    if (!_enumerator.MoveNext())
                    {
                        _exhausted = true;
                        return false;
                    }
                    // a skipped element is still produced, so a selector below runs (and throws) for it
                    _ = _enumerator.Current;
                }

                _buffer = new RingBuffer<TIn>(_end, _maximumLength);
            }

            if (!_exhausted)
            {
                while (_enumerator.MoveNext())
                {
                    if (_buffer.Add(_enumerator.Current, out var evicted))
                    {
                        _current = evicted;
                        return true;
                    }
                }
            }

            return false;
        }
    }

    readonly ref struct ChainBuilder<TIn, TBuilt, TBuilder>
        : IChainBuilder<TIn, TBuilt>
        where TBuilder : IChainBuilder<TIn, TBuilt>, allows ref struct
    {
        private readonly Range _range;
        private readonly ref readonly ErasedRef _builder;

        public ChainBuilder(Range range, in TBuilder builder)
        {
            _range = range;
            _builder = ref Erase.Ref(in builder);
        }

        TBuilt IChainBuilder<TIn, TBuilt>.Build<TNode, TEnumerator>(in TNode node)
            => Erase.Deref<TBuilder>(in _builder).Build<Node<TIn, TNode>, Pull<TIn, TNode, TEnumerator>>(new Node<TIn, TNode>(in node, _range));
    }

    // ^start..: the last start elements, oldest first, and how many the source had
    [Aggregator]
    partial struct TakeLastCollector<T>
        : IPushEnumerator<T>
    {
        RingBuffer<T> _buffer;
        int _count;

        public TakeLastCollector(int capacity, long? maximumLength)
        {
            _buffer = new RingBuffer<T>(capacity, maximumLength);
            _count = 0;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            foreach (var item in span)
                _buffer.Add(item, out _);
            checked { _count += span.Length; }
            return true;
        }

        public TPushResult GetResult<TPushResult>() => Impl.ResultCast<(T[]?, int), TPushResult>((_buffer.Linearize(), _count));

        public bool ProcessNext(T input)
        {
            _buffer.Add(input, out _);
            checked { ++_count; }
            return true;
        }
    }

    // ^start..end over a source of count elements, of which the last start were kept: the slice of them in range,
    // from their beginning
    private static int FromEndLength(int count, int start, int end, bool endFromEnd)
    {
        var first = Math.Max(0, count - start);
        var last = Math.Min(count, endFromEnd ? count - end : end);
        return Math.Max(0, last - first);
    }

    // constrained calls, so the resolved node is reached through its explicit interface members without boxing
    private static bool TryPushOptimization<Node, TRequest, TResult>(ref Node node, in TRequest request, [MaybeNullWhen(false)] out TResult result)
        where Node : INode
        where TRequest : notnull
        => node.TryPushOptimization<TRequest, TResult>(in request, out result);

    private static TResult CreateViaPush<T, Node, TResult, TPushEnumerator, TFiller>(in ChainOptions options, ref Node node, in TFiller filler)
        where Node : INode<T>
        where TPushEnumerator : IPushEnumerator<T>
        where TFiller : IPushFiller<TPushEnumerator>, allows ref struct
        => node.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler);
}
