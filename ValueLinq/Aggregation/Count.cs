namespace Cistern.ValueLinq.Aggregation
{
    struct Count
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.Count<EnumeratorElement, Enumerator>(ref enumerator);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    static partial class Impl
    {
        internal static int Count<EnumeratorElement, Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return DoCount(enumerator);
            }
            finally
            {
                enumerator.Dispose();
            }

            static int DoCount(Enumerator enumerator)
            {
                var initialSize = enumerator.InitialSize;
                if (initialSize.HasValue)
                    return initialSize.Value;

                var count = 0;
                checked
                {
                    while (enumerator.TryGetNext(out var _))
                        ++count;
                }
                return count;
            }
        }
    }
}
