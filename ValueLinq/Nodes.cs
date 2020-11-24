using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

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

        CreationType CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes;

        CreationType CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Nodes>(ref Nodes nodes, ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
            where Nodes : INodes;

        bool TryObjectAscentOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            where Nodes : INodes
        { creation = default; return false; }

        bool CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        { result = default; return false; }
    }

    public interface INode<T> : INode
    {
        TResult CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<T>;
    }

    public interface INodes
    {
        CreationType CreateObject<CreationType, EnumeratorElement, Enumerator>(int depth, ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>;

        bool TryObjectAscentOptimization<TRequest, CreationType>(int depth, in TRequest request, out CreationType creation) { creation = default; return false; }
    }

    struct NodesEnd
        : INodes
    {
        CreationType INodes.CreateObject<CreationType, EnumeratorElement, Enumerator>(int depth, ref Enumerator _) => throw new InvalidOperationException();
    }

    public struct Nodes<Head, Tail>
        : INodes
        where Tail : INodes
        where Head : INode
    {
        private Head _head;
        private Tail _tail;

        public Nodes(in Head head, in Tail tail) => (_head, _tail) = (head, tail);

        public CreationType CreateObject<CreationType, EnumeratorElement, Enumerator>(int depth, ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            if (depth == 0)
                return _head.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref _tail, ref enumerator);

            return _tail.CreateObject<CreationType, EnumeratorElement, Enumerator>(depth - 1, ref enumerator);
        }

        public bool TryObjectAscentOptimization<TRequest, CreationType>(int depth, in TRequest request, out CreationType creation)
        {
            if (depth == 0)
                return _head.TryObjectAscentOptimization<TRequest, CreationType, Tail>(in request, ref _tail, out creation);

            return _tail.TryObjectAscentOptimization<TRequest, CreationType>(depth - 1, in request, out creation);
        }
    }

    internal static class Helper
    {
        public static bool CheckForOptimization<Node, TRequest, TResult>(in Node node, in TRequest request, out TResult result)
            where Node : INode
            => node.CheckForOptimization(in request, out result);

        public static CreationType CreateObjectDescent<Node, CreationType, Head, Tail>(in Node node, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
            where Node : INode
            => node.CreateObjectDescent<CreationType, Head, Tail>(ref nodes);

        public static TResult CreateObjectViaFastEnumerator<Node, T, TResult, FEnumerator>(in Node node, in FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<T>
            where Node : INode<T>
            => node.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in fenum);
    }

    static class Nodes<T>
    {
        public static IEnumerator<T> CreateEnumerator<Node>(in Node node)
            where Node : INode
        {
            var nodes = new Nodes<FastEnumeratorToEnumeratorNode, NodesEnd>();
            return node.CreateObjectDescent<IEnumerator<T>, FastEnumeratorToEnumeratorNode, NodesEnd>(ref nodes);
        }

        public static ValueEnumerator<T> CreateValueEnumerator<Node>(in Node node)
            where Node : INode
        {
            var nodes = new Nodes<FastEnumeratorToValueEnumeratorNode, NodesEnd>();
            return node.CreateObjectDescent<ValueEnumerator<T>, FastEnumeratorToValueEnumeratorNode, NodesEnd>(ref nodes);
        }

        public static T Descend<Head, Tail, Next>(ref Next next, in Head head, in Tail tail)
            where Next : INode
            where Head : INode
            where Tail : INodes
        {
            var nodes = new Nodes<Head, Tail>(in head, in tail);
            return next.CreateObjectDescent<T, Head, Tail>(ref nodes);
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
            return inner.CreateObjectDescent<T, Aggregator, NodesEnd>(ref nodes);
        }



    }
}
