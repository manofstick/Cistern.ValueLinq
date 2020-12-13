namespace Cistern.ValueLinq.Optimizations
{
    /// <summary>
    /// Returns an NodeContainer&lt;T&gt; which from which CreateObjectViaFastEnumerator can be called
    /// on an aggregating object.
    /// </summary>
    struct Reverse
    {
        public static bool Try<T, Node>(ref Node node, out NodeContainer<T> container)
            where Node : INode<T>
            => node.TryPushOptimization(new Reverse(), out container);
    }
}
