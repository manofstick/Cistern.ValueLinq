using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    [MemoryDiagnoser]
    public partial class ReverseToArray
    {
        IEnumerable<double> _double;

#if true
        [Params(0, 1, 10, 100, 1000, 1000000)]
#else
        [Params(1)]
#endif
        public int Length { get; set; } = 0;

#if trueX
        [Params(ContainerTypes.Array, ContainerTypes.Enumerable, ContainerTypes.List)]
#else
        [Params(ContainerTypes.Enumerable)]
#endif
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
            var check = new SelectToList();

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
