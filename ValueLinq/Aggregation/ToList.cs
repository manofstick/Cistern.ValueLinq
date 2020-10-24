using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    struct ToList
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToList<EnumeratorElement, Enumerator>(ref enumerator);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    static partial class Impl
    {
        internal static List<EnumeratorElement> ToList<EnumeratorElement, Enumerator>(ref Enumerator enumerator)
                where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return DoToList(ref enumerator);
            }
            finally
            {
                enumerator.Dispose();
            }

            static List<EnumeratorElement> DoToList(ref Enumerator enumerator)
            {
                var list =
                    enumerator.InitialSize switch
                    {
                        (_, var size) => new List<EnumeratorElement>(size),
                        _ => new List<EnumeratorElement>()
                    };

                while (enumerator.TryGetNext(out var current))
                    list.Add(current);

                return list;
            }
        }
    }
}
