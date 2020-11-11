﻿using System;

namespace Cistern.ValueLinq.Nodes
{
    struct SkipWhileIdxNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, int, bool> _predicate;
        bool _skipping;
        int _idx;

        public SkipWhileIdxNodeEnumerator(in TInEnumerator enumerator, Func<TIn, int, bool> predicate) => (_enumerator, _predicate, _skipping, _idx) = (enumerator, predicate, true, 0);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            while(_skipping)
            {
                if (!_enumerator.TryGetNext(out current))
                    goto dispose;

                if (!_predicate(current, _idx++))
                {
                    _skipping = false;
                    return true;
                }
            }

            if (_enumerator.TryGetNext(out current))
                return true;

            dispose:
            _enumerator.Dispose();
            return false;
        }
    }

    public struct SkipWhileIdxNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private Func<T, int, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public SkipWhileIdxNode(in NodeT nodeT, Func<T, int, bool> predicate) => (_nodeT, _predicate) = (nodeT, predicate);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SkipWhileIdxNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, int, bool>)(object)_predicate);
            return tail.CreateObject<CreationType, EnumeratorElement, SkipWhileIdxNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TResult, SkipWhileIdxFoward<T, FEnumerator>>(new SkipWhileIdxFoward<T, FEnumerator>(fenum, _predicate));
    }

    struct SkipWhileIdxFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        Func<T, int, bool> _predicate;
        int _idx;
        bool _skipping;

        public SkipWhileIdxFoward(in Next prior, Func<T, int, bool> predicate) => (_next, _predicate, _skipping, _idx) = (prior, predicate, true, 0);

        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_skipping)
            {
                if (_predicate(input, _idx++))
                    return true;
                _skipping = false;
            }
            return _next.ProcessNext(input);
        }
    }
}
