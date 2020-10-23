using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Fold<T, TAccumulate>
        : INode
    {
        private TAccumulate _seed;
        private Func<TAccumulate, T, TAccumulate> _func;

        public Fold(TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            (_seed, _func) = (seed, func);
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => Impl.Fold(ref enumerator, (CreationType)(object)_seed, (Func<CreationType, EnumeratorElement, CreationType>)(object)_func);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
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
}
