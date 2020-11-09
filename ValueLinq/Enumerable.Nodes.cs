namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        internal static int Count<T, Inner>(in Inner inner, bool ignorePotentialSideEffects) where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);
            if (!countInfo.IsStale && (ignorePotentialSideEffects || !countInfo.PotentialSideEffects) && countInfo.ActualLengthIsMaximumLength && countInfo.MaximumLength.HasValue && countInfo.MaximumLength.Value <= int.MaxValue)
                return (int)countInfo.MaximumLength.Value;

            return inner.CheckForOptimization<T, Optimizations.Count, int>(new Optimizations.Count { IgnorePotentialSideEffects = ignorePotentialSideEffects }, out var count) switch
            {
                false => inner.CreateObjectViaFastEnumerator<int, Aggregation.Count<T>>(new Aggregation.Count<T>()),
                true => count
            };
        }
    }
}
