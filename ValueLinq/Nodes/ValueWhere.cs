﻿using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct ValueWhereNodeEnumerator<TIn, TInEnumerator, AlsoT, Predicate>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
        where Predicate : IFunc<AlsoT, bool>
    {
        private TInEnumerator _enumerator;
        private Predicate _filter;

        public ValueWhereNodeEnumerator(in TInEnumerator enumerator, Predicate filter) => (_enumerator, _filter) = (enumerator, filter);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            while (_enumerator.TryGetNext(out current))
            {
                if (_filter.Invoke((AlsoT)(object)current))
                    return true;
            }
            return false;
        }
    }

    public struct ValueWhereNode<T, NodeT, Predicate>
        : INode<T>
        where NodeT : INode<T>
        where Predicate : IFunc<T, bool>
    {
        private NodeT _nodeT;
        private Predicate _filter;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public ValueWhereNode(in NodeT nodeT, Predicate predicate) => (_nodeT, _filter) = (nodeT, predicate);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new ValueWhereNodeEnumerator<EnumeratorElement, Enumerator, T, Predicate>(in enumerator, _filter);
            return tail.CreateObject<CreationType, EnumeratorElement, ValueWhereNodeEnumerator<EnumeratorElement, Enumerator, T, Predicate>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TResult, ValueWhereFoward<T, FEnumerator, Predicate>>(new ValueWhereFoward<T, FEnumerator, Predicate>(fenum, _filter));
    }

    struct ValueWhereFoward<T, Next, Predicate>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
        where Predicate : IFunc<T, bool>
    {
        Next _next;
        Predicate _predicate;

        public ValueWhereFoward(in Next prior, Predicate predicate) => (_next, _predicate) = (prior, predicate);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            if (_predicate.Invoke(input))
                return _next.ProcessNext(input);
            return true;
        }
    }
}
