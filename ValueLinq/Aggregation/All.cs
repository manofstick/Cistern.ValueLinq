using System;

namespace Cistern.ValueLinq.Aggregation
{
    struct All<T>
        : INode
    {
        private Func<T, bool> _predicate;

        public All(Func<T, bool> predicate) => _predicate = predicate;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.All(ref enumerator, (Func<EnumeratorElement, bool>)(object)_predicate);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    static partial class Impl
    {
        internal static bool All<EnumeratorElement, Enumerator>(ref Enumerator enumerator, Func<EnumeratorElement, bool> predicate)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return DoAll(ref enumerator, predicate);
            }
            finally
            {
                enumerator.Dispose();
            }

            static bool DoAll(ref Enumerator enumerator, Func<EnumeratorElement, bool> predicate)
            {
                while (enumerator.TryGetNext(out var current))
                {
                    if (!predicate(current))
                        return false;
                }
                return true;
            }
        }
    }
}
