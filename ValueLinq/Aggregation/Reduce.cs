using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct Reduce<T>
        : INode
    {
        private Func<T, T, T> _func;

        public Reduce(Func<T, T, T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            _func = func;
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotImplementedException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
        {
            try
            {
                var f = (Func<EnumeratorElement, EnumeratorElement, EnumeratorElement>)(object)_func;

                if (!enumerator.TryGetNext(out var state))
                    throw new InvalidOperationException();

                while (enumerator.TryGetNext(out var current))
                    state = f(state, current);

                return (CreationType)(object)state;
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
