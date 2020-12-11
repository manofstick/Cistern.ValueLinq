using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct WhereIdxNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, int, bool> _filter;
        private int _idx;

        public WhereIdxNodeEnumerator(in TInEnumerator enumerator, Func<TIn, int, bool> filter) => (_enumerator, _filter, _idx) = (enumerator, filter, 0);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TIn current)
        {
            while(_enumerator.TryGetNext(out current))
            {
                if (_filter(current, _idx++))
                    return true;
            }
            return false;
        }
    }

    public struct WhereIdxNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private Func<T, int, bool> _filter;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public WhereIdxNode(in NodeT nodeT, Func<T, int, bool> predicate) => (_nodeT, _filter) = (nodeT, predicate);

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new WhereIdxNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, int, bool>)(object)_filter);
            return tail.CreateObject<CreationType, EnumeratorElement, WhereIdxNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateViaPush<TResult, WhereIdxFoward<T, FEnumerator>>(new WhereIdxFoward<T, FEnumerator>(fenum, _filter));
    }

    struct WhereIdxFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        Func<T, int, bool> _predicate;
        int _idx;

        public WhereIdxFoward(in Next prior, Func<T, int, bool> predicate) => (_next, _predicate, _idx) = (prior, predicate, 0);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_predicate(input, _idx++))
                return _next.ProcessNext(input);
            return true;
        }
    }
}
