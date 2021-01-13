using System;
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

    class InstanceOfEmptyPullEnumerator<T>
        : PullEnumerator<T>
    {
        public static readonly PullEnumerator<T> Instance = new InstanceOfEmptyPullEnumerator<T>();
        private InstanceOfEmptyPullEnumerator() { }

        public override void Dispose() { }

        public override bool TryGetNext(out T current)
        {
            current = default;
            return false;
        }
    }

    struct EmptyPullEnumerator<T>
        : IPullEnumerator<T>
    {
        public void Dispose() { }

        public bool TryGetNext(out T current)
        { 
            current = default;
            return false;
        }
    }

    public struct EmptyNode<T>
        : INode<T>
    {
        public static INode<T> Empty { get; } = new EmptyNode<T>();

        public void GetCountInformation(out CountInformation info) => 
            info = new CountInformation(0, true);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => EmptyNode.Create<T, TNodes, CreationType>(ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => EmptyNode.TryPushOptimization<T, TRequest, TResult>(in request, out result);

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => EmptyNode.ExecutePush<T, TResult, TPushEnumerator>(fenum);
    }

    static class EmptyNode
    {
        internal static bool TryPushOptimization<T, TRequest, TResult>(in TRequest request, out TResult result)
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

        public static CreationType Create<T, Nodes, CreationType>(ref Nodes nodes)
            where Nodes : INodes
        {
            var enumerator = new EmptyPullEnumerator<T>();
            return nodes.CreateObject<CreationType, T, EmptyPullEnumerator<T>>(ref enumerator);
        }

        internal static TResult ExecutePush<TElement, TResult, TPushEnumerator>(TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TElement>
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
