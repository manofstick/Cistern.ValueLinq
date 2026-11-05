namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

// The first _count elements of the source dropped. A Skip over this node adds into it, and a Take over it stacks on
// it, so the pull here carries the one count and its MoveNext the one test.
public static partial class Skip
{
    [Operator(Forwards = true)]
    public partial struct Node<T, NodeT>
        : INode<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationElementAt<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        // a long, as skips add into it and an unbounded source is longer than int.MaxValue
        [Propagate] internal long _count;

        public Node(in NodeT nodeT, long count)
        {
            _nodeT = nodeT;
            _count = Math.Max(0, count);
        }

        // this skip, then skip more of what it yields; held at long.MaxValue past it
        internal readonly Node<T, NodeT> Then(int count)
        {
            Optimizations.SkipTake.Compose(_count, long.MaxValue, count, long.MaxValue, out var total, out _);

            return new(in _nodeT, total);
        }

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            CountInformationAfter(ref info, _count);
        }

        // a source known to be no longer than the skip yields nothing, so it is never enumerated (an OrderBy would
        // otherwise be sorted in full); a source with side effects still has to run them
        internal bool SkipsEverything()
        {
            if (_count <= 0)
                return false;

            _nodeT.GetCountInformation(out var info);

            return !info.PotentialSideEffects && !info.IsStale && info.MaximumLength <= _count;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);

            return HandleSkipTake(request.Skip, request.Take, ref response);
        }

        // any other request is put to the source re-sliced, where it can be
        private bool Forward<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            where TRequest : notnull
        {
            if (!Optimizations.Materialise.Is<TRequest>()
                && Optimizations.SkipTake.Try<T, NodeT>(ref _nodeT, _count, long.MaxValue, out var node))
            {
                return node.TryPushOptimization<TRequest, TRequestResult>(in request, out result);
            }

            result = default;
            return false;
        }

        // index i is the source's i + _count; a from-end index is made one from the start by this sequence's length,
        // so without a known length, or beyond the source's int indexes, it is left to the container path
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var index = request.Index;
            long offset = index.Value;
            if (index.IsFromEnd)
            {
                GetCountInformation(out var info);
                if (info.IsStale || info.ActualSize is not int size)
                {
                    response = default;
                    return false;
                }
                offset = size - index.Value;
            }

            if (offset < 0)
            {
                response = (false, default!);
                return true;
            }

            if (_count > int.MaxValue - offset)
            {
                response = default;
                return false;
            }

            return _nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(new Index((int)(offset + _count))), out response);
        }

        private bool HandleSkipTake(long skip, long take, ref NodeContainer<T> container)
        {
            Optimizations.SkipTake.Compose(_count, long.MaxValue, skip, take, out var composedSkip, out var composedTake);

            return Optimizations.SkipTake.Answer<T, NodeT>(ref _nodeT, composedSkip, composedTake, ref container);
        }

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            if (_count <= 0)
                return _nodeT.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);

            if (SkipsEverything())
                return CisternEmpty.Node<T>.Empty.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);

            if (SystemArray.TryPushSlice<T, NodeT, TPushResult, TPushEnumerator, TPushFiller>(in _nodeT, _count, long.MaxValue, in filler, out var sliced))
                return sliced;

            if (Optimizations.SkipTake.Try<T, NodeT>(ref _nodeT, _count, long.MaxValue, out var node))
                return node.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);

            // an int count where the skip fits, as the JIT lays the batch loop out worse around a long's compare
            if (_count <= int.MaxValue)
                return _nodeT.CreateViaPush<TPushResult, CountPush<T, TPushEnumerator, int>, CountFiller<T, TPushEnumerator, int, TPushFiller>>(in options, new CountFiller<T, TPushEnumerator, int, TPushFiller>((int)_count, in filler));

            return _nodeT.CreateViaPush<TPushResult, CountPush<T, TPushEnumerator, long>, CountFiller<T, TPushEnumerator, long, TPushFiller>>(in options, new CountFiller<T, TPushEnumerator, long, TPushFiller>(_count, in filler));
        }
    }

    [Forwarder]
    partial struct CountPush<T, Next, TCount>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<T>
        where TCount : struct, IBinaryInteger<TCount>
    {
        TCount _count;

        // writes this forwarder's own fields into the chain being filled, and hands back the slot for its downstream
        [UnscopedRef]
        internal ref Next Fill(TCount count)
        {
            _count = count;
            return ref _next;
        }

        public bool ProcessNext(T input)
        {
            if (_count <= TCount.Zero)
                return _next.ProcessNext(input);

            --_count;
            return true;
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        private const int SourceNotCreated = -1;

        static Pull<T, NodeT, TInEnumerator> INodeEnumerator<Node<T, NodeT>, Pull<T, NodeT, TInEnumerator>>.Create(in Node<T, NodeT> node)
            => node.SkipsEverything()
                ? new(default, SourceNotCreated)
                : new(TInEnumerator.Create(in node._nodeT), node._count);

        static bool IPullEnumerator<T>.RebuildChanges => TInEnumerator.RebuildChanges || typeof(TInEnumerator).IsAssignableTo(typeof(Optimizations.IPullOptimizationMergeSkipTake<T>));

        static bool IPullEnumerator<T>.PreferRebuilt => TInEnumerator.PreferRebuilt || typeof(TInEnumerator).IsAssignableTo(typeof(Optimizations.IPullOptimizationMergeSkipTake<T>));

        static TBuilt IPullEnumerator<T>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
        {
            ref readonly var own = ref ChainBuilder.As<TOwnNode, Node<T, NodeT>>(in node);
            var above = new ChainBuilder<T, TBuilt, TBuilder>(own._count, in builder);
            return TInEnumerator.Rebuild<NodeT, TBuilt, ChainBuilder<T, TBuilt, TBuilder>>(in own._nodeT, in above);
        }

        public void Dispose()
        {
            if (_count != SourceNotCreated)
                _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            if (_count != 0)
                return SkipThenMoveNext();

            return _enumerator.MoveNext();
        }

        private bool SkipThenMoveNext()
        {
            if (_count == SourceNotCreated)
                return false;

            while (_count > 0)
            {
                if (!_enumerator.MoveNext())
                    return false;

                // a skipped element is still produced, so a selector below runs (and throws) for it as in push mode
                _ = _enumerator.Current;
                --_count;
            }

            return _enumerator.MoveNext();
        }

        public T Current => _enumerator.Current;
    }

    // rebuilds the skip over the node that comes back up, merged into it where that node is a buffering node's window
    internal readonly ref struct ChainBuilder<T, TBuilt, TBuilder>
        : IChainBuilder<T, TBuilt>
        where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
    {
        private readonly long _count;
        private readonly ref readonly ErasedRef _builder;

        public ChainBuilder(long count, in TBuilder builder)
        {
            _count = count;
            _builder = ref Erase.Ref(in builder);
        }

        TBuilt IChainBuilder<T, TBuilt>.Build<TNode, TEnumerator>(in TNode node)
        {
            ref readonly var builder = ref Erase.Deref<TBuilder>(in _builder);
            if (TEnumerator.TryPullOptimization<Optimizations.MergeSkipTake, TNode, TBuilt, TBuilder>(new(_count, long.MaxValue), in node, in builder, out var merged))
                return merged;

            return builder.Build<Node<T, TNode>, Pull<T, TNode, TEnumerator>>(new Node<T, TNode>(in node, _count));
        }
    }

    // a source's count information once count of its elements are skipped
    internal static void CountInformationAfter(ref CountInformation info, long count)
    {
        if (info.MaximumLength.HasValue)
            info.MaximumLength = Math.Max(0, info.MaximumLength.Value - count);
    }
}
