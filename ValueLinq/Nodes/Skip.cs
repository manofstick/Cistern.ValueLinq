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

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            _count <= 0
                ? _nodeT.CreateObjectDescent<CreationType, Head, Tail>(ref nodes)
                : Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SkipNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, _count);
            return tail.CreateObject<CreationType, EnumeratorElement, SkipNodeEnumerator<EnumeratorElement, Enumerator>>(0, ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                var maybeSkip = HandleSkipOptimization(skip.Count);
                if (maybeSkip != null)
                {
                    result = (TResult)(object)maybeSkip;
                    return true;
                }
            }

            if (_nodeT.CheckForOptimization<Optimizations.Skip, INode<T>>(new Optimizations.Skip { Count = _count }, out var node))
                return node.CheckForOptimization<TRequest, TResult>(in request, out result);

            result = default;
            return false;
        }

        INode<T> HandleSkipOptimization(int count)
        {
            INode<T> node;

            var total = (long)_count + count;
            if (total <= int.MaxValue)
            {
                if (_nodeT.CheckForOptimization<Optimizations.Skip, INode<T>>(new Optimizations.Skip { Count = (int)total }, out node))
                    return node;
            }

            _nodeT.GetCountInformation(out var info);
            if (total > info.MaximumLength)
                return EmptyNode<T>.Empty;

            if (total > int.MaxValue)
                return null;

            return new SkipNode<T, NodeT>(_nodeT, (int)total);
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
        {
            if (_count <= 0)
                return _nodeT.CreateObjectViaFastEnumerator<TResult, FEnumerator>(fenum);

            if (_nodeT.CheckForOptimization<Optimizations.Skip, INode<T>>(new Optimizations.Skip { Count = _count}, out var node))
                return node.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in fenum);

            return _nodeT.CreateObjectViaFastEnumerator<TResult, SkipFoward<T, FEnumerator>>(new SkipFoward<T, FEnumerator>(fenum, _count));
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
