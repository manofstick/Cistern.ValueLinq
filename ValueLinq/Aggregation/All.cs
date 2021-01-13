using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct All<T, Func>
        : IPushEnumerator<T>
        where Func : IFunc<T, bool>
    {
        private Func _predicate;
        private bool _all;

        public All(Func predicate) => (_predicate, _all) = (predicate, true);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IPushEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public bool GetResult() => _all;
        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate.Invoke(input))
            {
                _all = false;
                return false;
            }
            return true;
        }
    }
}
