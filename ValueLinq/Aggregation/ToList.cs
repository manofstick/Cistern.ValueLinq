using System;
using System.Collections.Generic;
using System.Text;

namespace Cistern.ValueLinq.Aggregation
{
    struct ToList
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
                    var list = initialSize.HasValue ? new List<EnumeratorElement>(initialSize.Value) : new List<EnumeratorElement>();
                    while (e.TryGetNext(out var current))
                        list.Add(current);
                    return (CreationType)(object)list;
                }
                finally
                {
                    e.Dispose();
                }
            }
        }
    }
}
