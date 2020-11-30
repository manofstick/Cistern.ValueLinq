using System;

namespace Cistern.ValueLinq
{
    public delegate U InFunc<T, U>(in T t);
    public delegate void RefAction<T, U>(ref T t, U u);

    public interface IFuncBase<T, U> { }

    public interface IFunc<T, U> : IFuncBase<T, U>
    {
        U Invoke(T t);
    }
    public interface IInFunc<T, U> : IFuncBase<T, U>
    {
        U Invoke(in T t);
    }

    public struct FuncToIFunc<T, U>
        : IFunc<T, U>
    {
        private Func<T, U> _func;

        public FuncToIFunc(Func<T, U> func) => _func = func;

        public U Invoke(T t) => _func(t);
    }
}
