using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    struct FilterInNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private InFunc<TIn, bool> _filter;

        public FilterInNodeEnumerator(in TInEnumerator enumerator, InFunc<TIn, bool> filter) => (_enumerator, _filter) = (enumerator, filter);

        public int? InitialSize => null;

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            while(_enumerator.TryGetNext(out current))
            {
                if (_filter(in current))
                    return true;
            }
            return false;
        }
    }

    public struct FilterInNode<T, NodeT>
        : IValueEnumerable<T>
        where NodeT : INode
    {
        private NodeT _nodeT;
        private InFunc<T, bool> _filter;

        public FilterInNode(in NodeT nodeT, InFunc<T, bool> filter) => (_nodeT, _filter) = (nodeT, filter);

        public ValueEnumerator<T> GetEnumerator() => Nodes<T>.CreateValueEnumerator(in this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Nodes<T>.CreateEnumerator(in this);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, in Enumerator enumerator)
        {
            var x = new FilterInNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (InFunc<EnumeratorElement, bool>)(object)_filter);
            return tail.CreateObject<CreationType, EnumeratorElement, FilterInNodeEnumerator<EnumeratorElement, Enumerator>>(in x);
        }
    }
}
