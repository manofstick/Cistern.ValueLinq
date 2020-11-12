using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct Where_InNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private InFunc<TIn, bool> _filter;

        public Where_InNodeEnumerator(in TInEnumerator enumerator, InFunc<TIn, bool> filter) => (_enumerator, _filter) = (enumerator, filter);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TIn current)
        {
            while(_enumerator.TryGetNext(out current))
            {
                if (_filter(in current))
                    return true;
            }
            return false;
        }
    }

    public struct Where_InNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private InFunc<T, bool> _filter;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public Where_InNode(in NodeT nodeT, InFunc<T, bool> filter) => (_nodeT, _filter) = (nodeT, filter);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new Where_InNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (InFunc<EnumeratorElement, bool>)(object)_filter);
            return tail.CreateObject<CreationType, EnumeratorElement, Where_InNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TResult, Where_InFoward<T, FEnumerator>>(new Where_InFoward<T, FEnumerator>(fenum, _filter));
    }

    struct Where_InFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        InFunc<T, bool> _predicate;

        public Where_InFoward(in Next prior, InFunc<T, bool> predicate) => (_next, _predicate) = (prior, predicate);

        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_predicate(in input))
                return _next.ProcessNext(input);
            return true;
        }
    }
}
