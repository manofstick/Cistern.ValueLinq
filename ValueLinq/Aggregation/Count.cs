namespace Cistern.ValueLinq.Aggregation
{
    struct Count
        : INode
    {
        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.Count<EnumeratorElement, Enumerator>(ref enumerator);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            => Impl.CreateObjectViaFastEnumerator<TResult>();
    }

    static partial class Impl
    {
        internal static int Count<EnumeratorElement, Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return DoCount(ref enumerator);
            }
            finally
            {
                enumerator.Dispose();
            }

            static int DoCount(ref Enumerator enumerator) =>
                enumerator.InitialSize switch
                {
                    (NoSelect:true, var size) => size,
                    _ => IterateElements(ref enumerator)
                };

            static int IterateElements(ref Enumerator enumerator)
            { 
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
