﻿using System;

namespace Cistern.ValueLinq.Containers
{
    struct ReturnFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private T _element;
        private bool _iterated;

        public ReturnFastEnumerator(T element) => (_element, _iterated) = (element, false);

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            if (!_iterated)
            {
                _iterated = true;
                current = _element;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct ReturnNode<T>
        : INode<T>
    {
        private T _element;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(1, true);

        public ReturnNode(T element) => _element = element;

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var enumerator = new ReturnFastEnumerator<T>(_element);
            return nodes.CreateObject<CreationType, T, ReturnFastEnumerator<T>>(ref enumerator);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
            => ReturnNode.FastEnumerate<T, TResult, FEnumerator>(_element, fenum);
    }

    static class ReturnNode
    {
        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(TIn element, FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                fenum.ProcessNext(element);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }
    }
}
