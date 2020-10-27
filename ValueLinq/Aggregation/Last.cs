using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Last<T>
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.Last<EnumeratorElement, Enumerator>(ref enumerator);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            => Impl.CreateObjectViaFastEnumerator<TResult>();
    }

    struct LastOrDefault<T>
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.LastOrDefault<EnumeratorElement, Enumerator>(ref enumerator);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            => Impl.CreateObjectViaFastEnumerator<TResult>();
    }

    static partial class Impl
    {
        static EnumeratorElement DoLast<EnumeratorElement, Enumerator>(ref Enumerator enumerator, EnumeratorElement current)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            while (enumerator.TryGetNext(out var next))
                current = next;
            return current;
        }

        internal static EnumeratorElement Last<EnumeratorElement, Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                if (!enumerator.TryGetNext(out var current))
                    throw new InvalidOperationException();
                return DoLast(ref enumerator, current);
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        internal static EnumeratorElement LastOrDefault<EnumeratorElement, Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return DoLast(ref enumerator, default(EnumeratorElement));
            }
            finally
            {
                enumerator.Dispose();
            }
        }
    }
}
