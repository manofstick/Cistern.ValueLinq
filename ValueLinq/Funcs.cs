using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public delegate U InFunc<T, U>(in T t);
    public delegate void RefAction<T, U>(ref T t, U u);

    // called through its constraint, never tested for with `is`: over a reference type the node's code is shared, and a
    // type test on the struct boxes it per call
    public interface IFunc<T, U>
    {
        U Invoke(T t);
    }

    public interface IRefAction<T, U>
    {
        void Invoke(ref T t, U u);
    }


    public readonly struct FuncToIFunc<T, U>
        : IFunc<T, U>
    {
        private readonly Func<T, U> _func;

        public FuncToIFunc(Func<T, U> func) => _func = func;

        public U Invoke(T t) => _func(t);
    }

    /// <summary>
    /// Wraps what a SelectMany selector returns as an SystemIEnumerable. Holding the selector here rather than in a
    /// lambda keeps the query free of a per-call closure.
    /// </summary>
    public readonly struct EnumerableNodeSelector<T, U>(Func<T, IEnumerable<U>> selector)
        : IFunc<T, Containers.SystemIEnumerable.Node<U>>
    {
        public Containers.SystemIEnumerable.Node<U> Invoke(T t) => new(selector(t));
    }

    public readonly struct ArrayNodeSelector<T, U>(Func<T, U[]> selector)
        : IFunc<T, Containers.SystemArray.Node<U>>
    {
        public Containers.SystemArray.Node<U> Invoke(T t) => new(selector(t), false);
    }

}
