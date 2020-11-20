using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct SelectNodeEnumerator<TIn, TOut, TInEnumerator>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, TOut> _map;

        public SelectNodeEnumerator(in TInEnumerator enumerator, Func<TIn, TOut> map) => (_enumerator, _map) = (enumerator, map);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TOut current)
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

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (Func<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, SelectNodeEnumerator<EnumeratorElement, U, Enumerator>>(0, ref nextEnumerator);
        }
        bool INode.TryObjectAscentOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            if (typeof(TRequest) == typeof(Optimizations.SourceArray<T>))
            {
                return SelectNode.CreateArray<T, U, CreationType, Tail>(((Optimizations.SourceArray<T>)(object)request).Array, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceArrayWhere<T>))
            {
                var arrayWhere = (Optimizations.SourceArrayWhere<T>)(object)request;
                return SelectNode.CreateArray<T, U, CreationType, Tail>(arrayWhere.Array, arrayWhere.Predicate, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceList<T>))
            {
                return SelectNode.CreateList<T, U, CreationType, Tail>(((Optimizations.SourceList<T>)(object)request).List, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceListWhere<T>))
            {
                var listWhere = (Optimizations.SourceListWhere<T>)(object)request;
                return SelectNode.CreateList<T, U, CreationType, Tail>(listWhere.List, listWhere.Predicate, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceEnumerable<T>))
            {
                return SelectNode.CreateEnumerable<T, U, CreationType, Tail>(((Optimizations.SourceEnumerable<T>)(object)request).Enumerable, _map, ref tail, out creation);
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceEnumerableWhere<T>))
            {
                var listWhere = (Optimizations.SourceEnumerableWhere<T>)(object)request;
                return SelectNode.CreateEnumerable<T, U, CreationType, Tail>(listWhere.Enumerable, listWhere.Predicate, _map, ref tail, out creation);
            }

            creation = default;
            return false;
        }

        TResult INode<U>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TResult, SelectFoward<T, U, FEnumerator>>(new SelectFoward<T, U, FEnumerator>(fenum, _map));
    }

    class SelectNode
    {
        internal static bool CreateEnumerable<T, U, CreationType, Nodes>(IEnumerable<T> e, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new EnumerableFastSelectEnumerator<T, U>(e, _map);
            creation = tail.CreateObject<CreationType, U, EnumerableFastSelectEnumerator<T, U>>(0, ref enumerator);
            return true;
        }

        internal static bool CreateEnumerable<T, U, CreationType, Nodes>(IEnumerable<T> e, Func<T, bool> predicate, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new EnumerableFastWhereSelectEnumerator<T, U>(e, predicate, _map);
            creation = tail.CreateObject<CreationType, U, EnumerableFastWhereSelectEnumerator<T, U>>(0, ref enumerator);
            return true;
        }

        internal static bool CreateList<T, U, CreationType, Nodes>(List<T> l, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new ListFastSelectEnumerator<T, U>(l.GetEnumerator(), _map); ;
            creation = tail.CreateObject<CreationType, U, ListFastSelectEnumerator<T, U>>(0, ref enumerator);
            return true;
        }

        internal static bool CreateList<T, U, CreationType, Nodes>(List<T> l, Func<T, bool> predicate, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new ListFastWhereSelectEnumerator<T, U>(l.GetEnumerator(), predicate, _map); ;
            creation = tail.CreateObject<CreationType, U, ListFastWhereSelectEnumerator<T, U>>(0, ref enumerator);
            return true;
        }

        internal static bool CreateArray<T, U, CreationType, Nodes>(T[] a, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new ArrayFastSelectEnumerator<T, U>(a, _map);
            creation = tail.CreateObject<CreationType, U, ArrayFastSelectEnumerator<T, U>>(0, ref enumerator);
            return true;
        }

        internal static bool CreateArray<T, U, CreationType, Nodes>(T[] a, Func<T, bool> predicate, Func<T, U> _map, ref Nodes tail, out CreationType creation)
            where Nodes : INodes
        {
            var enumerator = new ArrayFastWhereSelectEnumerator<T, U>(a, predicate, _map);
            creation = tail.CreateObject<CreationType, U, ArrayFastWhereSelectEnumerator<T, U>>(0, ref enumerator);
            return true;
        }
    }

    public struct SelectLegacyNode<T, U>
        : INode<U>
    {
        private IEnumerable<T> _enumerable;
        private Func<T, U> _map;

        public void GetCountInformation(out CountInformation info)
        {
            new EnumerableNode<T>(_enumerable).GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        public SelectLegacyNode(IEnumerable<T> enumerable, Func<T, U> selector) => (_enumerable, _map) = (enumerable, selector);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            EnumerableNode.CreateObjectDescent<T, U, CreationType, Head, Tail>(ref nodes, _enumerable, _map);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException("Shouldn't ascend through legacy node");

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode<U>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            EnumerableNode.FastEnumerateSwitch<T, TResult, SelectFoward<T, U, FEnumerator>>(_enumerable, new SelectFoward<T, U, FEnumerator>(fenum, _map));
    }

    struct SelectFoward<T, U, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<U>
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
