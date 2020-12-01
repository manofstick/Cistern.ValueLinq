using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct ForEachForward<T>
        : IForwardEnumerator<T>
    {
        private Action<T> _func;

        public ForEachForward(Action<T> func) => (_func) = (func);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => default;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _func(input);
            return true;
        }
    }

    struct ForEachForwardRef<T, U>
        : IForwardEnumerator<U>
    {
        private T _state;
        private RefAction<T, U> _func;

        public ForEachForwardRef(T seed, RefAction<T, U> func) => (_state, _func) = (seed, func);

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

    struct ValueForeachForwardRef<T, U, RefAction>
        : IForwardEnumerator<U>
        where RefAction : IRefAction<T, U>
    {
        private T _state;
        private RefAction _func;

        public ValueForeachForwardRef(T seed, RefAction func) => (_state, _func) = (seed, func);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<U>.GetResult<TResult>() => (TResult)(object)GetResult();
        public T GetResult() => _state;

        bool IForwardEnumerator<U>.ProcessNext(U input)
        {
            _func.Invoke(ref _state, input);
            return true;
        }
    }
}
