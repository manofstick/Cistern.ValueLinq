﻿using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public struct CountInformation
    {
        public CountInformation(long? size, bool isImmutable)
        {
            MaximumLength = size;
            ActualLengthIsMaximumLength = true;
            IsImmutable = isImmutable;
            IsStale = PotentialSideEffects = false;
        }

        public long? MaximumLength;
        public bool ActualLengthIsMaximumLength;
        public bool IsImmutable;
        public bool IsStale;
        public bool PotentialSideEffects;
    }

    public interface INode
    {
        CreationType CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes;

        CreationType CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
            where Tail : INodes;

        TResult CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>;

        bool CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result);

        void GetCountInformation(out CountInformation info);
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
