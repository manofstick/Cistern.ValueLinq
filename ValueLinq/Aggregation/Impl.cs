﻿using System;

namespace Cistern.ValueLinq.Aggregation
{
    static partial class Impl
    {
        internal static void CountInfo(out CountInformation info)
            => throw new InvalidOperationException("Aggregation objects provide no count");

        internal static CreationType CreateObjectDescent<CreationType>()
            => throw new InvalidOperationException("Aggregation objects shouldn't be descending any further");

        public static bool TryPushOptimization<TResult>(out TResult _)
            => throw new InvalidOperationException("Aggregation objects shouldn't be partipating in Optimization check");
    }
}
