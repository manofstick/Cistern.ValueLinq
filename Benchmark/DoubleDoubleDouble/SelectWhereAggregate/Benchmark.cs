using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.DoubleDoubleDouble.SelectWhereAggregate
{
    [MemoryDiagnoser]
    public partial class Benchmark
    {
        List<(double, double, double)> _doubledoubledoubles;

        [Params(0, 1, 10, 100, 1000, 1000000)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            var r = new Random(42);

            _doubledoubledoubles =
                Enumerable
                .Range(0, Length)
                .Select(x => (r.NextDouble(), r.NextDouble(), r.NextDouble()))
                .ToList();
        }

        internal static void SanityCheck()
        {
            var check = new Benchmark();

            check.Length = 100;
            check.SetupData();

            var baseline = check.Linq();
#if LINQAF
            var linqaf = check.LinqAF();
            if (baseline != linqaf) throw new Exception();
#endif

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

            var cisternvaluelinqbyref = check.CisternValueLinqByRef();
            if (baseline != cisternvaluelinqbyref) throw new Exception();

#if CISTERNLINQ
            var cisternlinq = check.CisternLinq();
            if (cisternlinq != baseline) throw new Exception();
#endif

            // check.HyperLinq(); // doesn't support Aggregate
        }
    }
}
