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

        bool INode.TryObjectAscentOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        readonly bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                NodeContainer<T> container = default;
                container.SetNode(_nodeT);
                result = (TResult)(object)container;
                return true;
            }

            if (_nodeT.CheckForOptimization<Optimizations.Reverse, NodeContainer<T>>(default, out var node))
            {
                if (node.CheckForOptimization<TRequest, TResult>(in request, out result))
                    return true; // we carry on with false, because can still do some other optimizations
            }

            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)GetReversedArray();
                return true;
            }

            // TODO: Maybe some extra thought here; I *think* this is good because we are ultimately creating a root node
            // which *should* mean that we don't have to traverse the optimization pipeline again, but I should actually
            // just ensure this is true. (Why? well here we're spending the time to actually reverse the array which
            // is an O(N) operation (at least - could be doing a sort, etc.))
            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                NodeContainer<T> container = default;
                MemoryNode.Skip(new ReadOnlyMemory<T>(GetReversedArray()), skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var skip = (Optimizations.Take)(object)request;
                NodeContainer<T> container = default;
                MemoryNode.Take(GetReversedArray(), skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
        {
            if (_nodeT.CheckForOptimization<Optimizations.Reverse, NodeContainer<T>>(new Optimizations.Reverse(), out var node))
                return node.CreateObjectViaFastEnumerator<TResult, FEnumerator>(fenum);

            var reversed = new ArrayNode<T>(GetReversedArray());
            return reversed.CreateObjectViaFastEnumerator<TResult, FEnumerator>(fenum);
        }
    }
}
