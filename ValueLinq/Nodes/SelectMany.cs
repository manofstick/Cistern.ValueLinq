using System;

namespace Cistern.ValueLinq.Nodes
{
    struct SelectManyNodeEnumerator<TIn, TOut, TInEnumerator, NodeU>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
        where NodeU : INode<TOut>
    {
        private TInEnumerator _outer;
        private Func<TIn, ValueEnumerable<TOut, NodeU>> _getInner;
        private FastEnumerator<TOut> _inner;

        public SelectManyNodeEnumerator(in TInEnumerator enumerator, Func<TIn, ValueEnumerable<TOut, NodeU>> getInner) => (_outer, _getInner, _inner) = (enumerator, getInner, FastEnumerator<TOut>.Empty);

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

            _inner = Nodes<TOut>.CreateFastEnumerator(_getInner(next));

            return true;
        }
    }

    struct SelectManyNodeEnumerator<TSource, TCollection, TResult, TSourceEnumerator, NodeCollection>
        : IFastEnumerator<TResult>
        where TSourceEnumerator : IFastEnumerator<TSource>
        where NodeCollection : INode<TCollection>
    {
        private TSourceEnumerator _outer;
        private Func<TSource, ValueEnumerable<TCollection, NodeCollection>> _collectionSelector;
        private Func<TSource, TCollection, TResult> _resultSelector;
        private TSource _source;
        private FastEnumerator<TCollection> _inner;

        public SelectManyNodeEnumerator(in TSourceEnumerator sourceEnumerator, Func<TSource, ValueEnumerable<TCollection, NodeCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            => (_outer, _collectionSelector, _resultSelector, _source, _inner) = (sourceEnumerator, collectionSelector, resultSelector, default, FastEnumerator<TCollection>.Empty);

        public void Dispose()
        {
            _inner.Dispose();
            _outer.Dispose();
        }

        public bool TryGetNext(out TResult current)
        {
            for (; ; )
            {
                if (_inner.TryGetNext(out var collectionItem))
                {
                    current = _resultSelector(_source, collectionItem);
                    return true;
                }

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
            _inner = FastEnumerator<TCollection>.Empty;

            if (!_outer.TryGetNext(out _source))
            {
                return false;
            }

            _inner = Nodes<TCollection>.CreateFastEnumerator(_collectionSelector(_source));

            return true;
        }
    }

    public struct SelectManyNode<T, U, NodeT, NodeU>
        : INode<U>
        where NodeT : INode<T>
        where NodeU : INode<U>
    {
        private NodeT _nodeT;
        private Func<T, ValueEnumerable<U, NodeU>> _map;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation();

        public SelectManyNode(in NodeT nodeT, Func<T, ValueEnumerable<U, NodeU>> selector) => (_nodeT, _map) = (nodeT, selector);

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectManyNodeEnumerator<EnumeratorElement, U, Enumerator, NodeU>(in enumerator, (Func<EnumeratorElement, ValueEnumerable<U, NodeU>>)(object)_map);
            return tail.CreateObject<CreationType, U, SelectManyNodeEnumerator<EnumeratorElement, U, Enumerator, NodeU>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }
        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
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
            // TODO: change to forwards
            using var e = Nodes<T>.CreateFastEnumerator(_nodeT);
            return SelectManyImpl.Count(e, _map);
        }

        TResult INode<U>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
            => _nodeT.CreateViaPush<TResult, SelectManyFoward<T, U, NodeU, FEnumerator>>(new SelectManyFoward<T, U, NodeU, FEnumerator>(new SelectManyCommonNext<U, FEnumerator>(in fenum), _map));
    }

    public struct SelectManyNode<TSource, TCollection, TResult, NodeSource, NodeCollection>
        : INode<TResult>
        where NodeSource : INode<TSource>
        where NodeCollection : INode<TCollection>
    {
        private NodeSource _nodeSource;
        private Func<TSource, ValueEnumerable<TCollection, NodeCollection>> _collectionSelector;
        private Func<TSource, TCollection, TResult> _resultSelector;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation();

        public SelectManyNode(in NodeSource nodeSource, Func<TSource, ValueEnumerable<TCollection, NodeCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            => (_nodeSource, _collectionSelector, _resultSelector) = (nodeSource, collectionSelector, resultSelector);

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeSource, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectManyNodeEnumerator<EnumeratorElement, TCollection, TResult, Enumerator, NodeCollection>(in enumerator, (Func<EnumeratorElement, ValueEnumerable<TCollection, NodeCollection>>)(object)_collectionSelector, (Func<EnumeratorElement, TCollection, TResult>)(object)_resultSelector);
            return tail.CreateObject<CreationType, TResult, SelectManyNodeEnumerator<EnumeratorElement, TCollection, TResult, Enumerator, NodeCollection>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, TObjResult, Nodes>(in TRequest request, ref Nodes nodes, out TObjResult creation) { creation = default; return false; }
        bool INode.TryPushOptimization<TRequest, TObjResult>(in TRequest request, out TObjResult result)
        {
            result = default;
            return false;
        }

        TObjResult INode<TResult>.CreateViaPush<TObjResult, FEnumerator>(in FEnumerator fenum)
            => _nodeSource.CreateViaPush<TObjResult, SelectManyFoward<TSource, TCollection, TResult, NodeCollection, FEnumerator>>(new SelectManyFoward<TSource, TCollection, TResult, NodeCollection, FEnumerator>(new SelectManyCommonNext<TResult, FEnumerator>(in fenum), _collectionSelector, _resultSelector));
    }

    static class SelectManyImpl
    {
        public static int Count<T, U, NodeU>(FastEnumerator<T> enumerator, Func<T, ValueEnumerable<U, NodeU>> _map)
            where NodeU : INode<U>
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

        public bool ProcessNext(T input) => _next.ProcessNext(input);
        public void Dispose() { }
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();
    }

    struct SelectManyProcessNextForward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        SelectManyCommonNext<T, Next> _next;
        private bool _processNext;

        public SelectManyProcessNextForward(SelectManyCommonNext<T, Next> next) => (_next, _processNext) = (next, true);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_processNext;

        bool IForwardEnumerator<T>.ProcessNext(T input) => _processNext = _next.ProcessNext(input);
    }

    struct SelectManyProcessNextForward<TSource, TCollection, TResult, Next>
        : IForwardEnumerator<TCollection>
        where Next : IForwardEnumerator<TResult>
    {
        private TSource _source;
        private Func<TSource, TCollection, TResult> _resultSelector;
        private SelectManyCommonNext<TResult, Next> _next;
        private bool _processNext;

        public SelectManyProcessNextForward(TSource source, Func<TSource, TCollection, TResult> resultSelector, SelectManyCommonNext<TResult, Next> next)
            => (_source, _resultSelector, _next, _processNext) = (source, resultSelector, next, true);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TObjResult IForwardEnumerator<TCollection>.GetResult<TObjResult>() => (TObjResult)(object)_processNext;

        bool IForwardEnumerator<TCollection>.ProcessNext(TCollection input) => _processNext = _next.ProcessNext(_resultSelector(_source, input));
    }

    struct SelectManyFoward<T, U, NodeU, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<U>
        where NodeU : INode<U>
    {
        private SelectManyCommonNext<U, Next> _next;
        private Func<T, ValueEnumerable<U, NodeU>> _getEnumerable;

        public SelectManyFoward(in SelectManyCommonNext<U, Next> next, Func<T, ValueEnumerable<U, NodeU>> predicate) => (_next, _getEnumerable) = (next, predicate);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input) =>
            _getEnumerable(input).Node.CreateViaPush<bool, SelectManyProcessNextForward<U, Next>>(new SelectManyProcessNextForward<U, Next>(_next));
    }

    struct SelectManyFoward<TSource, TCollection, TResult, NodeCollection, Next>
        : IForwardEnumerator<TSource>
        where Next : IForwardEnumerator<TResult>
        where NodeCollection : INode<TCollection>
    {
        private SelectManyCommonNext<TResult, Next> _next;
        private Func<TSource, ValueEnumerable<TCollection, NodeCollection>> _collectionSelector;
        private Func<TSource, TCollection, TResult> _resultSelector;

        public SelectManyFoward(in SelectManyCommonNext<TResult, Next> next, Func<TSource, ValueEnumerable<TCollection, NodeCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            => (_next, _collectionSelector, _resultSelector) = (next, collectionSelector, resultSelector);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();

        public TObjResult GetResult<TObjResult>() => _next.GetResult<TObjResult>();

        public bool ProcessNext(TSource input) =>
            _collectionSelector(input).Node.CreateViaPush<bool, SelectManyProcessNextForward<TSource, TCollection, TResult, Next>>(new SelectManyProcessNextForward<TSource, TCollection, TResult, Next>(input, _resultSelector, _next));
    }

}
