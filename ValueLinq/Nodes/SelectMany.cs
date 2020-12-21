﻿using System;

namespace Cistern.ValueLinq.Nodes
{

    struct SelectManyNodeEnumerator2<NodeU_Alt, U, TInEnumerator, NodeU>
        : IFastEnumerator<U>
        where TInEnumerator : IFastEnumerator<NodeU_Alt>
        where NodeU : INode<U>
    {
        private TInEnumerator _outer;
        private FastEnumerator<U> _inner;

        public SelectManyNodeEnumerator2(in TInEnumerator enumerator) => (_outer, _inner) = (enumerator, FastEnumerator<U>.Empty);

        public void Dispose()
        {
            _inner.Dispose();
            _outer.Dispose();
        }

        public bool TryGetNext(out U current)
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
            _inner = FastEnumerator<U>.Empty;

            if (!_outer.TryGetNext(out var next))
            {
                return false;
            }

            _inner = Nodes<U>.CreateFastEnumerator((NodeU)(object)next);

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

    public struct SelectManyNode2<U, NodeU, NodeEU>
        : INode<U>
        where NodeU : INode<U>
        where NodeEU : INode<NodeU>
    {
        private NodeEU _nodeEU;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation();

        public SelectManyNode2(in NodeEU nodeEU) => (_nodeEU) = (nodeEU);

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeEU, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectManyNodeEnumerator2<EnumeratorElement, U, Enumerator, NodeU>(in enumerator);
            return tail.CreateObject<CreationType, U, SelectManyNodeEnumerator2<EnumeratorElement, U, Enumerator, NodeU>>(ref nextEnumerator);
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
            // TODO: change to forwards?
            using var e = Nodes<NodeU>.CreateFastEnumerator(_nodeEU);
            return SelectManyImpl.Count<U, NodeU>(e);
        }

        TResult INode<U>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
            => _nodeEU.CreateViaPush<TResult, SelectManyFoward2<U, NodeU, FEnumerator, NodeEU>>(new SelectManyFoward2<U, NodeU, FEnumerator, NodeEU>(new SelectManyCommonNext<U, FEnumerator>(in fenum)));
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
        public static int Count<U, NodeU>(FastEnumerator<NodeU> enumerator)
            where NodeU : INode<U>
        {
            checked
            {
                var count = 0;
                while (enumerator.TryGetNext(out var item))
                {
                    count += NodeImpl.Count<U, NodeU>(item, false);
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

    struct SelectManyFoward2<U, NodeU, Next, NodeEU>
        : IForwardEnumerator<NodeU>
        where Next : IForwardEnumerator<U>
        where NodeU : INode<U>
        where NodeEU : INode<NodeU>
    {
        private SelectManyCommonNext<U, Next> _next;

        public SelectManyFoward2(in SelectManyCommonNext<U, Next> next) => _next = next;

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(NodeU input) =>
            input.CreateViaPush<bool, SelectManyProcessNextForward<U, Next>>(new SelectManyProcessNextForward<U, Next>(_next));
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
