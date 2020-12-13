﻿namespace Cistern.ValueLinq.Optimizations
{
    /// <summary>
    /// Returns an INode&lt;T&gt; which from which CreateObjectViaFastEnumerator can be called
    /// on an aggregating object.
    /// </summary>
    struct Skip
    {
        private Skip(int count) => Count = count;

        public int Count { get; }

        public static bool Try<T, Node>(ref Node node, int count, out NodeContainer<T> container)
            where Node : INode<T>
            => node.TryPushOptimization(new Skip(count), out container);
    }
}
