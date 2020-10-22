using System;

namespace Cistern.ValueLinq.Nodes
{
    struct FilterNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, bool> _filter;

        public FilterNodeEnumerator(in TInEnumerator enumerator, Func<TIn, bool> filter) => (_enumerator, _filter) = (enumerator, filter);

        public int? InitialSize => _enumerator.InitialSize == 0 ? (int?)0 : null;

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            while(_enumerator.TryGetNext(out current))
            {
                if (_filter(current))
                    return true;
            }
            return false;
        }
    }

    public struct FilterNode<T, NodeT>
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private Func<T, bool> _filter;

        public FilterNode(in NodeT nodeT, Func<T, bool> filter) => (_nodeT, _filter) = (nodeT, filter);


        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new FilterNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, bool>)(object)_filter);
            return tail.CreateObject<CreationType, EnumeratorElement, FilterNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToList_XXX))
            {
                return _nodeT.CheckForOptimization<TOuter, Optimizations.ToList_Where_XXX<T>, TResult>(new Optimizations.ToList_Where_XXX<T>(_filter), out result);
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Where_XXX<T>))
            {
                var fromRequest = (Optimizations.ToList_Where_XXX<T>)(object)request;
                var outer = fromRequest.Filter;
                var inner = _filter;
                bool combined(T t) => inner(t) && outer(t);
                return _nodeT.CheckForOptimization<TOuter, Optimizations.ToList_Where_XXX<T>, TResult>(new Optimizations.ToList_Where_XXX<T>(combined), out result);
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Select_XXX<T, TOuter>))
            {
                var fromRequest = (Optimizations.ToList_Select_XXX<T, TOuter>)(object)request;
                return _nodeT.CheckForOptimization<TOuter, Optimizations.ToList_Select_Where_XXX<T, TOuter>, TResult>(new Optimizations.ToList_Select_Where_XXX<T, TOuter>(_filter, fromRequest.Map), out result);
            }

            result = default;
            return false;
        }
    }
}
