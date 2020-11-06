using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    [MemoryDiagnoser]
    public partial class ToList
    {
        IEnumerable<double> _double;

        [Params(0, 1, 10, 100, 1000, 1000000)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            var r = new Random(42);

            _double = Create(Length);
        }

        private static IEnumerable<double> Create(int size)
        {
            for (var i = 0; i < size; ++i)
                yield return (double)i;
        }

        internal static void SanityCheck()
        {
            var check = new ToList();

            check.Length = 100;
            check.SetupData();

            var baseline = check.Linq();
#if LINQAF
            var linqaf = check.LinqAF();
            if (!Enumerable.SequenceEqual(baseline, linqaf)) throw new Exception();
#endif

            var cisternvaluelinq = check.CisternValueLinq();
            if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq)) throw new Exception();

#if CISTERNLINQ
            var cisternlinq = check.CisternLinq();
            if (!Enumerable.SequenceEqual(cisternlinq, baseline)) throw new Exception();
#endif

            // check.HyperLinq(); // doesn't support Aggregate
        }
    }
}
