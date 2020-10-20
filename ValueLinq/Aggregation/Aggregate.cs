using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Aggregate<T, TAccumulate>
        : INode
    {
        private TAccumulate _seed;
        private Func<TAccumulate, T, TAccumulate> _func;

        public Aggregate(TAccumulate seed, Func<TAccumulate, T, TAccumulate> func) => (_seed, _func) = (seed, func);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotImplementedException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
        {
            checked
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
        }
    }
}
