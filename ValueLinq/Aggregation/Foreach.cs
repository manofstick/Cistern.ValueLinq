using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct ForeachForward<T>
        : IForwardEnumerator<T>
    {
        private Action<T> _func;

        public ForeachForward(Action<T> func) => (_func) = (func);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => default;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _func(input);
            return true;
        }
    }

    struct ForeachForwardRef<T, U>
        : IForwardEnumerator<U>
    {
        private T _state;
        private RefAction<T, U> _func;

        public ForeachForwardRef(T seed, RefAction<T, U> func) => (_state, _func) = (seed, func);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<U>.GetResult<TResult>() => (TResult)(object)GetResult();
        public T GetResult() => _state;

        bool IForwardEnumerator<U>.ProcessNext(U input)
        {
            _func(ref _state, input);

            return true;
        }
    }
}
