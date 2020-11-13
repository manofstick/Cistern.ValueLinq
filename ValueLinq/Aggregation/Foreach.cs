using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct ForeachForward<T>
        : IForwardEnumerator<T>
    {
        private Action<T> _func;

        public ForeachForward(Action<T> func) => (_func) = (func);

        public bool CheckForOptimization<TObject, TRequest, TResult>(TObject obj, in TRequest request, out TResult result) { result = default; return false; }
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => default;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _func(input);
            return true;
        }
    }
}
