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

        public void GetCountInformation(out int? maximumLength)
        {
            maximumLength = null;
        }

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
            using var e = Nodes<T>.CreateValueEnumerator(_nodeT);
            return SelectManyImpl.Count(e.FastEnumerator, _map);
        }

        public TResult CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
            => _nodeT.CreateObjectViaFastEnumerator<T, TResult, SelectManyFoward<T, TIn, NodeU, FEnumerator>>(new SelectManyFoward<T, TIn, NodeU, FEnumerator>(new SelectManyCommonNext<TIn, FEnumerator>(in fenum), (Func<T, ValueEnumerable<TIn, NodeU>>)(object) _map));
    }

    static class SelectManyImpl
    {
        public static int Count<T, U, NodeU>(FastEnumerator<T> enumerator, Func<T, ValueEnumerable<U, NodeU>> _map)
            where NodeU : INode
        {
            checked
            {
                var count = 0;
                while (enumerator.TryGetNext(out var item))
                {
                    count += _map(item).Count();
                }
                return count;
            }
        }
    }

    sealed class SelectManyCommonNext<T, Next>
        where Next : IForwardEnumerator<T>
    {
        private Next _next;

        public SelectManyCommonNext(in Next next) => _next = next;

        public void Init(int? _) => _next.Init(null);
        public bool ProcessNext(T input) => _next.ProcessNext(input);
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();
    }

    struct SelectManyProcessNextForward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        SelectManyCommonNext<T, Next> _next;
        private bool _processNext;

        public SelectManyProcessNextForward(SelectManyCommonNext<T, Next> next) => (_next, _processNext) = (next, false);

        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_processNext;

        void IForwardEnumerator<T>.Init(int? size) {}

        bool IForwardEnumerator<T>.ProcessNext(T input) => _processNext = _next.ProcessNext(input);
    }

    struct SelectManyFoward<T, U, NodeU, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<U>
        where NodeU : INode
    {
        private SelectManyCommonNext<U, Next> _next;
        private Func<T, ValueEnumerable<U, NodeU>> _getEnumerable;

        public SelectManyFoward(in SelectManyCommonNext<U, Next> next, Func<T, ValueEnumerable<U, NodeU>> predicate) => (_next, _getEnumerable) = (next, predicate);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public void Init(int? size) => _next.Init(null);

        public bool ProcessNext(T input) =>
            _getEnumerable(input).Node.CreateObjectViaFastEnumerator<U, bool, SelectManyProcessNextForward<U, Next>>(new SelectManyProcessNextForward<U, Next>(_next));
    }
}
