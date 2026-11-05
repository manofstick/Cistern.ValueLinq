using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.ValueEnumerable
{
    // what ToEnumerable hands out: the chain rebuilt once, at the call, over the containers its IEnumerable<T> sources
    // hold, so neither its enumerators nor its pushes walk it again; an ICollection<T> where CopyTo's dry run answers,
    // which it then asks for its Count; and, fused, an INode but not an IPullEnumerable, so SystemIEnumerable.ExecutePush
    // pushes through its node as though the consumer had asked to fuse
    static class ChainEnumerable
    {
        public static IEnumerable<T> Create<T, TNode, TEnumerator>(in TNode node, EnumerableOptions options)
            where TNode : INode<T>
            where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
        {
            if (TEnumerator.RebuildChanges)
                return TEnumerator.Rebuild<TNode, IEnumerable<T>, Builder<T>>(in node, new Builder<T>(options));

            return Hold<T, TNode, TEnumerator>(in node, options);
        }

        private static IEnumerable<T> Hold<T, TNode, TEnumerator>(in TNode node, EnumerableOptions options)
            where TNode : INode<T>
            where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
        {
            var fused = (options & EnumerableOptions.Fuse) != 0;

            var counted = node;
            if (Optimizations.CopyTo.TryGetCount<T, TNode>(ref counted, out _))
            {
                return fused
                    ? new FusedChainCollection<T, TNode, TEnumerator>(in node)
                    : new ChainCollection<T, TNode, TEnumerator>(in node);
            }

            return fused
                ? new FusedChainEnumerable<T, TNode, TEnumerator>(in node)
                : new ChainEnumerable<T, TNode, TEnumerator>(in node);
        }

        private readonly struct Builder<T>(EnumerableOptions options)
            : IChainBuilder<T, IEnumerable<T>>
        {
            IEnumerable<T> IChainBuilder<T, IEnumerable<T>>.Build<TNode, TEnumerator>(in TNode node)
                => Hold<T, TNode, TEnumerator>(in node, options);
        }

        public static int Count<T, TNode>(ref TNode node)
            where TNode : INode<T>
        {
            if (!Optimizations.CopyTo.TryGetCount<T, TNode>(ref node, out var count))
                throw new InvalidOperationException("Should not be called on a chain whose CopyTo dry run declined");

            return count;
        }

        public static void CopyTo<T, TNode>(ref TNode node, T[] array, int arrayIndex)
            where TNode : INode<T>
        {
            ArgumentNullException.ThrowIfNull(array);
            ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length);
            ArgumentOutOfRangeException.ThrowIfLessThan(array.Length - arrayIndex, Count<T, TNode>(ref node), nameof(arrayIndex));

            Optimizations.CopyTo.Copy<T, TNode>(ref node, new Optimizations.CopyTo<T>(array).Advance(arrayIndex));
        }
    }

    class ChainEnumerable<T, TNode, TEnumerator>
        : IEnumerable<T>
        , IPullEnumerable<T>
        , INode<T>
        where TNode : INode<T>
        where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
    {
        protected TNode _node;

        public ChainEnumerable(in TNode node) => _node = node;

        public IEnumerator<T> GetEnumerator() => PullEnumerator.Create<T, TNode, TEnumerator>(in _node);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        PullEnumerator<T> IPullEnumerable<T>.CreatePullEnumerator()
            => PullEnumerator.Create<T, TNode, TEnumerator>(in _node);
        void IPullEnumerable<T>.Push(in ChainOptions options, ErasedChain.PushSink<T> sink)
            => _node.CreateViaPush<bool, ErasedChain.PushForward<T>, Construct<ErasedChain.PushForward<T>, ErasedChain.PushForward<T>.Args>>(in options, new Construct<ErasedChain.PushForward<T>, ErasedChain.PushForward<T>.Args>(new ErasedChain.PushForward<T>.Args(sink)));

        void INode.GetCountInformation(out CountInformation info) => _node.GetCountInformation(out info);

        bool INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            => _node.TryPushOptimization<TRequest, TRequestResult>(in request, out result);
        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => _node.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    sealed class ChainCollection<T, TNode, TEnumerator>(in TNode node)
        : ChainEnumerable<T, TNode, TEnumerator>(in node)
        , ICollection<T>
        , IReadOnlyCollection<T>
        where TNode : INode<T>
        where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
    {
        public int Count => ChainEnumerable.Count<T, TNode>(ref _node);

        bool ICollection<T>.IsReadOnly => true;

        bool ICollection<T>.Contains(T item) => NodeImpl.Contains<T, TNode>(_node, item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => ChainEnumerable.CopyTo<T, TNode>(ref _node, array, arrayIndex);

        void ICollection<T>.Add(T item) => ThrowHelper.ThrowNotSupportedException();

        void ICollection<T>.Clear() => ThrowHelper.ThrowNotSupportedException();

        bool ICollection<T>.Remove(T item)
        {
            ThrowHelper.ThrowNotSupportedException();

            return false;
        }
    }

    class FusedChainEnumerable<T, TNode, TEnumerator>
        : IEnumerable<T>
        , INode<T>
        where TNode : INode<T>
        where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
    {
        protected TNode _node;

        public FusedChainEnumerable(in TNode node) => _node = node;

        public IEnumerator<T> GetEnumerator() => PullEnumerator.Create<T, TNode, TEnumerator>(in _node);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        void INode.GetCountInformation(out CountInformation info) => _node.GetCountInformation(out info);

        bool INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            => _node.TryPushOptimization<TRequest, TRequestResult>(in request, out result);
        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => _node.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    sealed class FusedChainCollection<T, TNode, TEnumerator>(in TNode node)
        : FusedChainEnumerable<T, TNode, TEnumerator>(in node)
        , ICollection<T>
        , IReadOnlyCollection<T>
        where TNode : INode<T>
        where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
    {
        public int Count => ChainEnumerable.Count<T, TNode>(ref _node);

        bool ICollection<T>.IsReadOnly => true;

        bool ICollection<T>.Contains(T item) => NodeImpl.Contains<T, TNode>(_node, item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => ChainEnumerable.CopyTo<T, TNode>(ref _node, array, arrayIndex);

        void ICollection<T>.Add(T item) => ThrowHelper.ThrowNotSupportedException();

        void ICollection<T>.Clear() => ThrowHelper.ThrowNotSupportedException();

        bool ICollection<T>.Remove(T item)
        {
            ThrowHelper.ThrowNotSupportedException();

            return false;
        }
    }
}
