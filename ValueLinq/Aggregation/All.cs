﻿using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct All<T>
        : INode
    {
        private Func<T, bool> _predicate;

        public All(Func<T, bool> predicate) => _predicate = predicate;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotImplementedException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
        {
            try
            {
                var f = (Func<EnumeratorElement, bool>)(object)_predicate;
                while (enumerator.TryGetNext(out var current))
                {
                    if (!f(current))
                        return (CreationType)(object)false;
                }
                return (CreationType)(object)true;
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
