using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Aggregate<T, TAccumulate>
        : INode
    {
        private TAccumulate _seed;
        private Func<TAccumulate, T, TAccumulate> _func;

        public Aggregate(TAccumulate seed, Func<TAccumulate, T, TAccumulate> func) => (_seed, _func) = (seed, func);

        public CreationType CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
            => throw new NotImplementedException();

        public CreationType CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, in Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
            where Tail : INodes
        {
            checked
            {
                var e = enumerator;
                try
                {
                    var state = (CreationType)(object)_seed;
                    var f = (Func<CreationType, EnumeratorElement, CreationType>)(object)_func;
                    while (e.TryGetNext(out var current))
                        state = f(state, current);
                    return state;
                }
                finally
                {
                    e.Dispose();
                }
            }
        }
    }
}
