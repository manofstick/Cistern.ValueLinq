using System.Collections.Generic;

namespace Cistern.ValueLinq.ValueEnumerable
{
    public interface IValueEnumerable<T>
        : IEnumerable<T>
    {
        new ValueEnumerator<T> GetEnumerator();
    }

    public struct ValueEnumerable<T, TNode>
        : IValueEnumerable<T>
        , INode<T>
        where TNode : INode<T>
    {
        internal TNode Node;

        public ValueEnumerable(in TNode node) => Node = node;

        public ValueEnumerator<T> GetEnumerator() => Nodes<T>.CreateValueEnumerator(in Node);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Nodes<T>.CreateEnumerator(in Node);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        void INode.GetCountInformation(out CountInformation info) => Node.GetCountInformation(out info);
        bool INode.TryObjectAscentOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result) => Node.CheckForOptimization<TRequest, TResult>(in request, out result);
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator) => Node.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref tail, ref enumerator);
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Node.CreateObjectDescent<CreationType, Head, Tail>(ref nodes);
        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) => Node.CreateObjectViaFastEnumerator<TResult, FEnumerator>(fenum);
    }
}
