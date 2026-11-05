namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;

public static partial class SelectManyResult
{
    [Operator]
    public partial struct Node<TSource, TCollection, TResult, NodeCollection, NodeSrcCol>
        : INode<TResult>
        , Optimizations.IOptimizationCount
        where NodeCollection : INode<TCollection>
        where NodeSrcCol : INode<(TSource, NodeCollection)>
    {
        internal NodeSrcCol _nodeSource;
        [Propagate] internal Func<TSource, TCollection, TResult> _resultSelector;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation();

        // the result selector is not run: only the inner sequences are counted
        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
            {
                response = default;

                return false;
            }

            response =
                _nodeSource.CreateViaPush<int, CountPush<TSource, TCollection, NodeCollection>, Construct<CountPush<TSource, TCollection, NodeCollection>, CountPush<TSource, TCollection, NodeCollection>.Args>>(default(ChainOptions), new Construct<CountPush<TSource, TCollection, NodeCollection>, CountPush<TSource, TCollection, NodeCollection>.Args>(default));

            return true;
        }
    }

    partial struct Push<TSource, TCollection, TResult, NodeCollection, Next>
    {
        // As in SelectMany.Push, the downstream chain is held by value for as long as every inner sequence exposes a
        // span, each inner span being run from it with the selector applied per element; the first inner sequence that
        // cannot moves the chain into _common, which owns it from then on, and later spans run against its chain.
        private ResultSelectorSink<TSource, TCollection, TResult, Next>? _common;
        private ChainOptions _options;

        // Once _common owns the chain, _next is a stale copy sharing its resources, so only the live one is disposed.
        public void Dispose()
        {
            if (_common != null)
                _common.Dispose();
            else
                _next.Dispose();
        }

        public TPushResult GetResult<TPushResult>() => _common != null ? _common.GetResult<TPushResult>() : _next.GetResult<TPushResult>();

        public bool ProcessNext((TSource, NodeCollection) input)
        {
            if (Optimizations.AsSpanSource.TryGet<TCollection, NodeCollection>(ref input.Item2, out var source))
            {
                ref Next next = ref (_common == null ? ref _next : ref _common.Chain);
                return typeof(Next).IsAssignableTo(typeof(IInternal__Aggregator__NotForManualUse))
                    ? ProcessSpanFromCopy(source.GetSpan(), input.Item1, _resultSelector, ref next)
                    : ProcessSpan(source.GetSpan(), input.Item1, _resultSelector, ref next);
            }

            _common ??= new ResultSelectorSink<TSource, TCollection, TResult, Next>(in _next, _resultSelector);
            _common.Source = input.Item1;
            input.Item2.CreateViaPush<bool, ErasedChain.PushForward<TCollection>, Construct<ErasedChain.PushForward<TCollection>, ErasedChain.PushForward<TCollection>.Args>>(in _options, new Construct<ErasedChain.PushForward<TCollection>, ErasedChain.PushForward<TCollection>.Args>(new ErasedChain.PushForward<TCollection>.Args(_common)));
            return !_common.Halted;
        }
    }

    // Each inner sequence is a node the outer chain yields, and is pulled through its own enumerator, created as the
    // outer chain moves onto it and disposed as the next replaces it.
    public partial struct Pull<TSource, TCollection, TResult, NodeCollection, NodeSrcCol, TSourceEnumerator, TCollectionEnumerator>
        where NodeCollection : INode<TCollection>
        where NodeSrcCol : INode<(TSource, NodeCollection)>
        where TSourceEnumerator : struct, IPullEnumerator<(TSource, NodeCollection)>, INodeEnumerator<NodeSrcCol, TSourceEnumerator>
        where TCollectionEnumerator : struct, IPullEnumerator<TCollection>, INodeEnumerator<NodeCollection, TCollectionEnumerator>
    {
        [AllowNull] private TSource _source;
        private TCollectionEnumerator _inner;
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
            for (; ; )
            {
                if (_hasInner && _inner.MoveNext())
                    return true;

                if (!MoveToNextInner())
                    return false;
            }
        }

        public TResult Current => _resultSelector(_source, _inner.Current);

        private bool MoveToNextInner()
        {
            if (_hasInner)
            {
                _hasInner = false;
                _inner.Dispose();
            }

            if (!_enumerator.MoveNext())
                return false;

            var (source, collection) = _enumerator.Current;
            _source = source;
            _inner = TCollectionEnumerator.Create(in collection);
            _hasInner = true;

            return true;
        }

        // as SelectMany's, through a sink that runs the selector before handing each element to the one given
        bool IPullEnumerator<TResult>.TryPushRest(ErasedChain.PushSink<TResult> sink)
        {
            var selecting = new ResultSelectorSink<TSource, TCollection, TResult, ErasedChain.PushForward<TResult>>(new ErasedChain.PushForward<TResult>(sink), _resultSelector);
            selecting.Source = _source;
            if (_hasInner)
                PullEnumerator.PushRest<TCollection, TCollectionEnumerator>(ref _inner, selecting);

            var forward = new ErasedChain.PushForward<TCollection>(selecting);
            while (!selecting.Halted)
            {
                if (_hasInner)
                {
                    _hasInner = false;
                    _inner.Dispose();
                }

                if (!_enumerator.MoveNext())
                    return true;

                var (source, collection) = _enumerator.Current;
                _source = source;
                selecting.Source = source;
                collection.CreateViaPush<bool, ErasedChain.PushForward<TCollection>, Downstream<ErasedChain.PushForward<TCollection>>>(default(ChainOptions), new Downstream<ErasedChain.PushForward<TCollection>>(in forward));
            }

            return true;
        }
    }

    private static bool ProcessSpan<TSource, TCollection, TResult, Next>(ReadOnlySpan<TCollection> span, TSource source, Func<TSource, TCollection, TResult> resultSelector, ref Next next)
        where Next : IPushEnumerator<TResult>
    {
        foreach (var item in span)
        {
            if (!next.ProcessNext(resultSelector(source, item)))
                return false;
        }
        return true;
    }

    // the aggregator runs the span from a local copy: Docs/BatchFromLocalCopy.md, "The fix" and "Constraints"
    private static bool ProcessSpanFromCopy<TSource, TCollection, TResult, Next>(ReadOnlySpan<TCollection> span, TSource source, Func<TSource, TCollection, TResult> resultSelector, ref Next next)
        where Next : IPushEnumerator<TResult>
    {
        var local = next;
        var result = true;
        foreach (var item in span)
        {
            if (!local.ProcessNext(resultSelector(source, item)))
            {
                result = false;
                break;
            }
        }
        next = local;
        return result;
    }

    // what a Count pushes the outer sequence into: each inner node counted, none of them run through the chain
    [Create]
    private partial struct CountPush<TSource, TCollection, NodeCollection>
        : IPushEnumerator<(TSource, NodeCollection)>
        where NodeCollection : INode<TCollection>
    {
        private SelectMany.InnerCounter<TCollection> _counter;

        public CountPush() { }

        public void Dispose() { }
        public TPushResult GetResult<TPushResult>() => (TPushResult)(object)_counter.Result;

        public bool ProcessNext((TSource, NodeCollection) input)
        {
            _counter.Add(ref input.Item2);
            return true;
        }
    }

    // what the inner sequences are pushed into, as a Concat's parts are: each element goes through the selector,
    // paired with the outer element whose inner sequence is being pushed, into the downstream
    private sealed class ResultSelectorSink<TSource, TCollection, TResult, Next>
        : ErasedChain.PushSink<TCollection>
        where Next : IPushEnumerator<TResult>
    {
        private Next _chain;
        private readonly Func<TSource, TCollection, TResult> _resultSelector;
        [AllowNull] public TSource Source;

        public ResultSelectorSink(in Next chain, Func<TSource, TCollection, TResult> resultSelector)
        {
            _chain = chain;
            _resultSelector = resultSelector;
            Source = default;
        }

        // every span batch is looped through the selector; a cold one is not taken
        public override bool Takes(BatchKinds requires) => (BatchKinds.Container | BatchKinds.Forwarder).Takes(requires);

        // the inner sequence's elements each paired with the source element through the selector
        public override bool ProcessBatch(scoped ref Batch<TCollection> batch)
        {
            foreach (var item in batch.GetSpan())
            {
                if (!ProcessNext(item))
                    return false;
            }
            return true;
        }

        public override bool ProcessNext(TCollection input)
        {
            if (_chain.ProcessNext(_resultSelector(Source, input)))
                return true;
            Halted = true;
            return false;
        }

        // for an owner that also runs the chain itself (Push running an inner span with the selector)
        public ref Next Chain => ref _chain;

        public TPushResult GetResult<TPushResult>() => _chain.GetResult<TPushResult>();
        public void Dispose() => _chain.Dispose();
    }
}
