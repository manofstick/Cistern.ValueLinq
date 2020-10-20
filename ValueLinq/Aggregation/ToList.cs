using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    struct ToList
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
                    var list = initialSize.HasValue ? new List<EnumeratorElement>(initialSize.Value) : new List<EnumeratorElement>();
                    while (enumerator.TryGetNext(out var current))
                        list.Add(current);
                    return (CreationType)(object)list;
                }
                finally
                {
                    enumerator.Dispose();
                }
            }
        }
    }
}
