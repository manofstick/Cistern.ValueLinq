
namespace Cistern.ValueLinq.Optimizations
{
    [Optimization]
    internal interface IOptimizationCount
    {
        bool Count(in Count request, out int response);
    }

    /// <summary>
    /// Returns an int that is the count of the collection. As a dry run it is answered only where nothing is enumerated
    /// and no user code runs (a collection's Count is read), and declined otherwise, as TryGetNonEnumeratedCount asks it.
    /// </summary>
    internal struct Count
    {
        public bool IgnorePotentialSideEffects;
        public bool DryRun;
    }
}
