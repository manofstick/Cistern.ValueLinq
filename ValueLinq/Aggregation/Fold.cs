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

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotImplementedException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
        {
            try
            {
                var state = (CreationType)(object)_seed;
                var f = (Func<CreationType, EnumeratorElement, CreationType>)(object)_func;
                while (enumerator.TryGetNext(out var current))
                    state = f(state, current);
                return state;
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
