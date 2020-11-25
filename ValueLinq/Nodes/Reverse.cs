using Cistern.ValueLinq.Containers;
using System;
using System.Buffers;

namespace Cistern.ValueLinq.Nodes
{
    public struct ReverseNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;

        int? _maybeMaxCountForStackBasedPath;
        (ArrayPool<T> arrayPool, bool cleanBuffers)? _arrayPoolInfo;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
        }

        public ReverseNode(in NodeT nodeT, int? maybeMaxCountForStackBasedPath, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo) =>
            (_nodeT, _maybeMaxCountForStackBasedPath, _arrayPoolInfo) = (nodeT, maybeMaxCountForStackBasedPath, arrayPoolInfo);

        private readonly T[] GetReversedArray()
        {
            var array = NodeImpl.ToArray(in _nodeT, _maybeMaxCountForStackBasedPath, _arrayPoolInfo);
            Array.Reverse(array);
            return array;
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var reversed = GetReversedArray();
            return ArrayNode.Create<T, Nodes<Head, Tail>, CreationType>(reversed, ref nodes);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException();

        readonly bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object) GetReversedArray();
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                result = (TResult)(object)(INode<T>)_nodeT;
                return true;
            }

            if (_nodeT.CheckForOptimization<Optimizations.Reverse, INode<T>>(default, out var node))
                return node.CheckForOptimization<TRequest, TResult>(in request, out result);

            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
        {
            if (_nodeT.CheckForOptimization<Optimizations.Reverse, INode<T>>(new Optimizations.Reverse(), out var node))
            {
                return node.CreateObjectViaFastEnumerator<TResult, FEnumerator>(fenum);
            }

            var reversed = new ArrayNode<T>(GetReversedArray());
            return ((INode<T>)reversed).CreateObjectViaFastEnumerator<TResult, FEnumerator>(fenum);
        }
    }
}
