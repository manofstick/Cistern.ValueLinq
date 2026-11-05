namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;

public static partial class SelectMany
{
    [Operator]
    public partial struct Node<U, NodeU, NodeEU>
        : INode<U>
        , Optimizations.IOptimizationCount
        where NodeU : INode<U>
        where NodeEU : INode<NodeU>
    {
        internal NodeEU _nodeEU;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation();

        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
            {
                response = default;

                return false;
            }

            response =
                _nodeEU.CreateViaPush<int, CountPush<U, NodeU>, Construct<CountPush<U, NodeU>, CountPush<U, NodeU>.Args>>(default(ChainOptions), new Construct<CountPush<U, NodeU>, CountPush<U, NodeU>.Args>(default));

            return true;
        }
    }

    partial struct Push<U, NodeU, Next>
    {
        // The downstream chain is held by value for as long as every inner sequence can expose its elements as a
        // span (arrays, memory, lists): each inner span is then offered to the chain as a batch (so a Select or
        // Where directly above the aggregator can run it from a local copy, see Docs/BatchFromLocalCopy.md) or
        // looped here. The first inner sequence that cannot moves the chain into _common, which owns it from then
        // on: an inner node's push copies the enumerator it is given, so a shared reference is the only way to
        // continue one chain across those pushes. Inner sequences are pushed into it as a Concat pushes its parts, so
        // an inner Cistern chain met through IEnumerable<T> pushes into this sink rather than building one of its own.
        private ErasedChain.PushSink<U, Next>? _common;
        private ChainOptions _options;

        // Once _common owns the chain, _next is a stale copy sharing its resources (a pooled collector's rented
        // arrays), so only the live one is disposed.
        public void Dispose()
        {
            if (_common != null)
                _common.Dispose();
            else
                _next.Dispose();
        }

        public TPushResult GetResult<TPushResult>() => _common != null ? _common.GetResult<TPushResult>() : _next.GetResult<TPushResult>();

        public bool ProcessNext(NodeU input)
        {
            if (Optimizations.AsSpanSource.TryGet<U, NodeU>(ref input, out var source))
            {
                ref Next next = ref (_common == null ? ref _next : ref _common.Chain);

                var span = source.GetSpan();
                if (Next.AllowBatchesWhen.Takes(BatchKinds.Container))
                {
                    var batch = new Batch<U>(span, BatchKinds.Container);
                    return next.ProcessBatch(ref batch);
                }

                return ProcessSpan(span, ref next);
            }

            _common ??= new ErasedChain.PushSink<U, Next>(in _next);
            input.CreateViaPush<bool, ErasedChain.PushForward<U>, Construct<ErasedChain.PushForward<U>, ErasedChain.PushForward<U>.Args>>(in _options, new Construct<ErasedChain.PushForward<U>, ErasedChain.PushForward<U>.Args>(new ErasedChain.PushForward<U>.Args(_common)));
            return !_common.Halted;
        }
    }

    // Each inner sequence is a node the outer chain yields, and is pulled through its own enumerator, created as the
    // outer chain moves onto it and disposed as the next replaces it.
    public partial struct Pull<U, NodeU, NodeEU, TOuterEnumerator, TInnerEnumerator>
        where NodeU : INode<U>
        where NodeEU : INode<NodeU>
        where TOuterEnumerator : struct, IPullEnumerator<NodeU>, INodeEnumerator<NodeEU, TOuterEnumerator>
        where TInnerEnumerator : struct, IPullEnumerator<U>, INodeEnumerator<NodeU, TInnerEnumerator>
    {
        private TInnerEnumerator _inner;
        private bool _hasInner;

        public void Dispose()
        {
            if (_hasInner)
            {
                _hasInner = false;
                _inner.Dispose();
            }
            _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            for (;;)
            {
                if (_hasInner && _inner.MoveNext())
                    return true;

                if (!MoveToNextInner())
                    return false;
            }
        }

        public U Current => _inner.Current;

        private bool MoveToNextInner()
        {
            if (_hasInner)
            {
                _hasInner = false;
                _inner.Dispose();
            }

            if (!_enumerator.MoveNext())
                return false;

            var node = _enumerator.Current;
            _inner = TInnerEnumerator.Create(in node);
            _hasInner = true;

            return true;
        }

        // the inner sequence already open is drained into the sink, and the ones not yet opened are pushed into it
        // rather than opened, as a Concat's parts are
        bool IPullEnumerator<U>.TryPushRest(ErasedChain.PushSink<U> sink)
        {
            if (_hasInner)
                PullEnumerator.PushRest<U, TInnerEnumerator>(ref _inner, sink);

            var forward = new ErasedChain.PushForward<U>(sink);
            while (!sink.Halted)
            {
                if (_hasInner)
                {
                    _hasInner = false;
                    _inner.Dispose();
                }

                if (!_enumerator.MoveNext())
                    return true;

                var node = _enumerator.Current;
                node.CreateViaPush<bool, ErasedChain.PushForward<U>, Downstream<ErasedChain.PushForward<U>>>(default(ChainOptions), new Downstream<ErasedChain.PushForward<U>>(in forward));
            }

            return true;
        }
    }

    private static bool ProcessSpan<U, Next>(ReadOnlySpan<U> span, ref Next next)
        where Next : IPushEnumerator<U>
    {
        foreach (var item in span)
        {
            if (!next.ProcessNext(item))
                return false;
        }
        return true;
    }

    // counts each inner sequence as NodeImpl.Count does, except that one which has to be pushed to be counted is
    // pushed into a sink shared by the whole count, as a Concat's parts are, rather than given a chain of its own
    internal struct InnerCounter<U>
    {
        private int _count;
        private ErasedChain.PushSink<U, Aggregation.Count<U>>? _pushed;

        public void Add<NodeU>(ref NodeU node)
            where NodeU : INode<U>
        {
            if (NodeImpl.TryCountWithoutPush<U, NodeU>(in node, false, out var count))
            {
                _count = checked(_count + count);
                return;
            }

            _pushed ??= new ErasedChain.PushSink<U, Aggregation.Count<U>>(new Aggregation.Count<U>());
            node.CreateViaPush<bool, ErasedChain.PushForward<U>, Construct<ErasedChain.PushForward<U>, ErasedChain.PushForward<U>.Args>>(default(ChainOptions), new Construct<ErasedChain.PushForward<U>, ErasedChain.PushForward<U>.Args>(new ErasedChain.PushForward<U>.Args(_pushed)));
        }

        public readonly int Result => _pushed == null ? _count : checked(_count + _pushed.GetResult<int>());
    }

    // what a Count pushes the outer sequence into: each inner node counted, none of them run through the chain
    [Create]
    private partial struct CountPush<U, NodeU>
        : IPushEnumerator<NodeU>
        where NodeU : INode<U>
    {
        private InnerCounter<U> _counter;

        public CountPush() { }

        public void Dispose() { }
        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)_counter.Result;

        public bool ProcessNext(NodeU input)
        {
            _counter.Add(ref input);
            return true;
        }
    }
}
