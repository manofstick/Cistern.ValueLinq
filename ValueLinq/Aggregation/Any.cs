using System;

#nullable enable

namespace Cistern.ValueLinq.Aggregation
{
    struct Any<T>
        : INode
    {
        private readonly Func<T, bool>? _maybePredicate;

        public Any(Func<T, bool>? predicate) => _maybePredicate = predicate;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator) =>
            (CreationType)(object)(
                (enumerator.InitialSize, _maybePredicate) switch
                {
                    ((_, 0), _) => false,
                    (_, null) => Impl.Any<EnumeratorElement, Enumerator>(ref enumerator),
                    (_, var predicate) => Impl.Any(ref enumerator, (Func<EnumeratorElement, bool>)(object)predicate)
                });

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    static partial class Impl
    {
        internal static bool Any<EnumeratorElement, Enumerator>(ref Enumerator enumerator, Func<EnumeratorElement, bool> predicate)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return DoAny(ref enumerator, predicate);
            }
            finally
            {
                enumerator.Dispose();
            }

            static bool DoAny(ref Enumerator enumerator, Func<EnumeratorElement, bool> predicate)
            {
                while (enumerator.TryGetNext(out var current))
                {
                    if (predicate(current))
                        return true;
                }
                return false;
            }
        }

        internal static bool Any<EnumeratorElement, Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return enumerator.TryGetNext(out var _);
            }
            finally
            {
                enumerator.Dispose();
            }
        }
    }
}
