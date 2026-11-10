namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Utils;
using Cistern.ValueLinq.Containers;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

public static partial class Reverse
{
    // the source is buffered and reversed when the enumerator is created, and when it is pushed through unless the
    // source answers the Reverse request itself
    [Materialized(Forwards = true)]
    public partial struct Node<T, NodeT>
        : INode<T>
        , Optimizations.IOptimizationReverse<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationToArray<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationAny
        , Optimizations.IOptimizationContains<T>
        , IBuffered<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;

        // held flat rather than as the nullables the constructor takes: the JIT promotes each of these, so the node is
        // written in place where it is built, where a nullable tuple field is built in a temp and copied whole
        int _maxCountForStackBasedPath;
        ArrayPool<T>? _arrayPool;
        bool _cleanBuffers;

        public Node(in NodeT nodeT, int? maybeMaxCountForStackBasedPath, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo)
        {
            _nodeT = nodeT;
            _maxCountForStackBasedPath = maybeMaxCountForStackBasedPath ?? 64;
            _arrayPool = arrayPoolInfo?.arrayPool;
            _cleanBuffers = arrayPoolInfo?.cleanBuffers ?? false;
        }

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
        }

        // pulled, the reversal lives only as long as the enumeration, so it is buffered in a rented array its enumerator
        // returns
        internal PooledBuffer.Node<T> Materialize(in ChainOptions options) => new(GetReversedPooled());

        private PooledArray<T> GetReversedPooled()
        {
            var reversed = NodeImpl.ToPooledArrayByRef<T, NodeT>(ref _nodeT, _arrayPool, _cleanBuffers);
            reversed.Span.Reverse();
            return reversed;
        }

        internal T[] GetReversedArray()
        {
            var array = NodeImpl.ToArrayByRef(ref _nodeT, _maxCountForStackBasedPath, _arrayPool, _cleanBuffers);
            Array.Reverse(array);
            return array;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(_nodeT);
            return true;
        }

        private bool Count(in Optimizations.Count request, out int response) =>
            BufferedWindow.Count<T, NodeT>(in _nodeT, in request, out response);

        // index i of the reversal is ^(i + 1) of the source and ^k is k - 1, so the source answers the mirrored index
        // without anything being reversed; a source that declines an index from the start or its last is run to that
        // element, as System.Linq's Reverse runs it for First and Last; the one index that cannot be mirrored is put to
        // the reversed source
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var index = request.Index;
            if (index.IsFromEnd && index.Value == 0)
            {
                response = (false, default!);
                return true;
            }
            if (index.IsFromEnd || index.Value < int.MaxValue)
            {
                var mirrored = index.IsFromEnd ? new Index(index.Value - 1) : new Index(index.Value + 1, fromEnd: true);
                if (_nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(mirrored), out response))
                    return true;

                if (mirrored.IsFromEnd && mirrored.Value != 1)
                    return false;

                response = _nodeT.CreateViaPush<(bool, T), Aggregation.FoundElementAt<T>, Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>>(default(ChainOptions), new Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>(new Aggregation.FoundElementAt<T>.Args(mirrored)));
                return true;
            }

            return Forward(in request, out response);
        }

        private bool Any(in Optimizations.Any request, out bool response) =>
            BufferedWindow.Any<T, NodeT>(in _nodeT, out response);

        private bool Contains(in Optimizations.Contains<T> request, out bool response) =>
            TryContains(0, long.MaxValue, request.Value, out response);

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            if (Forward(in request, out response))
                return true;

            response = GetReversedArray();
            return true;
        }

        // the dry run is the source's; the source is copied into the destination and reversed there, as GetReversedArray
        // reverses its own array
        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            if (Forward(in request, out response))
                return true;

            if (request.IsDryRun)
                return Optimizations.CopyTo.TryGetCount<T, NodeT>(ref _nodeT, out response);

            response = Optimizations.CopyTo.Copy<T, NodeT>(ref _nodeT, in request);
            request.Span.Slice(0, response).Reverse();
            return true;
        }

        ReadOnlyMemory<T> IBuffered<T>.GetWindow(long skip, long take) => BufferedWindow.Slice(GetReversedArray(), skip, take);

        bool IBuffered<T>.TryContains(long skip, long take, T value, out bool found) => TryContains(skip, take, value, out found);

        // order is nothing to Contains, so the whole reversal is the source, and a window of it the source's mirrored
        // slice where the source's length is known and it slices itself; nothing is buffered
        private bool TryContains(long skip, long take, T value, out bool found)
        {
            if (BufferedWindow.CoversAll<T, NodeT>(in _nodeT, skip, take))
            {
                found = NodeImpl.Contains<T, NodeT>(_nodeT, value);
                return true;
            }

            if (NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out var count))
            {
                var end = count - skip;
                if (end <= 0)
                {
                    found = false;
                    return true;
                }

                var start = Math.Max(0, end - take);
                var source = _nodeT;
                if (Optimizations.SkipTake.Try<T, NodeT>(ref source, start, end - start, out var slice))
                {
                    found = NodeImpl.Contains<T, NodeContainerNode<T>>(new NodeContainerNode<T>(in slice), value);
                    return true;
                }
            }

            found = false;
            return false;
        }

        // a window over the reversal, reversed only when the window is run
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            if (Forward(in request, out response))
                return true;

            Unsafe.SkipInit(out response);
            response.SetNode(new BufferedWindow.Node<T, Node<T, NodeT>>(in this, request.Skip, request.Take));
            return true;
        }

        // a request is put to the reversed source where the source reverses itself, before this node answers it
        private bool Forward<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            where TRequest : notnull
        {
            if (!Optimizations.Materialise.Is<TRequest>()
                && Optimizations.Reverse.Try<T, NodeT>(ref _nodeT, out var node))
            {
                if (node.TryPushOptimization<TRequest, TRequestResult>(in request, out result))
                    return true; // we carry on with false, because can still do some other optimizations (TODO: Is this true?)
            }

            result = default;
            return false;
        }

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            if (Optimizations.Reverse.Try<T, NodeT>(ref _nodeT, out var node))
                return node.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);

            // the reversal lives only as long as the push, so it is buffered in a rented array
            return SystemArray.ExecutePush<T, TPushResult, TPushEnumerator>(GetReversedPooled(), PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
        }
    }

    public partial struct Pull<T, NodeT>
        : Optimizations.IPullOptimizationMergeSkipTake<T>
    {
        private PooledBuffer.Pull<T> _enumerator;

        private static bool MergeSkipTake<TOwnNode, TBuilt, TBuilder>(in Optimizations.MergeSkipTake request, in TOwnNode node, in TBuilder builder, [MaybeNullWhen(false)] out TBuilt built)
            where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
        {
            built = BufferedWindow.Window<T, Node<T, NodeT>, TOwnNode, TBuilt, TBuilder>(in node, request.Skip, request.Take, in builder);
            return true;
        }
    }
}
