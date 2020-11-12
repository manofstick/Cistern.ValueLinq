using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public struct CountInformation
    {
        enum Flags : byte
        {
            HasMaximumLength                = 0b_00001,
            ActualLengthIsMaximumLength     = 0b_00010,
            IsImmutable                     = 0b_00100,
            IsStale                         = 0b_01000,
            PotentialSideEffects            = 0b_10000,
        }

        public CountInformation(long? size, bool isImmutable)
        {
            _flags = Flags.ActualLengthIsMaximumLength;
            
            _maybeMaximumLength = size.GetValueOrDefault();
            if (size.HasValue)
                _flags |= Flags.HasMaximumLength;
            
            if (isImmutable)
                _flags |= Flags.IsImmutable;
        }

        long _maybeMaximumLength;
        Flags _flags;

        public int? ActualSize => ActualLengthIsMaximumLength ? (int?)MaximumLength : null;

        /// <summary>
        /// The maximum length of the sequence. It is null if the maximum length is unknown.
        /// </summary>
        public long? MaximumLength
        {
            get => _flags.HasFlag(Flags.HasMaximumLength) ? (long?)_maybeMaximumLength : null;
            set {
                _maybeMaximumLength = value.GetValueOrDefault();
                if (value.HasValue) _flags |= Flags.HasMaximumLength; else _flags &= ~Flags.HasMaximumLength;
            }
        }

        /// <summary>
        /// MaximumLength contains the true length of the sequence when this is true. It is false if the
        /// stream passed through a filtering function such as Where.
        /// </summary>
        public bool ActualLengthIsMaximumLength
        {
            get => _flags.HasFlag(Flags.ActualLengthIsMaximumLength);
            set { if (value) _flags |= Flags.ActualLengthIsMaximumLength; else _flags &= ~Flags.ActualLengthIsMaximumLength; }
        }

        /// <summary>
        /// If the length of the underlying containers length is immutable, such as an array, or rather than an 
        /// enumerable that could change size such as a List
        /// </summary>
        public bool IsImmutable
        {
            get => _flags.HasFlag(Flags.IsImmutable);
            set { if (value) _flags |= Flags.IsImmutable; else _flags &= ~Flags.IsImmutable; }
        }

        /// <summary>
        /// When the data has been cached on a node, such as Concat. Because potentially the underlying structure
        /// might have changed, such as with a List
        /// </summary>
        public bool IsStale
        {
            get => _flags.HasFlag(Flags.IsStale);
            set { if (value) _flags |= Flags.IsStale; else _flags &= ~Flags.IsStale; }
        }

        /// <summary>
        /// Lamdbda's passed to Select are considered as having side effects so in a usual Count() instruction they
        /// are enumerated, athough System.Linq's implemetation doesn't handle this consistently (i.e. if a Select
        /// is then contained within a Concat as an example)
        /// </summary>
        public bool PotentialSideEffects
        {
            get => _flags.HasFlag(Flags.PotentialSideEffects);
            set { if (value) _flags |= Flags.PotentialSideEffects; else _flags &= ~Flags.PotentialSideEffects; }
        }
    }

    public interface INode
    {
        void GetCountInformation(out CountInformation info);

        CreationType CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes;

        CreationType CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
            where Tail : INodes;

        bool CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result);
    }

    public interface INode<T> : INode
    {
        TResult CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<T>;
    }

    public interface INodes
    {
        CreationType CreateObject<CreationType, EnumeratorElement, Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>;
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
            where Enumerator : IFastEnumerator<EnumeratorElement>
                => _head.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref _tail, ref enumerator);
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
