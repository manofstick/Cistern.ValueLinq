using System;

namespace Cistern.ValueLinq.Optimizations
{
    /// <summary>
    /// Returns a bool or Memory&lt;T&gt;, dependant on Probe flag
    /// </summary>
    struct AsMemory
    {
        private AsMemory(bool probe) => Probe = probe;

        public bool Probe { get; }

        public static bool IsAvailable<T, Node>(ref Node node)
            where Node : INode<T>
            => node.TryPushOptimization<AsMemory, object>(new AsMemory(true), out object _);

        public static bool TryGet<T, Node>(ref Node node, out ReadOnlyMemory<T> memory)
            where Node : INode<T>
            => node.TryPushOptimization(new AsMemory(false), out memory);
    }
}
