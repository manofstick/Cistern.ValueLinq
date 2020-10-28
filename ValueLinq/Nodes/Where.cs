using System;

namespace Cistern.ValueLinq.Nodes
{
    struct WhereNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, bool> _filter;

        public WhereNodeEnumerator(in TInEnumerator enumerator, Func<TIn, bool> filter) => (_enumerator, _filter) = (enumerator, filter);

        public (bool, int)? InitialSize =>
            _enumerator.InitialSize switch
            {
                (var flag, 0) => (flag, 0),
                _ => null
            };

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

    public struct WhereNode<T, NodeT>
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private Func<T, bool> _filter;

        public WhereNode(in NodeT nodeT, Func<T, bool> predicate) => (_nodeT, _filter) = (nodeT, predicate);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new WhereNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, bool>)(object)_filter);
            return tail.CreateObject<CreationType, EnumeratorElement, WhereNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
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

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TIn, TResult, WhereFoward<TIn, FEnumerator>>(new WhereFoward<TIn, FEnumerator>(fenum, (Func<TIn, bool>)(object)_filter));
    }

    struct WhereFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        Func<T, bool> _predicate;

        public WhereFoward(in Next prior, Func<T, bool> predicate) => (_next, _predicate) = (prior, predicate);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public void Init(int? size) => _next.Init(null);

        public bool ProcessNext(T input)
        {
            if (_predicate(input))
                return _next.ProcessNext(input);
            return true;
        }
    }
}
