namespace Cistern.ValueLinq.Optimizations
{
    [Optimization]
    internal interface IOptimizationToArray<T>
    {
        bool ToArray(in ToArray request, [MaybeNullWhen(false)] out T[] response);
    }

    /// <summary>
    /// Returns a T[]
    /// </summary>
    readonly struct ToArray
    {
        public static bool Try<T, Node>(ref Node node, [MaybeNullWhen(false)] out T[] array)
            where Node : INode<T>
            => node.TryPushOptimization(new ToArray(), out array);
    }
}
