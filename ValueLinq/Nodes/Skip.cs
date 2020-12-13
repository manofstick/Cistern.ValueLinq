using Cistern.ValueLinq.Containers;
using System;

namespace Cistern.ValueLinq.Nodes
{
    struct SkipNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private int _count;

        public SkipNodeEnumerator(in TInEnumerator enumerator, int count) => (_enumerator, _count) = (enumerator, count);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            while(_count > 0)
            {
                if (!_enumerator.TryGetNext(out current))
                    goto dispose;
                --_count;
            }
            if (_enumerator.TryGetNext(out current))
                return true;

            dispose:
            _enumerator.Dispose();
            return false;
        }
    }

    public struct SkipNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private int _count;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            if (info.MaximumLength.HasValue)
            {
                info.MaximumLength = Math.Max(0, info.MaximumLength.Value - _count);
            }
        }

        public SkipNode(in NodeT nodeT, int count) => (_nodeT, _count) = (nodeT, Math.Max(0, count));

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            _count <= 0
                ? _nodeT.CreateViaPullDescend<CreationType, Head, Tail>(ref nodes)
                : Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SkipNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, _count);
            return tail.CreateObject<CreationType, EnumeratorElement, SkipNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail nodes, out CreationType creation)
        {
            if (typeof(TRequest) == typeof(Optimizations.SourceArray<T>))
            {
                var src = (Optimizations.SourceArray<T>)(object)request;
                return SkipNode.CreateArray(in src, _count, ref nodes, out creation);
            }

            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                NodeContainer<T> container = default;
                if (HandleSkipOptimization(skip.Count, ref container))
                {
                    result = (TResult)(object)container;
                    return true;
                }
            }

            if (Optimizations.Skip.Try<T, NodeT>(ref _nodeT, _count, out var node))
                return node.CheckForOptimization<TRequest, TResult>(in request, out result);

            result = default;
            return false;
        }

        bool HandleSkipOptimization(int count, ref NodeContainer<T> container)
        {
            var total = (long)_count + count;
            if (total <= int.MaxValue)
            {
                if (Optimizations.Skip.Try<T, NodeT>(ref _nodeT, (int)total, out var node))
                {
                    return true;
                }
            }

            _nodeT.GetCountInformation(out var info);
            if (total > info.MaximumLength)
            {
                container.SetEmpty();
                return true;
            }

            if (total > int.MaxValue)
                return false;

            container.SetNode(new SkipNode<T, NodeT>(_nodeT, (int)total));
            return true;
        }

        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
        {
            if (_count <= 0)
                return _nodeT.CreateViaPush<TResult, FEnumerator>(fenum);

            if (Optimizations.Skip.Try<T, NodeT>(ref _nodeT, _count, out var node))
                return node.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in fenum);

            return _nodeT.CreateViaPush<TResult, SkipFoward<T, FEnumerator>>(new SkipFoward<T, FEnumerator>(fenum, _count));
        }
    }

    static class SkipNode
    {
        public static bool CreateArray<T, CreationType, Tail>(in Optimizations.SourceArray<T> src, int _count, ref Tail nodes, out CreationType creation) 
            where Tail : INodes
        {
            var start = src.Start + _count;
            var count = src.Count - _count;
            return SkipTake.CreateArray<T, CreationType, Tail>(src, ref nodes, out creation, start, count);
        }
    }

    struct SkipFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        int _count;

        public SkipFoward(in Next prior, int count) => (_next, _count) = (prior, count);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_count <= 0)
                return _next.ProcessNext(input);
            --_count;
            return true;
        }
    }
}
