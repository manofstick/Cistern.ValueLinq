using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct SumInt
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
                    var total = 0;
                    while (e.TryGetNext(out var current))
                        total += (int)(object)current;
                    return (CreationType)(object)total;
                }
                finally
                {
                    e.Dispose();
                }
            }
        }
    }
    struct SumDouble
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
                    var total = 0.0;
                    while (e.TryGetNext(out var current))
                        total += (double)(object)current;
                    return (CreationType)(object)total;
                }
                finally
                {
                    e.Dispose();
                }
            }
        }
    }
}
