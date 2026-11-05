using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    readonly partial struct ToHashSet<T>
        : IPushEnumerator<T>
    {
        private readonly HashSet<T> _hashSet;

        public ToHashSet(IEqualityComparer<T>? comparer) => _hashSet = new HashSet<T>(comparer);

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public HashSet<T> GetResult() => _hashSet;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _hashSet.Add(input);
            return true;
        }
    }
}
