﻿using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct FoldForward<T, TAccumulate>
        : IForwardEnumerator<T>
    {
        private TAccumulate _accumulate;
        private Func<TAccumulate, T, TAccumulate> _func;

        public FoldForward(Func<TAccumulate, T, TAccumulate> func, TAccumulate seed) => (_func, _accumulate) = (func, seed);

        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_accumulate;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _accumulate = _func(_accumulate, input);
            return true;
        }
    }
}
