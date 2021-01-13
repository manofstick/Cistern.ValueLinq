using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct WhereNodeEnumerator<TIn, TInEnumerator>
        : IPullEnumerator<TIn>
        where TInEnumerator : IPullEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, bool> _filter;

        public WhereNodeEnumerator(in TInEnumerator enumerator, Func<TIn, bool> filter) => (_enumerator, _filter) = (enumerator, filter);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private Func<T, bool> _filter;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public WhereNode(in NodeT nodeT, Func<T, bool> predicate) => (_nodeT, _filter) = (nodeT, predicate);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new WhereNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, bool>)(object)_filter);
            return tail.CreateObject<CreationType, EnumeratorElement, WhereNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            if (typeof(TRequest) == typeof(Optimizations.CheckForWhere))
            {
                creation = (CreationType)(object)_filter;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceArray<T>))
            {
                var src = (Optimizations.SourceArray<T>)(object)request;
                return WhereNode.CreateArray<T, CreationType, Tail>(in src, _filter, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceList<T>))
            {
                var src = (Optimizations.SourceList<T>)(object)request;
                return WhereNode.CreateList<T, CreationType, Tail>(src.List, _filter, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceEnumerable<T>))
            {
                var src = (Optimizations.SourceEnumerable<T>)(object)request;
                return WhereNode.CreateEnumerable<T, CreationType, Tail>(src.Enumerable, _filter, ref tail, out creation);
            }

            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum) =>
            _nodeT.CreateViaPush<TResult, WhereFoward<T, TPushEnumerator>>(new WhereFoward<T, TPushEnumerator>(fenum, _filter));
    }

    static class WhereNode
    {
        internal static bool CreateEnumerable<T, CreationType, Tail>(IEnumerable<T> e, Func<T, bool> _filter, ref Tail tail, out CreationType creation) where Tail : INodes
        {
            if (tail.TryObjectAscentOptimization<Optimizations.SourceEnumerableWhere<T>, CreationType>(new Optimizations.SourceEnumerableWhere<T> { Enumerable = e, Predicate = _filter }, out creation))
                return true;

            var enumerator = new EnumerableWherePullEnumerator<T>(e, _filter);
            creation = tail.CreateObject<CreationType, T, EnumerableWherePullEnumerator<T>>(ref enumerator);
            return true;
        }

        internal static bool CreateList<T, CreationType, Tail>(List<T> l, Func<T, bool> _filter, ref Tail tail, out CreationType creation) where Tail : INodes
        {
            if (tail.TryObjectAscentOptimization<Optimizations.SourceListWhere<T>, CreationType>(new Optimizations.SourceListWhere<T> { List = l, Predicate = _filter }, out creation))
                return true;

            var enumerator = new ListWherePullEnumerator<T>(l.GetEnumerator(), _filter); ;
            creation = tail.CreateObject<CreationType, T, ListWherePullEnumerator<T>>(ref enumerator);
            return true;
        }

        internal static bool CreateArray<T, CreationType, Tail>(in Optimizations.SourceArray<T> src, Func<T, bool> _filter, ref Tail tail, out CreationType creation) where Tail : INodes
        {
            if (tail.TryObjectAscentOptimization<Optimizations.SourceArrayWhere<T>, CreationType>(new Optimizations.SourceArrayWhere<T> { Array = src.Array, Start = src.Start, Count = src.Count, Predicate = _filter }, out creation))
                return true;

            var enumerator = new ArrayWherePullEnumerator<T>(src.Array, src.Start, src.Count, _filter);
            creation = tail.CreateObject<CreationType, T, ArrayWherePullEnumerator<T>>(ref enumerator);
            return true;
        }
    }

    struct WhereFoward<T, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<T>
    {
        Next _next;
        Func<T, bool> _predicate;

        public WhereFoward(in Next prior, Func<T, bool> predicate) => (_next, _predicate) = (prior, predicate);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_predicate(input))
                return _next.ProcessNext(input);
            return true;
        }
    }
}
