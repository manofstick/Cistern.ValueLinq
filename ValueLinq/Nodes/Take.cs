namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Runtime.CompilerServices;

// At most the first _count elements of the source. A Take over this node narrows it, and a Skip over it stacks on
// it, so the pull here carries the one count and its MoveNext the one test.
public static partial class Take
{
    [Operator(Forwards = true)]
    public partial struct Node<T, NodeT>
        : INode<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationElementAt<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal int _count;

        // this take, then at most count of what it yields
        internal readonly Node<T, NodeT> Then(int count) =>
            new(in _nodeT, Math.Min(_count, count));

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            CountInformationAfter(ref info, _count);
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
                && Optimizations.SkipTake.Try<T, NodeT>(ref _nodeT, 0, _count, out var node))
            {
                return node.TryPushOptimization<TRequest, TRequestResult>(in request, out result);
            }

            result = default;
            return false;
        }

        // index i is the source's i when below the count, and nothing otherwise; a from-end index is made one from the
        // start by this sequence's length, so without a known length it is left to the container path
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var index = request.Index;
            var offset = index.Value;
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

            if (offset < 0 || offset >= _count)
            {
                response = (false, default!);
                return true;
            }

            return _nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(new Index(offset)), out response);
        }

        private bool HandleSkipTake(long skip, long take, ref NodeContainer<T> container)
        {
            Optimizations.SkipTake.Compose(0, _count, skip, take, out var composedSkip, out var composedTake);

            return Optimizations.SkipTake.Answer<T, NodeT>(ref _nodeT, composedSkip, composedTake, ref container);
        }

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            if (_count <= 0)
                return CisternEmpty.Node<T>.Empty.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);

            if (SystemArray.TryPushSlice<T, NodeT, TPushResult, TPushEnumerator, TPushFiller>(in _nodeT, 0, _count, in filler, out var sliced))
                return sliced;

            if (Optimizations.SkipTake.Try<T, NodeT>(ref _nodeT, 0, _count, out var node))
                return node.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);

            return _nodeT.CreateViaPush<TPushResult, Push<T, TPushEnumerator>, Filler<T, TPushEnumerator, TPushFiller>>(in options, new Filler<T, TPushEnumerator, TPushFiller>(_count, in filler));
        }
    }

    partial struct Push<T, Next>
    {
        // halts on the last element taken, so the source produces nothing past it
        public bool ProcessNext(T input)
        {
            if (_count <= 0)
                return false;

            --_count;
            return _next.ProcessNext(input) && _count > 0;
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        private const int SourceNotCreated = -1;

        static Pull<T, NodeT, TInEnumerator> INodeEnumerator<Node<T, NodeT>, Pull<T, NodeT, TInEnumerator>>.Create(in Node<T, NodeT> node)
            => node._count > 0
                ? new(TInEnumerator.Create(in node._nodeT), node._count)
                : new(default, SourceNotCreated);

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
            if (_count <= 0)
                return false;

            --_count;
            return _enumerator.MoveNext();
        }

        public T Current => _enumerator.Current;
    }

    // rebuilds the take over the node that comes back up, merged into it where that node is a buffering node's window
    internal readonly ref struct ChainBuilder<T, TBuilt, TBuilder>
        : IChainBuilder<T, TBuilt>
        where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
    {
        private readonly int _count;
        private readonly ref readonly ErasedRef _builder;

        public ChainBuilder(int count, in TBuilder builder)
        {
            _count = count;
            _builder = ref Erase.Ref(in builder);
        }

        TBuilt IChainBuilder<T, TBuilt>.Build<TNode, TEnumerator>(in TNode node)
        {
            ref readonly var builder = ref Erase.Deref<TBuilder>(in _builder);
            if (TEnumerator.TryPullOptimization<Optimizations.MergeSkipTake, TNode, TBuilt, TBuilder>(new(0, _count), in node, in builder, out var merged))
                return merged;

            return builder.Build<Node<T, TNode>, Pull<T, TNode, TEnumerator>>(new Node<T, TNode> { _nodeT = node, _count = _count });
        }
    }

    // a source's count information once at most count of its elements are taken
    internal static void CountInformationAfter(ref CountInformation info, int count)
    {
        if (count <= 0)
        {
            info = new CountInformation(0, true);
        }
        else
        {
            // bounded by the count even when the source's length is unknown; the actual length stays unknown
            info.MaximumLength = Math.Min(info.MaximumLength ?? count, count);
        }
    }
}
