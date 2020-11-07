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

        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_all;

        void IForwardEnumerator<T>.Init(int? size) { }

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
