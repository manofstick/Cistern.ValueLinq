using Cistern.ValueLinq.Containers;
using System;
using System.Buffers;

namespace Cistern.ValueLinq.Nodes
{
    public struct OrderByNode<T, NodeT>
        : IOrderedNode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;

        int? _maybeMaxCountForStackBasedPath;
        (ArrayPool<T> arrayPool, bool cleanBuffers)? _arrayPoolInfo;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
        }

        public OrderByNode(in NodeT nodeT, int? maybeMaxCountForStackBasedPath, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo) =>
            (_nodeT, _maybeMaxCountForStackBasedPath, _arrayPoolInfo) = (nodeT, maybeMaxCountForStackBasedPath, arrayPoolInfo);

        private readonly T[] GetOrderedArray()
        {
            var array = NodeImpl.ToArray(in _nodeT, _maybeMaxCountForStackBasedPath, _arrayPoolInfo);
            
            // sort

            return array;
        }

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var reversed = GetOrderedArray();
            return ArrayNode.Create<T, Nodes<Head, Tail>, CreationType>(reversed, ref nodes);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        readonly bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
        {
            var ordered = new ArrayNode<T>(GetOrderedArray());
            return ordered.CreateViaPush<TResult, FEnumerator>(fenum);
        }

        public TResult CreateOrderedEnumerableViaPush<TKey, TResult, FEnumerator>(in FEnumerator fenum, Func<T, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer, bool descending) where FEnumerator : IForwardEnumerator<T>
        {
            throw new NotImplementedException();
        }

        public CreationType CreateOrderedEnumerableViaPull<TKey, CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes, Func<T, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer, bool descending)
            where Head : INode
            where Tail : INodes
        {
            throw new NotImplementedException();
        }
    }
}
