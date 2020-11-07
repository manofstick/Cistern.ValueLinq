using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct EmptyFastEnumerator<T>
        : IFastEnumerator<T>
    {
        public (bool, int)? InitialSize => (true, 0);

        public void Dispose() { }

        public bool TryGetNext(out T current) { current = default; return false; }
    }

    public struct EmptyNode<T>
        : INode
    {
        public void GetCountInformation(out int? maximumLength)
        {
            maximumLength = 0;
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => EmptyNode.Create<T, Head, Tail, CreationType>(ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)0;
                return true;
            }

            result = default;
            return false;
        }

        public TResult CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
            => EmptyNode.FastEnumerate<TIn, TResult, FEnumerator>(fenum);
    }

    static class EmptyNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new EmptyFastEnumerator<T>();
            return nodes.CreateObject<CreationType, T, EmptyFastEnumerator<T>>(ref enumerator);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            fenum.Init(0);
            return fenum.GetResult<TResult>();
        }
    }
}
