namespace Cistern.ValueLinq.Nodes
{
    struct Where_InNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private InFunc<TIn, bool> _filter;

        public Where_InNodeEnumerator(in TInEnumerator enumerator, InFunc<TIn, bool> filter) => (_enumerator, _filter) = (enumerator, filter);

        public int? InitialSize => null;

        public void Dispose() => _enumerator.Dispose();

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
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private InFunc<T, bool> _filter;

        public Where_InNode(in NodeT nodeT, InFunc<T, bool> filter) => (_nodeT, _filter) = (nodeT, filter);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new Where_InNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (InFunc<EnumeratorElement, bool>)(object)_filter);
            return tail.CreateObject<CreationType, EnumeratorElement, Where_InNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
