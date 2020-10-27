using System;

namespace Cistern.ValueLinq.Aggregation
{
    static partial class Impl
    {
        internal static CreationType CreateObjectDescent<CreationType>()
            => throw new InvalidOperationException("Aggregation objects shouldn't be descending any further");

        public static bool CheckForOptimization<TResult>(out TResult _)
            => throw new InvalidOperationException("Aggregation objects shouldn't be partipating in Optimization check");

        public static TResult CreateObjectViaFastEnumerator<TResult>()
            => throw new InvalidOperationException("Aggregation objects shouldn't be partipating in Optimization check");
    }
}
