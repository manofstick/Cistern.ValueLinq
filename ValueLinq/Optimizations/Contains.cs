namespace Cistern.ValueLinq.Optimizations
{
    [Optimization]
    internal interface IOptimizationContains<T>
    {
        bool Contains(in Contains<T> request, out bool response);
    }

    /// <summary>
    /// Whether the sequence holds the value under the default equality comparer, answered without running it through
    /// this node (a range's bounds, a repeated element compared once, an ordering's source asked in its place) and
    /// declined otherwise.
    /// </summary>
    readonly struct Contains<T>
    {
        public Contains(T value) => Value = value;

        public T Value { get; }
    }
}
