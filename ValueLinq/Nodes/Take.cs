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

        public (bool, int)? InitialSize =>
            _enumerator.InitialSize switch
            {
                (var flag, var length) => (flag, Min(_count, length)),
                _ => null
            };

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
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private int _count;

        public TakeNode(in NodeT nodeT, int count) => (_nodeT, _count) = (nodeT, count);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            if (_count <= 0)
            {
                var empty = new Containers.EmptyFastEnumerator<T>();
                return tail.CreateObject<CreationType, T, Containers.EmptyFastEnumerator<T>>(ref empty);
            }

            var nextEnumerator = new TakeNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, _count);
            return tail.CreateObject<CreationType, EnumeratorElement, TakeNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TIn, TResult, TakeFoward<TIn, FEnumerator>>(new TakeFoward<TIn, FEnumerator>(fenum, _count));
    }

    struct TakeFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        int _count;

        public TakeFoward(in Next prior, int count) => (_next, _count) = (prior, count);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public void Init(int? size) => _next.Init(null);

        public bool ProcessNext(T input)
        {
            if (_count <= 0)
                return false;
            --_count;
            return _next.ProcessNext(input);
        }
    }
}
