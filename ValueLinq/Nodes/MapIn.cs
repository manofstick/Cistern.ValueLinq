namespace Cistern.ValueLinq.Nodes
{
    struct MapInNodeEnumerator<TIn, TOut, TInEnumerator>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private InFunc<TIn, TOut> _map;

        public MapInNodeEnumerator(in TInEnumerator enumerator, InFunc<TIn, TOut> map) => (_enumerator, _map) = (enumerator, map);

        public int? InitialSize => _enumerator.InitialSize;

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

    public struct MapInNode<T, U, NodeT>
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private InFunc<T, U> _map;

        public MapInNode(in NodeT nodeT, InFunc<T, U> map) => (_nodeT, _map) = (nodeT, map);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new MapInNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (InFunc<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, MapInNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
