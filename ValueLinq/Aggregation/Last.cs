using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Last<T>
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotImplementedException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.Last<EnumeratorElement, Enumerator>(enumerator);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }

    static partial class Impl
    {
        public static EnumeratorElement Last<EnumeratorElement, Enumerator>(Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                if (!enumerator.TryGetNext(out var current))
                    throw new InvalidOperationException();
                return InnerLoop(enumerator, current);
            }
            finally
            {
                enumerator.Dispose();
            }

            static EnumeratorElement InnerLoop(Enumerator enumerator, EnumeratorElement current)
            {
                while (enumerator.TryGetNext(out var next))
                    current = next;
                return current;
            }
        }
    }
}
