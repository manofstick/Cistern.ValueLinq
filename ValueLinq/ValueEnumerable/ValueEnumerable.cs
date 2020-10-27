using System.Collections.Generic;

namespace Cistern.ValueLinq.ValueEnumerable
{
    public readonly struct ValueEnumerable<T, TNode>
        : IEnumerable<T>
        , INode
        where TNode : INode
    {
        public readonly TNode Node;
        public ValueEnumerable(in TNode node) => Node = node;

        public ValueEnumerator<T> GetEnumerator() => Nodes<T>.CreateValueEnumerator(in Node);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Nodes<T>.CreateEnumerator(in Node);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) => Node.CheckForOptimization<TOuter, TRequest, TResult>(in request, out result);
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator) => Node.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref tail, ref enumerator);
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Node.CreateObjectDescent<CreationType, Head, Tail>(ref nodes);
        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) => throw new System.NotImplementedException();
    }
}
