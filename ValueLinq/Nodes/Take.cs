using Cistern.ValueLinq.Containers;
using System;
using static System.Math;

namespace Cistern.ValueLinq.Nodes
{
    struct TakeNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private int _count;

        public TakeNodeEnumerator(in TInEnumerator enumerator, int count) => (_enumerator, _count) = (enumerator, count);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            if (_count <= 0)
                goto dispose;

            --_count;
            if (_enumerator.TryGetNext(out current))
                return true;

            dispose:
            current = default;
            _enumerator.Dispose();
            return false;
        }
    }

    public struct TakeNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private int _count;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            if (_count <= 0)
            {
                info = new CountInformation(0, true);
            }
            else if (info.MaximumLength.HasValue)
            {
                info.MaximumLength = Math.Min(info.MaximumLength.Value, _count);
            }
        }

        public TakeNode(in NodeT nodeT, int count) => (_nodeT, _count) = (nodeT, count);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            if (_count <= 0)
            {
                var empty = new Containers.EmptyFastEnumerator<T>();
                return nodes.CreateObject<CreationType, T, Containers.EmptyFastEnumerator<T>>(0, ref empty);
            }

            return Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new TakeNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, _count);
            return tail.CreateObject<CreationType, EnumeratorElement, TakeNodeEnumerator<EnumeratorElement, Enumerator>>(0, ref nextEnumerator);
        }

        bool INode.TryObjectAscentOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail nodes, out CreationType creation)
        {
            if (typeof(TRequest) == typeof(Optimizations.SourceArray<T>))
            {
                var src = (Optimizations.SourceArray<T>)(object)request;
                return TakeNode.CreateArray(in src, _count, ref nodes, out creation);
            }

            creation = default;
            return false;
        }


        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var take = (Optimizations.Take)(object)request;
                var container = new NodeContainer<T>();
                HandleTakeOptimization(take.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (_nodeT.CheckForOptimization<Optimizations.Take, NodeContainer<T>>(new Optimizations.Take { Count = _count }, out var node))
            {
                return node.CheckForOptimization<TRequest, TResult>(in request, out result);
            }

            result = default;
            return false;
        }

        void HandleTakeOptimization(int count, ref NodeContainer<T> container)
        {
            var total = Math.Min(_count, count);
            if (total <= 0)
            {
                container.SetEmpty();
            }
            else if (!_nodeT.CheckForOptimization(new Optimizations.Take { Count = total }, out container))
            {
                container.SetNode(new TakeNode<T, NodeT>(_nodeT, total));
            }
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
        {
            if (_count <= 0)
                return EmptyNode<T>.Empty.CreateObjectViaFastEnumerator<TResult, FEnumerator>(fenum);

            if (_nodeT.CheckForOptimization<Optimizations.Take, NodeContainer<T>>(new Optimizations.Take { Count = _count }, out var node))
                return node.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in fenum);

            return _nodeT.CreateObjectViaFastEnumerator<TResult, TakeFoward<T, FEnumerator>>(new TakeFoward<T, FEnumerator>(fenum, _count));
        }
    }

    static class TakeNode
    {
        public static bool CreateArray<T, CreationType, Tail>(in Optimizations.SourceArray<T> src, int _count, ref Tail nodes, out CreationType creation)
            where Tail : INodes
        {
            var start = src.Start;
            var count = Math.Min(src.Count, _count);
            return SkipTake.CreateArray<T, CreationType, Tail>(src, ref nodes, out creation, start, count);
        }
    }

    struct TakeFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        int _count;

        public TakeFoward(in Next prior, int count) => (_next, _count) = (prior, count);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_count <= 0)
                return false;
            --_count;
            return _next.ProcessNext(input);
        }
    }
}
