﻿using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.ValueLinq
{
    public struct CountInformation
    {
        enum Flags : byte
        {
            HasMaximumLength                = 0b_0000001,
            ActualLengthIsMaximumLength     = 0b_0000010,
            LengthIsImmutable               = 0b_0000100,
            IsStale                         = 0b_0001000,
            PotentialSideEffects            = 0b_0010000,
            CountingDepth                   = 0b_0100000,
            O1Reversal                      = 0b_1000000,
        }

        public CountInformation(long? size, bool isImmutable)
        {
            _flags = Flags.CountingDepth;
            
            _maybeMaximumLength = size.GetValueOrDefault();
            if (size.HasValue)
                _flags |= Flags.HasMaximumLength | Flags.ActualLengthIsMaximumLength;
            
            if (isImmutable)
                _flags |= Flags.LengthIsImmutable;

            _depth = 0;
        }

        long _maybeMaximumLength;
        Flags _flags;
        byte _depth;

        public readonly int? ActualSize => ActualLengthIsMaximumLength ? (int?)MaximumLength : null;

        /// <summary>
        /// The maximum length of the sequence. It is null if the maximum length is unknown.
        /// </summary>
        public long? MaximumLength
        {
            readonly get => _flags.HasFlag(Flags.HasMaximumLength) ? (long?)_maybeMaximumLength : null;
            set {
                _maybeMaximumLength = value.GetValueOrDefault();
                if (value.HasValue) _flags |= Flags.HasMaximumLength; else _flags &= ~Flags.HasMaximumLength;
            }
        }

        public int? Depth
        {
            readonly get => _flags.HasFlag(Flags.CountingDepth) ? (int?)_depth : null;
            set
            {
                if (value >= 0 || value <= byte.MaxValue)
                {
                    _flags |= Flags.CountingDepth;
                    _depth = (byte)value;
                }
                else
                {
                    _flags &= ~Flags.CountingDepth;
                    _depth = byte.MaxValue;
                }
            }
        }

        /// <summary>
        /// MaximumLength contains the true length of the sequence when this is true. It is false if the
        /// stream passed through a filtering function such as Where.
        /// </summary>
        public bool ActualLengthIsMaximumLength
        {
            readonly get => _flags.HasFlag(Flags.ActualLengthIsMaximumLength);
            set { if (value) _flags |= Flags.ActualLengthIsMaximumLength; else _flags &= ~Flags.ActualLengthIsMaximumLength; }
        }

        /// <summary>
        /// If the length of the underlying containers length is immutable, such as an array, or rather than an 
        /// enumerable that could change size such as a List
        /// </summary>
        public bool LengthIsImmutable
        {
            readonly get => _flags.HasFlag(Flags.LengthIsImmutable);
            set { if (value) _flags |= Flags.LengthIsImmutable; else _flags &= ~Flags.LengthIsImmutable; }
        }

        /// <summary>
        /// When the data has been cached on a node, such as Concat. Because potentially the underlying structure
        /// might have changed, such as with a List
        /// </summary>
        public bool IsStale
        {
            readonly get => _flags.HasFlag(Flags.IsStale);
            set { if (value) _flags |= Flags.IsStale; else _flags &= ~Flags.IsStale; }
        }

        /// <summary>
        /// Lamdbda's passed to Select are considered as having side effects so in a usual Count() instruction they
        /// are enumerated, athough System.Linq's implemetation doesn't handle this consistently (i.e. if a Select
        /// is then contained within a Concat as an example)
        /// </summary>
        public bool PotentialSideEffects
        {
            readonly get => _flags.HasFlag(Flags.PotentialSideEffects);
            set { if (value) _flags |= Flags.PotentialSideEffects; else _flags &= ~Flags.PotentialSideEffects; }
        }
    }

    public interface INode
    {
        void GetCountInformation(out CountInformation info);

        CreationType CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            where TNodes : INodes;

        CreationType CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Nodes>(ref Nodes nodes, ref Enumerator enumerator)
            where Enumerator : struct, IPullEnumerator<EnumeratorElement>
            where Nodes : INodes;

        bool TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            where Nodes : INodes;
        bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result);
    }

    public interface INode<T> : INode
    {
        TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum) where TPushEnumerator : IPushEnumerator<T>;
    }

    public interface INodes
    {
        CreationType CreateObject<CreationType, EnumeratorElement, Enumerator>(ref Enumerator enumerator)
            where Enumerator : struct, IPullEnumerator<EnumeratorElement>;

        bool TryObjectAscentOptimization<TRequest, CreationType>(in TRequest request, out CreationType creation) { creation = default; return false; }
    }

    struct NodesEnd
        : INodes
    {
        CreationType INodes.CreateObject<CreationType, EnumeratorElement, Enumerator>(ref Enumerator _) => throw new InvalidOperationException();
    }

    public struct Nodes<Head, Tail>
        : INodes
        where Tail : INodes
        where Head : INode
    {
        private Head _head;
        private Tail _tail;

        public Nodes(in Head head, in Tail tail) => (_head, _tail) = (head, tail);

        public CreationType CreateObject<CreationType, EnumeratorElement, Enumerator>(ref Enumerator enumerator)
            where Enumerator : struct, IPullEnumerator<EnumeratorElement> =>
            _head.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref _tail, ref enumerator);

        public bool TryObjectAscentOptimization<TRequest, CreationType>(in TRequest request, out CreationType creation) =>
            _head.TryPullOptimization<TRequest, CreationType, Tail>(in request, ref _tail, out creation);
    }

    internal static class Helper
    {
        public static bool TryPushOptimization<Node, TRequest, TResult>(in Node node, in TRequest request, out TResult result)
            where Node : INode
            => node.TryPushOptimization(in request, out result);

        public static CreationType CreateViaPullDescend<Node, CreationType, TNodes>(in Node node, ref TNodes nodes)
            where TNodes: INodes
            where Node : INode
            => node.CreateViaPullDescend<CreationType, TNodes>(ref nodes);

        public static TResult CreateViaPush<Node, T, TResult, TPushEnumerator>(in Node node, in TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
            where Node : INode<T>
            => node.CreateViaPush<TResult, TPushEnumerator>(in fenum);
    }

    enum NodeType : byte
    {
        Empty = 0,
        Reference,
        Array,
        Memory,
        ListSegment,
        ReversedMemory,
        ReversedListSegment,
    }

    struct NodeContainer<TElement>
    {
        public NodeType Type;

        private INode<TElement> Node;
        private ArrayNode<TElement> ArrayNode;
        private MemoryNode<TElement> MemoryNode;
        private ListSegmentNode<TElement> ListSegmentNode;
        private ReversedMemoryNode<TElement> ReversedMemoryNode;
        private ReversedListSegmentNode<TElement> ReversedListNode;

        public void SetEmpty()                          => Type                       = NodeType.Empty;
        public void SetNode(INode<TElement> node)              => (Type, Node)               = (NodeType.Reference, node);
        public void SetNode(ArrayNode<TElement> node)          => (Type, ArrayNode)          = (NodeType.Array, node);
        public void SetNode(MemoryNode<TElement> node)         => (Type, MemoryNode)         = (NodeType.Memory, node);
        public void SetNode(ListSegmentNode<TElement> node)    => (Type, ListSegmentNode)    = (NodeType.ListSegment, node);
        public void SetNode(ReversedMemoryNode<TElement> node) => (Type, ReversedMemoryNode) = (NodeType.ReversedMemory, node);
        public void SetNode(ReversedListSegmentNode<TElement> node)   => (Type, ReversedListNode)   = (NodeType.ReversedListSegment, node);

        public bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) =>
            Type switch
            {
                NodeType.Empty          => EmptyNode<TElement>.Empty.TryPushOptimization<TRequest, TResult>(in request, out result),
                NodeType.Reference      => Node.TryPushOptimization<TRequest, TResult>(in request, out result),
                NodeType.Array          => ArrayNode.TryPushOptimization<TRequest, TResult>(in request, out result),
                NodeType.Memory         => MemoryNode.TryPushOptimization<TRequest, TResult>(in request, out result),
                NodeType.ListSegment    => ListSegmentNode.TryPushOptimization<TRequest, TResult>(in request, out result),
                NodeType.ReversedMemory => ReversedMemoryNode.TryPushOptimization<TRequest, TResult>(in request, out result),
                NodeType.ReversedListSegment => ReversedListNode.TryPushOptimization<TRequest, TResult>(in request, out result),
                _ => throw new InvalidOperationException(),
            };

        public TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TElement> =>
            Type switch
            {
                NodeType.Empty          => EmptyNode<TElement>.Empty.CreateViaPush<TResult, TPushEnumerator>(in fenum),
                NodeType.Reference      => Node.CreateViaPush<TResult, TPushEnumerator>(in fenum),
                NodeType.Array          => ArrayNode.CreateViaPush<TResult, TPushEnumerator>(in fenum),
                NodeType.Memory         => MemoryNode.CreateViaPush<TResult, TPushEnumerator>(in fenum),
                NodeType.ListSegment    => ListSegmentNode.CreateViaPush<TResult, TPushEnumerator>(in fenum),
                NodeType.ReversedMemory => ReversedMemoryNode.CreateViaPush<TResult, TPushEnumerator>(in fenum),
                NodeType.ReversedListSegment => ReversedListNode.CreateViaPush<TResult, TPushEnumerator>(in fenum),
                _ => throw new InvalidOperationException(),
            };
    }

    static class Nodes<T>
    {
        public static IEnumerator<T> CreateEnumerator<Node>(in Node node)
            where Node : INode
        {
            var nodes = new Nodes<FastEnumeratorToIEnumeratorNode, NodesEnd>();
            return node.CreateViaPullDescend<IEnumerator<T>, Nodes<FastEnumeratorToIEnumeratorNode, NodesEnd>>(ref nodes);
        }

        public static PullEnumerator<T> CreatePullEnumerator<Node>(in Node node)
            where Node : INode
        {
            var nodes = new Nodes<PullEnumeratorToValueEnumeratorNode, NodesEnd>();
            return node.CreateViaPullDescend<PullEnumerator<T>, Nodes<PullEnumeratorToValueEnumeratorNode, NodesEnd>>(ref nodes);
        }

        public static ValueEnumerator<T> CreateValueEnumerator<Node>(in Node node)
            where Node : INode
            => new ValueEnumerator<T>(CreatePullEnumerator(node));

        public static T Descend<Head, Tail, Next>(ref Next next, in Head head, in Tail tail)
            where Next : INode
            where Head : INode
            where Tail : INodes
        {
            var nodes = new Nodes<Head, Tail>(in head, in tail);
            return next.CreateViaPullDescend<T, Nodes<Head, Tail>>(ref nodes);
        }

        public static T Aggregation<Enumerable, Aggregator>(in Enumerable inner)
            where Enumerable : INode
            where Aggregator : INode
                => Aggregation<Enumerable, Aggregator>(inner, default);

        public static T Aggregation<Enumerable, Aggregator>(in Enumerable inner, in Aggregator aggregator)
            where Enumerable : INode
            where Aggregator : INode
        {
            var nodes = new Nodes<Aggregator, NodesEnd>(aggregator, new NodesEnd());
            return inner.CreateViaPullDescend<T, Nodes<Aggregator, NodesEnd>>(ref nodes);
        }
    }
}
