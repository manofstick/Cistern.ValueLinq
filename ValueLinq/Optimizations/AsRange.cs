namespace Cistern.ValueLinq.Optimizations
{
    [Optimization]
    internal interface IOptimizationAsRange
    {
        bool AsRange(in AsRange request, out (int Start, int Count) response);
    }

    /// <summary>
    /// The start and count of a range of integers, so an operator over one can compute its elements where it would
    /// otherwise pull them.
    /// </summary>
    readonly struct AsRange
    {
        public static bool TryGet<Node>(ref Node node, out (int Start, int Count) range)
            where Node : INode
            => node.TryPushOptimization(new AsRange(), out range);
    }
}
