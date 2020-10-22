using System;

namespace Cistern.ValueLinq.Nodes
{
    struct SelectNodeEnumerator<TIn, TOut, TInEnumerator>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, TOut> _map;

        public SelectNodeEnumerator(in TInEnumerator enumerator, Func<TIn, TOut> map) => (_enumerator, _map) = (enumerator, map);

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

    public struct SelectNode<T, U, NodeT>
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private Func<T, U> _map;

        public SelectNode(in NodeT nodeT, Func<T, U> selector)
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            (_nodeT, _map) = (nodeT, selector);
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (Func<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, SelectNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToList_XXX))
            {
                return _nodeT.CheckForOptimization<TOuter, Optimizations.ToList_Select_XXX<T, U>, TResult>(new Optimizations.ToList_Select_XXX<T, U>(_map), out result);
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Select_XXX<U, TOuter>))
            {
                var fromRequest = (Optimizations.ToList_Select_XXX<U, TOuter>)(object)request;
                var u2Outer = fromRequest.Map;
                var t2u = _map;
                TOuter t2Outer(T t) => u2Outer(t2u(t));
                return _nodeT.CheckForOptimization<TOuter, Optimizations.ToList_Select_XXX<T, TOuter>, TResult>(new Optimizations.ToList_Select_XXX<T, TOuter>(t2Outer), out result);
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Where_XXX<TOuter>))
            {
                var fromRequest = (Optimizations.ToList_Where_XXX<U>)(object)request;
                return _nodeT.CheckForOptimization<U, Optimizations.ToList_Where_Select_XXX<T, U>, TResult>(new Optimizations.ToList_Where_Select_XXX<T, U>(_map, fromRequest.Filter), out result);
            }

            result = default;
            return false;
        }
    }
}
