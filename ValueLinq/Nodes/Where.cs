using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct WhereNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, bool> _filter;

        public WhereNodeEnumerator(in TInEnumerator enumerator, Func<TIn, bool> filter) => (_enumerator, _filter) = (enumerator, filter);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TIn current)
        {
            while(_enumerator.TryGetNext(out current))
            {
                if (_filter(current))
                    return true;
            }
            return false;
        }
    }

    public struct WhereNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private Func<T, bool> _filter;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public WhereNode(in NodeT nodeT, Func<T, bool> predicate) => (_nodeT, _filter) = (nodeT, predicate);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new WhereNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, bool>)(object)_filter);
            return tail.CreateObject<CreationType, EnumeratorElement, WhereNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TResult, WhereFoward<T, FEnumerator>>(new WhereFoward<T, FEnumerator>(fenum, _filter));
    }

    struct WhereFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        Func<T, bool> _predicate;

        public WhereFoward(in Next prior, Func<T, bool> predicate) => (_next, _predicate) = (prior, predicate);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_predicate(input))
                return _next.ProcessNext(input);
            return true;
        }
    }
}
