using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Linq;

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
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)Count();
                return true;
            }

            result = default;
            return false;
        }

        private readonly int Count()
        {
            checked
            {
                var e = Nodes<T>.CreateValueEnumerator(_nodeT).FastEnumerator;
                var count = 0;
                while (e.TryGetNext(out var item))
                {
                    count += Enumerable.Count(_map(item));
                }
                return count;
            }
        }

        public TResult CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
            => _nodeT.CreateObjectViaFastEnumerator<T, TResult, SelectManyFoward<T, TIn, NodeU, FEnumerator>>(new SelectManyFoward<T, TIn, NodeU, FEnumerator>(fenum, (Func<T, ValueEnumerable<TIn, NodeU>>)(object) _map));
    }

    struct SelectManyFoward<T, U, NodeU, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<U>
        where NodeU : INode
    {
        Next _next;
        private Func<T, ValueEnumerable<U, NodeU>> _getEnumerable;

        public SelectManyFoward(in Next prior, Func<T, ValueEnumerable<U, NodeU>> predicate) => (_next, _getEnumerable) = (prior, predicate);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public void Init(int? size) => _next.Init(size);

        public bool ProcessNext(T input)
        {
            // TODO: think of cheaper pass through mechanism
            var enumerator =
                _getEnumerable(input)
                .GetEnumerator()
                .FastEnumerator;

            while (enumerator.TryGetNext(out var current))
            {
                if (!_next.ProcessNext(current))
                    return false;
            }

            return true;
        }
    }

}
