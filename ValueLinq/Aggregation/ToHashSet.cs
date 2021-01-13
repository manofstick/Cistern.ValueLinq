using System.Collections.Generic;

namespace Cistern.ValueLinq.Aggregation
{
    struct ToHashSet<T>
        : IPushEnumerator<T>
    {
        private HashSet<T> _hashSet;

        public ToHashSet(IEqualityComparer<T> comparer) => _hashSet = new HashSet<T>(comparer);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IPushEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public HashSet<T> GetResult() => _hashSet;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _hashSet.Add(input);
            return true;
        }
    }
}
