namespace Cistern.ValueLinq.Nodes
{
    struct Select_InNodeEnumerator<TIn, TOut, TInEnumerator>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private InFunc<TIn, TOut> _map;

        public Select_InNodeEnumerator(in TInEnumerator enumerator, InFunc<TIn, TOut> map) => (_enumerator, _map) = (enumerator, map);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TOut current)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                current = _map(in currentIn);
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct Select_InNode<T, U, NodeT>
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private InFunc<T, U> _map;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        public Select_InNode(in NodeT nodeT, InFunc<T, U> map) => (_nodeT, _map) = (nodeT, map);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new Select_InNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (InFunc<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, Select_InNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<T, TResult, Select_InFoward<T, TIn, FEnumerator>>(new Select_InFoward<T, TIn, FEnumerator>(fenum, (InFunc<T, TIn>)(object)_map));
    }

    struct Select_InFoward<T, U, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<U>
    {
        Next _next;
        InFunc<T, U> _selector;

        public Select_InFoward(in Next prior, InFunc<T, U> predicate) => (_next, _selector) = (prior, predicate);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input) => _next.ProcessNext(_selector(in input));
    }

}
