using Cistern.ValueLinq.Containers;
using System;
using System.Collections;
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
            return tail.CreateObject<CreationType, U, SelectNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<U>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TResult, SelectFoward<T, U, FEnumerator>>(new SelectFoward<T, U, FEnumerator>(fenum, _map));
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
