using System;

namespace Cistern.ValueLinq.Nodes
{
    struct SelectiNodeEnumerator<TIn, TOut, TInEnumerator>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, int, TOut> _map;
        private int _i;

        public SelectiNodeEnumerator(in TInEnumerator enumerator, Func<TIn, int, TOut> map) => (_enumerator, _map, _i) = (enumerator, map, 0);

        public (bool, int)? InitialSize =>
            _enumerator.InitialSize switch
            {
                null => null,
                (_, var size) => (false, size)
            };

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TOut current)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                current = _map(currentIn, _i++);
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct SelectiNode<T, U, NodeT>
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private Func<T, int, U> _map;

        public SelectiNode(in NodeT nodeT, Func<T, int, U> selector) => (_nodeT, _map) = (nodeT, selector);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectiNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (Func<EnumeratorElement, int, U>)(object)_map);
            return tail.CreateObject<CreationType, U, SelectiNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false;}

        public TResult CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            throw new NotImplementedException();
        }
    }
}
