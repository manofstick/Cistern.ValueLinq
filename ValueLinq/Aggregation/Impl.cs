using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Aggregation
{
    static partial class Impl
    {
        /// <summary>
        /// An aggregator's GetResult&lt;TResult&gt; is only ever asked for the type the aggregator produces. The value may
        /// legitimately be null when TFrom is a nullable reference type, which a boxing cast cannot express to the
        /// nullable analysis for an unconstrained type parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TResult ResultCast<TFrom, TResult>(TFrom value)
        {
            Debug.Assert(typeof(TFrom) == typeof(TResult));
            return Unsafe.As<TFrom, TResult>(ref value);
        }
    }
}
