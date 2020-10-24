using Cistern.ValueLinq.ValueEnumerable;
using System;

namespace Cistern.ValueLinq.Nodes
{
    struct SelectManyNodeEnumerator<TIn, TOut, TInEnumerator, NodeU>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
        where NodeU : INode
    {
        private TInEnumerator _outer;
        private Func<TIn, ValueEnumerable<TOut, NodeU>> _getInner;
        private FastEnumerator<TOut> _inner;

        public SelectManyNodeEnumerator(in TInEnumerator enumerator, Func<TIn, ValueEnumerable<TOut, NodeU>> getInner) => (_outer, _getInner, _inner) = (enumerator, getInner, FastEnumerator<TOut>.Empty);

        public (bool, int)? InitialSize => null;

        public void Dispose()
        {
            _inner.Dispose();
            _outer.Dispose();
        }

        public bool TryGetNext(out TOut current)
        {
            for (;;)
            {
                if (_inner.TryGetNext(out current))
                    return true;

                if (!Next())
                {
                    current = default;
                    return false;
                }
            }
        }

        public bool Next()
        {
            _inner.Dispose();
            _inner = FastEnumerator<TOut>.Empty;

            if (!_outer.TryGetNext(out var next))
            {
                return false;
            }

            _inner = _getInner(next).GetEnumerator().FastEnumerator;

            return true;
        }
    }

    public struct SelectManyNode<T, U, NodeT, NodeU>
        : INode
        where NodeT : INode
        where NodeU : INode
    {
        private NodeT _nodeT;
        private Func<T, ValueEnumerable<U, NodeU>> _map;

        public SelectManyNode(in NodeT nodeT, Func<T, ValueEnumerable<U, NodeU>> selector) => (_nodeT, _map) = (nodeT, selector);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectManyNodeEnumerator<EnumeratorElement, U, Enumerator, NodeU>(in enumerator, (Func<EnumeratorElement, ValueEnumerable<U, NodeU>>)(object)_map);
            return tail.CreateObject<CreationType, U, SelectManyNodeEnumerator<EnumeratorElement, U, Enumerator, NodeU>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }
    }
}
