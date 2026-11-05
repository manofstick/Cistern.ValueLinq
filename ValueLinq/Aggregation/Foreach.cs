using System;

namespace Cistern.ValueLinq.Aggregation
{
    [Aggregator]
    readonly partial struct ForEachForward<T>
        : IPushEnumerator<T>
    {
        private readonly Action<T> _func;

        public ForEachForward(Action<T> func) => (_func) = (func);

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<TPushResult?, TPushResult>(default);

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _func(input);
            return true;
        }
    }

    [Aggregator]
    partial struct ForEachForwardRef<T, U>
        : IPushEnumerator<U>
    {
        private T _state;
        private RefAction<T, U> _func;

        public ForEachForwardRef(T seed, RefAction<T, U> func)
        {
            _state = seed;
            _func = func;
        }

        TPushResult IPushEnumerator<U>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());
        public T GetResult() => _state;

        bool IPushEnumerator<U>.ProcessNext(U input)
        {
            _func(ref _state, input);

            return true;
        }
    }

    [Aggregator]
    partial struct ValueForeachForwardRef<T, U, RefAction>
        : IPushEnumerator<U>
        where RefAction : IRefAction<T, U>
    {
        private T _state;
        private RefAction _func;

        public ValueForeachForwardRef(T seed, RefAction func)
        {
            _state = seed;
            _func = func;
        }

        TPushResult IPushEnumerator<U>.GetResult<TPushResult>() => Impl.ResultCast<T, TPushResult>(GetResult());
        public T GetResult() => _state;

        bool IPushEnumerator<U>.ProcessNext(U input)
        {
            _func.Invoke(ref _state, input);
            return true;
        }
    }
}
