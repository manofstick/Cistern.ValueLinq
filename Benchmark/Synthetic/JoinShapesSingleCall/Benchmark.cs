using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using static Cistern.Benchmarks.Synthetic.JoinShapes;

namespace Cistern.Benchmarks.Synthetic
{
    // one join per measurement on a heap collected beforehand, as a program calling it once would see it; in a loop the
    // lookup's groupings survive into later generations, which a single call may not pay. Run with --invocationCount 1
    // --unrollFactor 1, which the iteration setup requires
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class JoinShapesSingleCall
    {
        int[] _fewOuter;
        Small[] _fewInner;
        int[] _distinctOuter;
        Small[] _distinctInner;

        public JoinShapesSingleCall()
        {
            _fewOuter = Keys(5, 10);
            _fewInner = FewKeys(100_000, 10);
            _distinctOuter = Keys(0, 10_000);
            _distinctInner = DistinctKeys(10_000, 2_500);
        }

        [IterationSetup]
        public void Collect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        internal static void SanityCheck()
        {
            var check = new JoinShapesSingleCall();

            Same(100_005, check.Handcoded_FullJoinFewKeys(), check.CisternValueLinq_FullJoinFewKeys());
            Same(12_500, check.Handcoded_FullJoinDistinct(), check.CisternValueLinq_FullJoinDistinct());
        }
    }
}
