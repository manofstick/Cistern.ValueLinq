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

        public SkipNode(in NodeT nodeT, int count) => (_nodeT, _count) = (nodeT, count);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            _count <= 0
                ? _nodeT.CreateObjectDescent<CreationType, Head, Tail>(ref nodes)
                : Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SkipNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, _count);
            return tail.CreateObject<CreationType, EnumeratorElement, SkipNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            _count <= 0
                ? _nodeT.CreateObjectViaFastEnumerator<TResult, FEnumerator>(fenum)
                : _nodeT.CreateObjectViaFastEnumerator<TResult, SkipFoward<T, FEnumerator>>(new SkipFoward<T, FEnumerator>(fenum, _count));
    }

    struct SkipFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        int _count;

        public SkipFoward(in Next prior, int count) => (_next, _count) = (prior, count);

        public bool CheckForOptimization<TObject, TRequest, TResult>(TObject obj, in TRequest request, out TResult result) { result = default; return false; }
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
