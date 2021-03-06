﻿using System;

namespace Cistern.ValueLinq.Nodes
{
    struct SelectManyNodeEnumerator<NodeU_Alt, U, TInEnumerator, NodeU>
        : IPullEnumerator<U>
        where TInEnumerator : IPullEnumerator<NodeU_Alt>
        where NodeU : INode<U>
    {
        private TInEnumerator _outer;
        private PullEnumerator<U> _inner;

        public SelectManyNodeEnumerator(in TInEnumerator enumerator) => (_outer, _inner) = (enumerator, PullEnumerator<U>.Empty);

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
            _inner = PullEnumerator<U>.Empty;

            if (!_outer.TryGetNext(out var next))
            {
                return false;
            }

            _inner = Nodes<U>.CreatePullEnumerator((NodeU)(object)next);

            return true;
        }
    }

    struct SelectManyNodeEnumerator<NodeU_Alt, TSource, TCollection, TResult, TSourceEnumerator, NodeCollection>
        : IPullEnumerator<TResult>
        where TSourceEnumerator : IPullEnumerator<NodeU_Alt>
        where NodeCollection : INode<TCollection>
    {
        private TSourceEnumerator _outer;
        private Func<TSource, TCollection, TResult> _resultSelector;
        private TSource _source;
        private PullEnumerator<TCollection> _inner;

        public SelectManyNodeEnumerator(in TSourceEnumerator sourceEnumerator, Func<TSource, TCollection, TResult> resultSelector)
            => (_outer, _resultSelector, _source, _inner) = (sourceEnumerator, resultSelector, default, PullEnumerator<TCollection>.Empty);

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
            _inner = PullEnumerator<TCollection>.Empty;

            if (!_outer.TryGetNext(out var tmp))
            {
                return false;
            }

            var correctlyTyped = ((TSource, NodeCollection))(object)tmp;
            _source = correctlyTyped.Item1;
            _inner = Nodes<TCollection>.CreatePullEnumerator(correctlyTyped.Item2);

            return true;
        }
    }

    public struct SelectManyNode<U, NodeU, NodeEU>
        : INode<U>
        where NodeU : INode<U>
        where NodeEU : INode<NodeU>
    {
        private NodeEU _nodeEU;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation();

        public SelectManyNode(in NodeEU nodeEU) => (_nodeEU) = (nodeEU);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => Nodes<CreationType>.Descend(ref _nodeEU, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectManyNodeEnumerator<EnumeratorElement, U, Enumerator, NodeU>(in enumerator);
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
            // TODO: change to forwards?
            using var e = Nodes<NodeU>.CreatePullEnumerator(_nodeEU);
            return SelectManyImpl.Count<U, NodeU>(e);
        }

        TResult INode<U>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => _nodeEU.CreateViaPush<TResult, SelectManyFoward2<U, NodeU, TPushEnumerator, NodeEU>>(new SelectManyFoward2<U, NodeU, TPushEnumerator, NodeEU>(new SelectManyCommonNext<U, TPushEnumerator>(in fenum)));
    }

    public struct SelectManyNode<TSource, TCollection, TResult, NodeCollection, NodeSrcCol>
        : INode<TResult>
        where NodeCollection : INode<TCollection>
        where NodeSrcCol : INode<(TSource, NodeCollection)>
    {
        private NodeSrcCol _nodeSource;
        private Func<TSource, TCollection, TResult> _resultSelector;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation();

        public SelectManyNode(in NodeSrcCol nodeSource, Func<TSource, TCollection, TResult> resultSelector)
            => (_nodeSource, _resultSelector) = (nodeSource, resultSelector);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => Nodes<CreationType>.Descend(ref _nodeSource, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectManyNodeEnumerator<EnumeratorElement, TSource, TCollection, TResult, Enumerator, NodeCollection>(in enumerator, _resultSelector);
            return tail.CreateObject<CreationType, TResult, SelectManyNodeEnumerator<EnumeratorElement, TSource, TCollection, TResult, Enumerator, NodeCollection>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, TObjResult, Nodes>(in TRequest request, ref Nodes nodes, out TObjResult creation) { creation = default; return false; }
        bool INode.TryPushOptimization<TRequest, TResultLocal>(in TRequest request, out TResultLocal result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResultLocal)(object)Count();
                return true;
            }

            result = default;
            return false;
        }

        private readonly int Count()
        {
            // TODO: change to forwards?
            using var e = Nodes<NodeCollection>.CreatePullEnumerator(_nodeSource);
            return SelectManyImpl.Count<TCollection, NodeCollection>(e);
        }

        TObjResult INode<TResult>.CreateViaPush<TObjResult, TPushEnumerator>(in TPushEnumerator fenum) =>
            _nodeSource.CreateViaPush<TObjResult, SelectManyFoward<TSource, TCollection, TResult, NodeCollection, TPushEnumerator>>(new SelectManyFoward<TSource, TCollection, TResult, NodeCollection, TPushEnumerator>(new SelectManyCommonNext<TResult, TPushEnumerator>(in fenum), _resultSelector));
    }

    static class SelectManyImpl
    {
        public static int Count<U, NodeU>(PullEnumerator<NodeU> enumerator)
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
        where Next : IPushEnumerator<T>
    {
        private Next _next;

        public SelectManyCommonNext(in Next next) => _next = next;

        public bool ProcessNext(T input) => _next.ProcessNext(input);
        public void Dispose() { }
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();
    }

    struct SelectManyProcessNextForward<T, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<T>
    {
        SelectManyCommonNext<T, Next> _next;
        private bool _processNext;

        public SelectManyProcessNextForward(SelectManyCommonNext<T, Next> next) => (_next, _processNext) = (next, true);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IPushEnumerator<T>.GetResult<TResult>() => (TResult)(object)_processNext;

        bool IPushEnumerator<T>.ProcessNext(T input) => _processNext = _next.ProcessNext(input);
    }

    struct SelectManyProcessNextForward<TSource, TCollection, TResult, Next>
        : IPushEnumerator<TCollection>
        where Next : IPushEnumerator<TResult>
    {
        private TSource _source;
        private Func<TSource, TCollection, TResult> _resultSelector;
        private SelectManyCommonNext<TResult, Next> _next;
        private bool _processNext;

        public SelectManyProcessNextForward(TSource source, Func<TSource, TCollection, TResult> resultSelector, SelectManyCommonNext<TResult, Next> next)
            => (_source, _resultSelector, _next, _processNext) = (source, resultSelector, next, true);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TObjResult IPushEnumerator<TCollection>.GetResult<TObjResult>() => (TObjResult)(object)_processNext;

        bool IPushEnumerator<TCollection>.ProcessNext(TCollection input) => _processNext = _next.ProcessNext(_resultSelector(_source, input));
    }

    struct SelectManyFoward2<U, NodeU, Next, NodeEU>
        : IPushEnumerator<NodeU>
        where Next : IPushEnumerator<U>
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
        : IPushEnumerator<(TSource, NodeCollection)>
        where Next : IPushEnumerator<TResult>
        where NodeCollection : INode<TCollection>
    {
        private SelectManyCommonNext<TResult, Next> _next;
        private Func<TSource, TCollection, TResult> _resultSelector;

        public SelectManyFoward(in SelectManyCommonNext<TResult, Next> next, Func<TSource, TCollection, TResult> resultSelector)
            => (_next, _resultSelector) = (next, resultSelector);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();

        public TObjResult GetResult<TObjResult>() => _next.GetResult<TObjResult>();

        public bool ProcessNext((TSource, NodeCollection) input) =>
            input.Item2.CreateViaPush<bool, SelectManyProcessNextForward<TSource, TCollection, TResult, Next>>(new SelectManyProcessNextForward<TSource, TCollection, TResult, Next>(input.Item1, _resultSelector, _next));
    }

}
