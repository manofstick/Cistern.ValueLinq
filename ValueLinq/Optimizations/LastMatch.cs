using System;

namespace Cistern.ValueLinq.Optimizations
{
    [PushOptimization]
    internal interface IOptimizationLastMatch<T>
    {
        bool LastMatch(in LastMatch<T> request, out (bool, T) response);
    }

    /// <summary>
    /// The last element matching the predicate, as (found, element), answered where it is cheaper than running the
    /// sequence through this node (an ordering scans its source once for its greatest match, the latest among equal
    /// keys, and an IList&lt;T&gt; scans backwards to its last match, the predicate run on no element after it) and
    /// declined otherwise.
    /// </summary>
    readonly struct LastMatch<T>
    {
        public LastMatch(Func<T, bool> predicate) => Predicate = predicate;

        public Func<T, bool> Predicate { get; }
    }
}
