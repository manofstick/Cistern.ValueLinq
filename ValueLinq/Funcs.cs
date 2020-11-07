using System;

namespace Cistern.ValueLinq
{
    public delegate U InFunc<T, U>(in T t);

    public interface IFunc<T, U>
    {
        U Invoke(T t);
    }

    public struct FuncToIFunc<T, U>
        : IFunc<T, U>
    {
        private Func<T, U> _func;

        public FuncToIFunc(Func<T, U> func) => _func = func;

        public U Invoke(T t) => _func(t);
    }
}
