using System;
using static System.Math;

namespace Cistern.ValueLinq.Nodes
{
    struct SkipNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private int _count;

        public SkipNodeEnumerator(in TInEnumerator enumerator, int count) => (_enumerator, _count) = (enumerator, count);

        public (bool, int)? InitialSize =>
            _enumerator.InitialSize switch
            {
                (var flag, var length) => (flag, Max(0, length-_count)),
                _ => null
            };

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
        : INode
        where NodeT : INode
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

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SkipNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, _count);
            return tail.CreateObject<CreationType, EnumeratorElement, SkipNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TIn, TResult, SkipFoward<TIn, FEnumerator>>(new SkipFoward<TIn, FEnumerator>(fenum, _count));
    }

    struct SkipFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        int _count;

        public SkipFoward(in Next prior, int count) => (_next, _count) = (prior, count);

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
