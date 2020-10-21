﻿using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct SumInt
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotImplementedException();
        
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
        {
            checked
            {
                try
                {
                    var total = 0;
                    while (enumerator.TryGetNext(out var current))
                        total += (int)(object)current;
                    return (CreationType)(object)total;
                }
                finally
                {
                    enumerator.Dispose();
                }
            }
        }

        T INode.CheckForOptimization<T>() => null;
    }
    struct SumDouble
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotImplementedException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
        {
            checked
            {
                try
                {
                    var total = 0.0;
                    while (enumerator.TryGetNext(out var current))
                        total += (double)(object)current;
                    return (CreationType)(object)total;
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
