using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct WhereNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
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

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new WhereNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, bool>)(object)_filter);
            return tail.CreateObject<CreationType, EnumeratorElement, WhereNodeEnumerator<EnumeratorElement, Enumerator>>(0, ref nextEnumerator);
        }

        bool INode.TryObjectAscentOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            if (typeof(TRequest) == typeof(Optimizations.CheckForWhere))
            {
                creation = (CreationType)(object)_filter;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.SourceArray<T>))
                return WhereNode.CreateArray<T, CreationType, Tail>(((Optimizations.SourceArray<T>)(object)request).Array, _filter, ref tail, out creation);

            if (typeof(TRequest) == typeof(Optimizations.SourceList<T>))
                return WhereNode.CreateList<T, CreationType, Tail>(((Optimizations.SourceList<T>)(object)request).List, _filter, ref tail, out creation);

            if (typeof(TRequest) == typeof(Optimizations.SourceEnumerable<T>))
                return WhereNode.CreateEnumerable<T, CreationType, Tail>(((Optimizations.SourceEnumerable<T>)(object)request).Enumerable, _filter, ref tail, out creation);

            creation = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TResult, WhereFoward<T, FEnumerator>>(new WhereFoward<T, FEnumerator>(fenum, _filter));
    }

    static class WhereNode
    {
        internal static bool CreateEnumerable<T, CreationType, Tail>(IEnumerable<T> e, Func<T, bool> _filter, ref Tail tail, out CreationType creation) where Tail : INodes
        {
            if (tail.TryObjectAscentOptimization<Optimizations.SourceEnumerableWhere<T>, CreationType>(0, new Optimizations.SourceEnumerableWhere<T> { Enumerable = e, Predicate = _filter }, out creation))
                return true;

            var enumerator = new EnumerableFastWhereEnumerator<T>(e, _filter);
            creation = tail.CreateObject<CreationType, T, EnumerableFastWhereEnumerator<T>>(0, ref enumerator);
            return true;
        }

        internal static bool CreateList<T, CreationType, Tail>(List<T> l, Func<T, bool> _filter, ref Tail tail, out CreationType creation) where Tail : INodes
        {
            if (tail.TryObjectAscentOptimization<Optimizations.SourceListWhere<T>, CreationType>(0, new Optimizations.SourceListWhere<T> { List = l, Predicate = _filter }, out creation))
                return true;

            var enumerator = new ListFastWhereEnumerator<T>(l.GetEnumerator(), _filter); ;
            creation = tail.CreateObject<CreationType, T, ListFastWhereEnumerator<T>>(0, ref enumerator);
            return true;
        }

        internal static bool CreateArray<T, CreationType, Tail>(T[] a, Func<T, bool> _filter, ref Tail tail, out CreationType creation) where Tail : INodes
        {
            if (tail.TryObjectAscentOptimization<Optimizations.SourceArrayWhere<T>, CreationType>(0, new Optimizations.SourceArrayWhere<T> { Array = a, Predicate = _filter }, out creation))
                return true;

            var enumerator = new ArrayFastWhereEnumerator<T>(a, _filter);
            creation = tail.CreateObject<CreationType, T, ArrayFastWhereEnumerator<T>>(0, ref enumerator);
            return true;
        }


    }

    public struct WhereLegacyNode<T>
        : INode<T>
    {
        private IEnumerable<T> _enumerable;
        private Func<T, bool> _filter;

        public void GetCountInformation(out CountInformation info)
        {
            new EnumerableNode<T>(_enumerable).GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public WhereLegacyNode(IEnumerable<T> nodeT, Func<T, bool> predicate) => (_enumerable, _filter) = (nodeT, predicate);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => EnumerableNode.CreateObjectDescent<T, CreationType, Head, Tail>(ref nodes, _enumerable, _filter);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException("Shouldn't ascend through legacy node");

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            EnumerableNode.FastEnumerateSwitch<T, TResult, WhereFoward<T, FEnumerator>>(_enumerable, new WhereFoward<T, FEnumerator>(fenum, _filter));
    }

    struct WhereFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
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
