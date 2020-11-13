using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct All<T, Func>
        : IForwardEnumerator<T>
        where Func : IFunc<T, bool>
    {
        private Func _predicate;
        private bool _all;

        public All(Func predicate) => (_predicate, _all) = (predicate, true);

        public bool CheckForOptimization<TObject, TRequest, TResult>(TObject obj, in TRequest request, out TResult result) { result = default; return false; }
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_all;

        bool IForwardEnumerator<T>.ProcessNext(T input)
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
