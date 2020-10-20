using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    struct MapInOutNodeEnumerator<TIn, TOut, TInEnumerator>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private InOutFunc<TIn, TOut> _map;

        public MapInOutNodeEnumerator(in TInEnumerator enumerator, InOutFunc<TIn, TOut> map) => (_enumerator, _map) = (enumerator, map);

        public int? InitialSize => _enumerator.InitialSize;

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TOut currentOut)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                _map(in currentIn, out currentOut);
                return true;
            }
            currentOut = default;
            return false;
        }
    }

    public struct MapInOutNode<T, U, NodeT>
        : IValueEnumerable<U>
        where NodeT : INode
    {
        private NodeT _nodeT;
        private InOutFunc<T, U> _map;

        public MapInOutNode(in NodeT nodeT, InOutFunc<T, U> map) => (_nodeT, _map) = (nodeT, map);

        public ValueEnumerator<U> GetEnumerator() => Nodes<U>.CreateValueEnumerator(in this);
        IEnumerator<U> IEnumerable<U>.GetEnumerator() => Nodes<U>.CreateEnumerator(in this);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<U>)this).GetEnumerator();

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new MapInOutNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (InOutFunc<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, MapInOutNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }
    }
}
