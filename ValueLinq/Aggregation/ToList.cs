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
            try
            {
                var list = CreateList<EnumeratorElement>(enumerator.InitialSize);
                PopulateList(ref enumerator, list);
                return (CreationType)(object)list;
            }
            finally
            {
                enumerator.Dispose();
            }
        }
        private static void PopulateList<EnumeratorElement, Enumerator>(ref Enumerator enumerator, List<EnumeratorElement> list) where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            while (enumerator.TryGetNext(out var current))
                list.Add(current);
        }

        private static List<EnumeratorElement> CreateList<EnumeratorElement>(Nullable<int> size)
        {
            if (size.HasValue)
                return new List<EnumeratorElement>(size.Value);

            return new List<EnumeratorElement>();
        }
    }
}
