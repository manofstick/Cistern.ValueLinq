using System.Collections.Generic;

namespace Cistern.ValueLinq
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
        : IValueEnumerable<U>
        where NodeT : INode
    {
        private NodeT _nodeT;
        private InFunc<T, U> _map;

        public MapInNode(in NodeT nodeT, InFunc<T, U> map) => (_nodeT, _map) = (nodeT, map);

        public ValueEnumerator<U> GetEnumerator() => Nodes<U>.CreateValueEnumerator(in this);
        IEnumerator<U> IEnumerable<U>.GetEnumerator() => Nodes<U>.CreateEnumerator(in this);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<U>)this).GetEnumerator();

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, in Enumerator enumerator)
        {
            var x = new MapInNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (InFunc<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, MapInNodeEnumerator<EnumeratorElement, U, Enumerator>>(in x);
        }
    }
}
