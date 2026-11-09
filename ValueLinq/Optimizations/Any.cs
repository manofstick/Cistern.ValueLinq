namespace Cistern.ValueLinq.Optimizations
{
    [PushOptimization]
    internal interface IOptimizationAny
    {
        bool Any(in Any request, out bool response);
    }

    /// <summary>
    /// Whether the sequence has an element, answered without running it through this node (an ordering asks its source,
    /// sorting nothing) and declined otherwise.
    /// </summary>
    readonly struct Any
    {
    }
}
