using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Nodes
{
    struct MapNodeEnumerator<TIn, TOut, TInEnumerator>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, TOut> _map;

        public MapNodeEnumerator(in TInEnumerator enumerator, Func<TIn, TOut> map) => (_enumerator, _map) = (enumerator, map);

        public int? InitialSize => _enumerator.InitialSize;

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TOut current)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                current = _map(currentIn);
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct MapNode<T, U, NodeT>
        : INode
        , IOptimizedCreateCollectionOuter<U>
        where NodeT : INode
    {
        private NodeT _nodeT;
        private Func<T, U> _map;

        public MapNode(in NodeT nodeT, Func<T, U> map) => (_nodeT, _map) = (nodeT, map);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new MapNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (Func<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, MapNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        List<U> IOptimizedCreateCollectionOuter<U>.ToList() => (_nodeT as IOptimizedCreateCollectionInner<T>).ToList(_map);

        TOptimization INode.CheckForOptimization<TOptimization>()
        {
            if (typeof(TOptimization) == typeof(IOptimizedCreateCollectionOuter<U>) && _nodeT is IOptimizedCreateCollectionInner<T>)
            {
                return (TOptimization)(object)(this);
            }
            return null;
        }
    }
}
