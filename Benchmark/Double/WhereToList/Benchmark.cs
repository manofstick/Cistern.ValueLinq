using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    [MemoryDiagnoser]
    public partial class WhereToList
    {
        IEnumerable<double> _double;

        [Params(0, 1, 10, 100, 1000, 1000000)]
        public int Length { get; set; } = 0;

        [Params(ContainerTypes.Array, ContainerTypes.Enumerable, ContainerTypes.List)]
        public ContainerTypes ContainerType { get; set; } = ContainerTypes.Enumerable;

        [GlobalSetup]
        public void SetupData()
        {
            var data = Create(Length);

            _double = ContainerType switch
            {
                ContainerTypes.Enumerable => data,
                ContainerTypes.Array => data.ToArray(),
                ContainerTypes.List => data.ToList(),

                _ => throw new Exception("Unknown ContainerType")
            };
        }

        private static IEnumerable<double> Create(int size)
        {
            for (var i = 0; i < size; ++i)
                yield return (double)i;
        }

        internal static void SanityCheck()
        {
            var check = new WhereToList();

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
