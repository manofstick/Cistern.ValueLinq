namespace Cistern.ValueLinq.Optimizations
{
    [PushOptimization]
    internal interface IOptimizationReverse<T>
    {
        bool Reverse(in Reverse request, out NodeContainer<T> response);
    }

    /// <summary>
    /// Returns an NodeContainer&lt;T&gt; which from which CreateObjectViaFastEnumerator can be called
    /// on an aggregating object.
    /// </summary>
    readonly struct Reverse
    {
        public static bool Try<T, Node>(ref Node node, out NodeContainer<T> container)
            where Node : INode<T>
            => node.TryPushOptimization(new Reverse(), out container);
    }
}
