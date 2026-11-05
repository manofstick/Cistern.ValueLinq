namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;

public static partial class Zip
{
    // NodeUEnumerator is part of the node's type, as its push pulls the second sequence
    [Answerer]
    public partial struct Node<T, U, TResult, NodeT, NodeU, NodeUEnumerator, TSelector>
        : INode<TResult>
        , Optimizations.IOptimizationElementAt<TResult>
        , Optimizations.IOptimizationCopyTo<TResult>
        where NodeT : INode<T>
        where NodeU : INode<U>
        where NodeUEnumerator : struct, IPullEnumerator<U>, INodeEnumerator<NodeU, NodeUEnumerator>
        where TSelector : struct, ISelector<T, U, TResult>
    {
        // POTENTIAL OPTIMIZATION: SkipTake and Reverse for a selector with no side effects, as this node over both sources' slices (each cut to the shorter length to reverse), which needs a pull enumerator over NodeContainerNode<U>
        internal NodeT _nodeT;
        internal NodeU _nodeU;
        internal TSelector _selector;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            _nodeU.GetCountInformation(out var infoU);

            if (info.MaximumLength.HasValue && infoU.MaximumLength.HasValue)
            {
                info.MaximumLength = Math.Min(info.MaximumLength.Value, infoU.MaximumLength.Value);
                info.ActualLengthIsMaximumLength &= infoU.ActualLengthIsMaximumLength;
                info.LengthIsImmutable &= infoU.LengthIsImmutable;
                info.IsStale |= infoU.IsStale;
                info.PotentialSideEffects |= infoU.PotentialSideEffects;
            }
            else
            {
                info.MaximumLength = null;
                info.ActualLengthIsMaximumLength = false;
            }

            info.PotentialSideEffects |= TSelector.PotentialSideEffects;
        }

        public Node(in NodeT nodeT, in NodeU nodeU, TSelector selector)
        {
            _nodeT = nodeT;
            _nodeU = nodeU;
            _selector = selector;
        }

        TPushResult INode<TResult>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            // two containers with spans (an array, a list, a memory) are zipped by index; only a container answers
            // AsSpanSource, so asking runs no user code
            if (Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var sourceT)
                && Optimizations.AsSpanSource.TryGet<U, NodeU>(ref _nodeU, out var sourceU))
            {
                return FastEnumerate<T, U, TResult, TSelector, TPushResult, TPushEnumerator>(sourceT.GetSpan(), sourceU.GetSpan(), _selector, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
            }

            // the second sequence is held behind its own heap enumerator, so copies of the push chain share its position;
            // opened before the first is pushed, it is released here when the push fails before the chain holding it starts
            var eu = PullEnumerator.Create<U, NodeU, NodeUEnumerator>(in _nodeU);

            var started = false;
            try
            {
                var guard = new Guard<TPushEnumerator, TPushFiller>(ref started, in filler);
                return _nodeT.CreateViaPush<TPushResult, Push<T, U, TResult, TPushEnumerator, TSelector>, Filler<T, U, TResult, TPushEnumerator, TSelector, Guard<TPushEnumerator, TPushFiller>>>(in options, new Filler<T, U, TResult, TPushEnumerator, TSelector, Guard<TPushEnumerator, TPushFiller>>(eu, _selector, in guard));
            }
            finally
            {
                if (!started)
                    eu.Dispose();
            }
        }

        // a result selector runs on every element before the one asked, as System.Linq runs it, so only a tuple is answered
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, TResult) response)
        {
            response = default;
            if (TSelector.PotentialSideEffects)
                return false;

            var index = request.Index;
            int position;
            if (!index.IsFromEnd)
            {
                position = index.Value;
            }
            else
            {
                _nodeT.GetCountInformation(out var infoT);
                _nodeU.GetCountInformation(out var infoU);
                if (infoT.IsStale
                    || infoU.IsStale
                    || infoT.ActualSize is not int lengthT
                    || infoU.ActualSize is not int lengthU)
                {
                    return false;
                }

                var length = Math.Min(lengthT, lengthU);

                position = length - index.Value;
                if (position < 0)
                    return true;
            }

            var at = new Optimizations.ElementAt(position);
            if (!_nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(in at, out var answerT))
                return false;

            if (!answerT.Item1)
                return true;

            if (!_nodeU.TryPushOptimization<Optimizations.ElementAt, (bool, U)>(in at, out var answerU))
                answerU = PullAt(position);

            if (answerU.Item1)
                response = (true, _selector.Invoke(answerT.Item2, answerU.Item2));

            return true;
        }

        // the first source has answered, by running whatever user code it holds (an OrderBy's selection), which a push
        // would run again; the second is pulled to the position instead, reading each element's Current as the push does
        private (bool, U) PullAt(int position)
        {
            var enumerator = NodeUEnumerator.Create(in _nodeU);
            try
            {
                for (var i = 0; i < position; ++i)
                {
                    if (!enumerator.MoveNext())
                        return (false, default!);

                    _ = enumerator.Current;
                }

                var element =
                    enumerator.MoveNext()
                        ? (true, enumerator.Current)
                        : (false, default!);

                return element;
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private bool CopyTo(in Optimizations.CopyTo<TResult> request, out int response)
        {
            if (!Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var sourceT)
                || !Optimizations.AsSpanSource.TryGet<U, NodeU>(ref _nodeU, out var sourceU))
            {
                response = default;

                return false;
            }

            var ts = sourceT.GetSpan();
            var us = sourceU.GetSpan();
            var length = Math.Min(ts.Length, us.Length);
            response = length;
            if (request.IsDryRun)
                return true;

            var destination = request.Span;
            for (var i = 0; i < length; ++i)
                destination[i] = _selector.Invoke(ts[i], us[i]);

            return true;
        }
    }

    public interface ISelector<T, U, TResult>
    {
        static abstract bool PotentialSideEffects { get; }

        TResult Invoke(T t, U u);
    }

    public readonly struct PairSelector<T, U>
        : ISelector<T, U, (T, U)>
    {
        static bool ISelector<T, U, (T, U)>.PotentialSideEffects => false;

        (T, U) ISelector<T, U, (T, U)>.Invoke(T t, U u) => (t, u);
    }

    public readonly struct FuncSelector<T, U, TResult>(Func<T, U, TResult> resultSelector)
        : ISelector<T, U, TResult>
    {
        static bool ISelector<T, U, TResult>.PotentialSideEffects => true;

        TResult ISelector<T, U, TResult>.Invoke(T t, U u) => resultSelector(t, u);
    }

    public readonly struct TripleSelector<T, U, V>
        : ISelector<(T, U), V, (T, U, V)>
    {
        static bool ISelector<(T, U), V, (T, U, V)>.PotentialSideEffects => false;

        (T, U, V) ISelector<(T, U), V, (T, U, V)>.Invoke((T, U) pair, V v) => (pair.Item1, pair.Item2, v);
    }

    [Forwarder]
    partial struct Push<T, U, TResult, Next, TSelector>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<TResult>
        where TSelector : struct, ISelector<T, U, TResult>
    {
        PullEnumerator<U> _eu;
        TSelector _selector;

        // writes this forwarder's own fields into the chain being filled, and hands back the slot for its downstream
        [UnscopedRef]
        internal ref Next Fill(PullEnumerator<U> eu, TSelector selector)
        {
            _eu = eu;
            _selector = selector;
            return ref _next;
        }

        // the second sequence is a source of this forwarder's own, released before the result is taken as a container
        // releases its source, and by Dispose only when no result was taken
        public TPushResult GetResult<TPushResult>()
        {
            var eu = _eu;
            _eu = PullEnumerator<U>.Empty;
            eu.Dispose();
            return _next.GetResult<TPushResult>();
        }

        public void Dispose() { try { _eu.Dispose(); } finally { _next.Dispose(); } }

        public bool ProcessNext(T t)
        {
            if (!_eu.TryGetNext(out var u))
                return false;

            var result = _selector.Invoke(t, u);

            return _next.ProcessNext(result);
        }
    }

    [Puller]
    public partial struct Pull<T, U, TResult, NodeT, NodeU, NodeUEnumerator, TSelector, TEnumerator>
        where NodeT : INode<T>
        where NodeU : INode<U>
        where NodeUEnumerator : struct, IPullEnumerator<U>, INodeEnumerator<NodeU, NodeUEnumerator>
        where TSelector : struct, ISelector<T, U, TResult>
        where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<NodeT, TEnumerator>
    {
        private TEnumerator _enumerator1;
        private NodeUEnumerator _enumerator2;
        private TSelector _selector;

        private Pull(in TEnumerator enumerator1, in NodeUEnumerator enumerator2, TSelector selector)
        {
            _enumerator1 = enumerator1;
            _enumerator2 = enumerator2;
            _selector = selector;
        }

        private static Pull<T, U, TResult, NodeT, NodeU, NodeUEnumerator, TSelector, TEnumerator> Create(in Node<T, U, TResult, NodeT, NodeU, NodeUEnumerator, TSelector> node)
        {
            var enumerator1 = TEnumerator.Create(in node._nodeT);
            var enumerator2 = NodeUEnumerator.Create(in node._nodeU);

            return new(in enumerator1, in enumerator2, node._selector);
        }

        // both sequences are rebuilt, the first's builder rebuilding the second
        static bool IPullEnumerator<TResult>.RebuildChanges =>
            TEnumerator.RebuildChanges
            || NodeUEnumerator.RebuildChanges;

        static TBuilt IPullEnumerator<TResult>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
        {
            ref readonly var zip = ref ChainBuilder.As<TOwnNode, Node<T, U, TResult, NodeT, NodeU, NodeUEnumerator, TSelector>>(in node);
            var above = new FirstChainBuilder<T, U, TResult, NodeU, NodeUEnumerator, TSelector, TBuilt, TBuilder>(in zip._nodeU, zip._selector, in builder);

            return TEnumerator.Rebuild<NodeT, TBuilt, FirstChainBuilder<T, U, TResult, NodeU, NodeUEnumerator, TSelector, TBuilt, TBuilder>>(in zip._nodeT, in above);
        }

        public void Dispose() { try { _enumerator1.Dispose(); } finally { _enumerator2.Dispose(); }; }

        public bool MoveNext() => _enumerator1.MoveNext() && _enumerator2.MoveNext();

        public TResult Current => _selector.Invoke(_enumerator1.Current, _enumerator2.Current);
    }

    readonly ref struct FirstChainBuilder<T, U, TResult, NodeU, NodeUEnumerator, TSelector, TBuilt, TBuilder>
        : IChainBuilder<T, TBuilt>
        where NodeU : INode<U>
        where NodeUEnumerator : struct, IPullEnumerator<U>, INodeEnumerator<NodeU, NodeUEnumerator>
        where TSelector : struct, ISelector<T, U, TResult>
        where TBuilder : IChainBuilder<TResult, TBuilt>, allows ref struct
    {
        private readonly ref readonly NodeU _nodeU;
        private readonly TSelector _selector;
        private readonly ref readonly ErasedRef _builder;

        public FirstChainBuilder(in NodeU nodeU, TSelector selector, in TBuilder builder)
        {
            _nodeU = ref nodeU;
            _selector = selector;
            _builder = ref Erase.Ref(in builder);
        }

        TBuilt IChainBuilder<T, TBuilt>.Build<TNode, TEnumerator>(in TNode node)
        {
            var above = new SecondChainBuilder<T, U, TResult, TNode, TEnumerator, TSelector, TBuilt, TBuilder>(in node, _selector, in Erase.Deref<TBuilder>(in _builder));

            return NodeUEnumerator.Rebuild<NodeU, TBuilt, SecondChainBuilder<T, U, TResult, TNode, TEnumerator, TSelector, TBuilt, TBuilder>>(in _nodeU, in above);
        }
    }

    readonly ref struct SecondChainBuilder<T, U, TResult, NodeT, TSourceEnumerator, TSelector, TBuilt, TBuilder>
        : IChainBuilder<U, TBuilt>
        where NodeT : INode<T>
        where TSourceEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<NodeT, TSourceEnumerator>
        where TSelector : struct, ISelector<T, U, TResult>
        where TBuilder : IChainBuilder<TResult, TBuilt>, allows ref struct
    {
        private readonly ref readonly NodeT _nodeT;
        private readonly TSelector _selector;
        private readonly ref readonly ErasedRef _builder;

        public SecondChainBuilder(in NodeT nodeT, TSelector selector, in TBuilder builder)
        {
            _nodeT = ref nodeT;
            _selector = selector;
            _builder = ref Erase.Ref(in builder);
        }

        TBuilt IChainBuilder<U, TBuilt>.Build<TNode, TEnumerator>(in TNode node)
        {
            var zip = new Node<T, U, TResult, NodeT, TNode, TEnumerator, TSelector>(in _nodeT, in node, _selector);

            var built = Erase.Deref<TBuilder>(in _builder).Build<Node<T, U, TResult, NodeT, TNode, TEnumerator, TSelector>, Pull<T, U, TResult, NodeT, TNode, TEnumerator, TSelector, TSourceEnumerator>>(in zip);

            return built;
        }
    }

    private static TPushResult FastEnumerate<T, U, TResult, TSelector, TPushResult, TPushEnumerator>(ReadOnlySpan<T> ts, ReadOnlySpan<U> us, TSelector selector, TPushEnumerator fenum)
        where TSelector : struct, ISelector<T, U, TResult>
        where TPushEnumerator : IPushEnumerator<TResult>
    {
        try
        {
            ProcessSpans<T, U, TResult, TSelector, TPushEnumerator>(ts, us, selector, ref fenum);

            return fenum.GetResult<TPushResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    private static void ProcessSpans<T, U, TResult, TSelector, TPushEnumerator>(ReadOnlySpan<T> ts, ReadOnlySpan<U> us, TSelector selector, ref TPushEnumerator fenum)
        where TSelector : struct, ISelector<T, U, TResult>
        where TPushEnumerator : IPushEnumerator<TResult>
    {
        var len = Math.Min(ts.Length, us.Length);
        for (var i = 0; i < len; ++i)
        {
            var result = selector.Invoke(ts[i], us[i]);
            if (!fenum.ProcessNext(result))
                break;
        }
    }
}
