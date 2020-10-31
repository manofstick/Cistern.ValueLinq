using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct All<T>
        : IForwardEnumerator<T>
    {
        private Func<T, bool> _predicate;
        private bool _all;

        public All(Func<T, bool> predicate) => (_predicate, _all) = (predicate, true);

        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_all;

        void IForwardEnumerator<T>.Init(int? size) { }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            if (!_predicate(input))
            {
                _all = false;
                return false;
            }
            return true;
        }
    }
}
