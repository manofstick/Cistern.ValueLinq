using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct SelectNodeEnumerator<TSource, TResult, TPullEnumerator>
        : IPullEnumerator<TResult>
        where TPullEnumerator : IPullEnumerator<TSource>
    {
        private TPullEnumerator _enumerator;
        private Func<TSource, TResult> _map;

        public SelectNodeEnumerator(in TPullEnumerator enumerator, Func<TSource, TResult> map) => (_enumerator, _map) = (enumerator, map);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TResult current)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                current = _map(currentIn);
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct SelectNode<T, U, NodeT>
        : INode<U>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private Func<T, U> _map;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        public SelectNode(in NodeT nodeT, Func<T, U> selector) => (_nodeT, _map) = (nodeT, selector);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (Func<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, SelectNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            if (typeof(TRequest) == typeof(Optimizations.SourceArray<T>))
            {
                var src = (Optimizations.SourceArray<T>)(object)request;
                return SelectNode.CreateArray<T, U, CreationType, Tail>(in src, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceArrayWhere<T>))
            {
                var src = (Optimizations.SourceArrayWhere<T>)(object)request;
                return SelectNode.CreateArray<T, U, CreationType, Tail>(in src, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceList<T>))
            {
                var src = (Optimizations.SourceList<T>)(object)request;
                return SelectNode.CreateList<T, U, CreationType, Tail>(src.List, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceListWhere<T>))
            {
                var src = (Optimizations.SourceListWhere<T>)(object)request;
                return SelectNode.CreateList<T, U, CreationType, Tail>(src.List, src.Predicate, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceEnumerable<T>))
            {
                var src = ((Optimizations.SourceEnumerable<T>)(object)request);
                return SelectNode.CreateEnumerable<T, U, CreationType, Tail>(src.Enumerable, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceEnumerableWhere<T>))
            {
                var src = (Optimizations.SourceEnumerableWhere<T>)(object)request;
                return SelectNode.CreateEnumerable<T, U, CreationType, Tail>(src.Enumerable, src.Predicate, _map, ref tail, out creation);
            }

            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode<U>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum) =>
            _nodeT.CreateViaPush<TResult, SelectFoward<T, U, TPushEnumerator>>(new SelectFoward<T, U, TPushEnumerator>(fenum, _map));
    }

    class SelectNode
    {
        internal static bool CreateEnumerable<T, U, CreationType, Nodes>(IEnumerable<T> e, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new EnumerableSelectPullEnumerator<T, U>(e, _map);
            creation = tail.CreateObject<CreationType, U, EnumerableSelectPullEnumerator<T, U>>(ref enumerator);
            return true;
        }

        internal static bool CreateEnumerable<T, U, CreationType, Nodes>(IEnumerable<T> e, Func<T, bool> predicate, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new EnumerableWhereSelectPullEnumerator<T, U>(e, predicate, _map);
            creation = tail.CreateObject<CreationType, U, EnumerableWhereSelectPullEnumerator<T, U>>(ref enumerator);
            return true;
        }

        internal static bool CreateList<T, U, CreationType, Nodes>(List<T> l, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new ListSelectPullEnumerator<T, U>(l.GetEnumerator(), _map); ;
            creation = tail.CreateObject<CreationType, U, ListSelectPullEnumerator<T, U>>(ref enumerator);
            return true;
        }

        internal static bool CreateList<T, U, CreationType, Nodes>(List<T> l, Func<T, bool> predicate, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new ListWhereSelectPullEnumerator<T, U>(l.GetEnumerator(), predicate, _map); ;
            creation = tail.CreateObject<CreationType, U, ListWhereSelectPullEnumerator<T, U>>(ref enumerator);
            return true;
        }

        internal static bool CreateArray<T, U, CreationType, Nodes>(in Optimizations.SourceArray<T> src, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new ArraySelectPullEnumerator<T, U>(src.Array, src.Start, src.Count, _map);
            creation = tail.CreateObject<CreationType, U, ArraySelectPullEnumerator<T, U>>(ref enumerator);
            return true;
        }

        internal static bool CreateArray<T, U, CreationType, Nodes>(in Optimizations.SourceArrayWhere<T> src, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new ArrayWhereSelectPullEnumerator<T, U>(src.Array, src.Start, src.Count, src.Predicate, _map);
            creation = tail.CreateObject<CreationType, U, ArrayWhereSelectPullEnumerator<T, U>>(ref enumerator);
            return true;
        }
    }

    struct SelectFoward<T, U, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<U>
    {
        internal Next _next;

        Func<T, U> _selector;

        public SelectFoward(in Next prior, Func<T, U> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            (_next, _selector) = (prior, selector);
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();
        public bool ProcessNext(T input) => _next.ProcessNext(_selector(input));
    }

}
