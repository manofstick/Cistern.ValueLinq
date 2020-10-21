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

        public int? InitialSize => null;

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
        , IOptimizedCreateCollectionOuter<T>
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

        TOptimization INode.CheckForOptimization<TOptimization>()
        {
            if (typeof(TOptimization) == typeof(IOptimizedCreateCollectionOuter<T>) && _nodeT is IOptimizedCreateCollectionInner<T>)
            {
                return (TOptimization)(object)(this);
            }
            return null;
        }

        public System.Collections.Generic.List<T> ToList() => (_nodeT as IOptimizedCreateCollectionInner<T>).ToList(_filter);
    }
}
