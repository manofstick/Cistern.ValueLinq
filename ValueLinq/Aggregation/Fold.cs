using System;

namespace Cistern.ValueLinq.Aggregation
{
#if OLD_WAY
    struct Fold<T, TAccumulate>
        : INode
    {
        private TAccumulate _seed;
        private Func<TAccumulate, T, TAccumulate> _func;

        public Fold(TAccumulate seed, Func<TAccumulate, T, TAccumulate> func) => (_seed, _func) = (seed, func);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => Impl.Fold(ref enumerator, (CreationType)(object)_seed, (Func<CreationType, EnumeratorElement, CreationType>)(object)_func);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            => Impl.CreateObjectViaFastEnumerator<TResult>();
    }

    static partial class Impl
    {
        internal static TAccumulate Fold<EnumeratorElement, Enumerator, TAccumulate>(ref Enumerator enumerator, TAccumulate seed, Func<TAccumulate, EnumeratorElement, TAccumulate> f)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return DoFold(ref enumerator, seed, f);
            }
            finally
            {
                enumerator.Dispose();
            }

            static TAccumulate DoFold(ref Enumerator enumerator, TAccumulate state, Func<TAccumulate, EnumeratorElement, TAccumulate> f)
            {
                while (enumerator.TryGetNext(out var current))
                    state = f(state, current);
                return state;
            }
        }
    }
#endif

    struct FoldForward<T, TAccumulate>
        : IForwardEnumerator<T>
    {
        private TAccumulate _accumulate;
        private Func<TAccumulate, T, TAccumulate> _func;

        public FoldForward(Func<TAccumulate, T, TAccumulate> func, TAccumulate seed) => (_func, _accumulate) = (func, seed);

        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_accumulate;

        void IForwardEnumerator<T>.Init(int? size) { }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _accumulate = _func(_accumulate, input);
            return true;
        }
    }


}
