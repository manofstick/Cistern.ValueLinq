using Cistern.ValueLinq.Containers;
using System;
using static System.Math;

namespace Cistern.ValueLinq.Nodes
{
    struct TakeNodeEnumerator<TIn, TInEnumerator>
        : IPullEnumerator<TIn>
        where TInEnumerator : IPullEnumerator<TIn>
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

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
        {
            if (_count <= 0)
            {
                var empty = new Containers.EmptyPullEnumerator<T>();
                return nodes.CreateObject<CreationType, T, Containers.EmptyPullEnumerator<T>>(ref empty);
            }

            return Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new TakeNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, _count);
            return tail.CreateObject<CreationType, EnumeratorElement, TakeNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail nodes, out CreationType creation)
        {
            if (typeof(TRequest) == typeof(Optimizations.SourceArray<T>))
            {
                var src = (Optimizations.SourceArray<T>)(object)request;
                return TakeNode.CreateArray(in src, _count, ref nodes, out creation);
            }

            creation = default;
            return false;
        }


        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var take = (Optimizations.Take)(object)request;
                var container = new NodeContainer<T>();
                HandleTakeOptimization(take.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (Optimizations.Take.Try<T, NodeT>(ref _nodeT, _count, out var node))
                return node.TryPushOptimization<TRequest, TResult>(in request, out result);

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
            else if (!Optimizations.Take.Try<T, NodeT>(ref _nodeT, _count, out container))
            {
                container.SetNode(new TakeNode<T, NodeT>(_nodeT, total));
            }
        }

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
        {
            if (_count <= 0)
                return EmptyNode<T>.Empty.CreateViaPush<TResult, TPushEnumerator>(fenum);

            if (Optimizations.Take.Try<T, NodeT>(ref _nodeT, _count, out var node))
                return node.CreateViaPush<TResult, TPushEnumerator>(in fenum);

            return _nodeT.CreateViaPush<TResult, TakeFoward<T, TPushEnumerator>>(new TakeFoward<T, TPushEnumerator>(fenum, _count));
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
        : IPushEnumerator<T>
        where Next : IPushEnumerator<T>
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
