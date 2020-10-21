using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public interface INode
    {
        CreationType CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes;

        CreationType CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
            where Tail : INodes;

        T CheckForOptimization<T>() where T : class;
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
        {
            var nodes = new Nodes<Aggregator, NodesEnd>();
            return inner.CreateObjectDescent<T, Aggregator, NodesEnd>(ref nodes);
        }
    }
}
