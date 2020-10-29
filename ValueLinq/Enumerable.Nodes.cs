namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        internal static int Count<T, Inner>(in Inner inner) where Inner : INode =>
            inner.CheckForOptimization<T, Optimizations.Count, int>(new Optimizations.Count(), out var count) switch
            {
                false => Nodes<int>.Aggregation<Inner, Aggregation.Count>(in inner),
                true => count
            };

    }
}
