using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct ForeachForward<T>
        : IForwardEnumerator<T>
    {
        private Action<T> _func;

        public ForeachForward(Action<T> func) => (_func) = (func);

        TResult IForwardEnumerator<T>.GetResult<TResult>() => default;

        void IForwardEnumerator<T>.Init(int? size) { }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _func(input);
            return true;
        }
    }
}
