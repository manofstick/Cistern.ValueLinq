using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Reduce<T>
        : INode
    {
        private Func<T, T, T> _func;

        public Reduce(Func<T, T, T> func) => _func = func;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.Reduce(ref enumerator, (Func<EnumeratorElement, EnumeratorElement, EnumeratorElement>)(object)_func);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            => Impl.CreateObjectViaFastEnumerator<TResult>();
    }

    static partial class Impl
    {
        internal static EnumeratorElement Reduce<EnumeratorElement, Enumerator>(ref Enumerator enumerator, Func<EnumeratorElement, EnumeratorElement, EnumeratorElement> f) 
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return DoReduce(ref enumerator, f);
            }
            finally
            {
                enumerator.Dispose();
            }

            static EnumeratorElement DoReduce(ref Enumerator enumerator, Func<EnumeratorElement, EnumeratorElement, EnumeratorElement> f)
            {
                if (!enumerator.TryGetNext(out var state))
                    throw new InvalidOperationException();

                while (enumerator.TryGetNext(out var current))
                    state = f(state, current);

                return state;
            }
        }
    }
}
