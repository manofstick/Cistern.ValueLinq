using System.Collections.Generic;

namespace Cistern.ValueLinq.ValueEnumerable
{
    public readonly struct ValueEnumerable<T, TNode>
        : IEnumerable<T>
        where TNode : INode
    {
        public readonly TNode Node;
        public ValueEnumerable(in TNode node) => Node = node;

        public ValueEnumerator<T> GetEnumerator() => Nodes<T>.CreateValueEnumerator(in Node);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Nodes<T>.CreateEnumerator(in Node);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
    }
}
