using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Count
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotImplementedException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
        {
            checked
            {
                try
                {
                    var initialSize = enumerator.InitialSize;
                    if (initialSize.HasValue)
                        return (CreationType)(object)initialSize.Value;

                    var count = 0;
                    while (enumerator.TryGetNext(out var _))
                        ++count; ;
                    return (CreationType)(object)count;
                }
                finally
                {
                    enumerator.Dispose();
                }
            }
        }

        T INode.CheckForOptimization<T>() => null;
    }
}
