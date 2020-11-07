namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        internal static int Count<T, Inner>(in Inner inner) where Inner : INode
        {
            inner.GetCountInformation(out var countInfo);
            if (!countInfo.IsStale && !countInfo.PotentialSideEffects && countInfo.ActualLengthIsMaximumLength && countInfo.MaximumLength.HasValue && countInfo.MaximumLength.Value <= int.MaxValue)
                return (int)countInfo.MaximumLength.Value;

            return inner.CheckForOptimization<T, Optimizations.Count, int>(new Optimizations.Count(), out var count) switch
            {
                false => inner.CreateObjectViaFastEnumerator<T, int, Aggregation.Count<T>>(new Aggregation.Count<T>()),
                true => count
            };
        }
    }
}
