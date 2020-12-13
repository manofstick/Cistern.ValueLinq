using System.Collections.Generic;

namespace Cistern.ValueLinq.Optimizations
{
    /// <summary>
    /// Returns a List&lt;T&gt;
    /// </summary>
    struct ToList
    {
        public static bool Try<T, Node>(ref Node node, out List<T> list)
            where Node : INode<T>
            => node.TryPushOptimization(new ToList(), out list);
    }
}
