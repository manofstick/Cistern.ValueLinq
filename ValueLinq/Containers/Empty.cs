﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    class InstanceOfEmpty<T>
        : IEnumerator<T>
        , IEnumerable<T>
    {
        private static InstanceOfEmpty<T> _instance = new InstanceOfEmpty<T>();

        public static readonly IEnumerator<T> AsEnumerator = _instance;
        public static readonly IEnumerable<T> AsEnumerable = _instance;
        private InstanceOfEmpty() { }
        public T Current => default;
        object System.Collections.IEnumerator.Current => Current;
        public void Dispose() { }
        public bool MoveNext() => false;
        public void Reset() { }
        IEnumerator IEnumerable.GetEnumerator() => this;
        public IEnumerator<T> GetEnumerator() => this;
    }

    class InstanceOfEmptyFastEnumerator<T>
        : FastEnumerator<T>
    {
        public static readonly FastEnumerator<T> Instance = new InstanceOfEmptyFastEnumerator<T>();
        private InstanceOfEmptyFastEnumerator() { }

        public override void Dispose() { }

        public override bool TryGetNext(out T current)
        {
            current = default;
            return false;
        }
    }

    struct EmptyFastEnumerator<T>
        : IFastEnumerator<T>
    {
        public void Dispose() { }

        public bool TryGetNext(out T current) { current = default; return false; }
    }

    public struct EmptyNode<T>
        : INode<T>
    {
        public static INode<T> Empty { get; } = new EmptyNode<T>();

        public void GetCountInformation(out CountInformation info) => 
            info = new CountInformation(0, true);

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => EmptyNode.Create<T, Nodes<Head, Tail>, CreationType>(ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)Array.Empty<T>();
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)0;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                result = (TResult)(object)default(NodeContainer<T>);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                result = (TResult)(object)default(NodeContainer<T>);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                result = (TResult)(object)default(NodeContainer<T>);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
            => EmptyNode.FastEnumerate<T, TResult, FEnumerator>(fenum);
    }

    static class EmptyNode
    {
        public static CreationType Create<T, Nodes, CreationType>(ref Nodes nodes)
            where Nodes : INodes
        {
            var enumerator = new EmptyFastEnumerator<T>();
            return nodes.CreateObject<CreationType, T, EmptyFastEnumerator<T>>(ref enumerator);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }
    }
}
