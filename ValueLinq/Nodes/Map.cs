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

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToList))
            {
                return _nodeT.CheckForOptimization<TOuter, Optimizations.ToListSelect<T, U>, TResult>(new Optimizations.ToListSelect<T, U>(_map), out result);
            }

            if (typeof(TRequest) == typeof(Optimizations.ToListSelect<U, TOuter>))
            {
                var fromRequest = (Optimizations.ToListSelect<U, TOuter>)(object)request;
                var u2Outer = fromRequest.Map;
                var t2u = _map;
                TOuter t2Outer(T t) => u2Outer(t2u(t));
                return _nodeT.CheckForOptimization<TOuter, Optimizations.ToListSelect<T, TOuter>, TResult>(new Optimizations.ToListSelect<T, TOuter>(t2Outer), out result);
            }

            result = default;
            return false;
        }
    }
}
