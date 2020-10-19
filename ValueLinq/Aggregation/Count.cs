using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Count
        : INode
    {
        public CreationType CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
            => throw new NotImplementedException();

        public CreationType CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, in Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
            where Tail : INodes
        {
            checked
            {
                var e = enumerator;
                try
                {
                    var initialSize = e.InitialSize;
                    if (initialSize.HasValue)
                        return (CreationType)(object)initialSize.Value;

                    var count = 0;
                    while (e.TryGetNext(out var _))
                        ++count; ;
                    return (CreationType)(object)count;
                }
                finally
                {
                    e.Dispose();
                }
            }
        }
    }
}
