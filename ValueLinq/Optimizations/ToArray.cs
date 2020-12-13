namespace Cistern.ValueLinq.Optimizations
{
    /// <summary>
    /// Returns a T[]
    /// </summary>
    struct ToArray
    {
        public static bool Try<T, Node>(ref Node node, out T[] array)
            where Node : INode<T>
            => node.TryPushOptimization(new ToArray(), out array);
    }
}
