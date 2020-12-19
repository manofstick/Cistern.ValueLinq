using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Int
{
    [MemoryDiagnoser]
    public partial class Sum
    {
        IEnumerable<int> _data;

#if true
        [Params(0, 1, 10, 100, 1000, 1000000)]
#else
        [Params(1000000)]
#endif
        public int Length { get; set; } = 0;

#if true
        [Params(ContainerTypes.Array, ContainerTypes.Enumerable, ContainerTypes.List)]
#else
        [Params(ContainerTypes.Enumerable)]
#endif
        public ContainerTypes ContainerType { get; set; } = ContainerTypes.Enumerable;

        [GlobalSetup]
        public void SetupData()
        {
            var data = Create(Length);

            _data = ContainerType switch
            {
                ContainerTypes.Enumerable => data,
                ContainerTypes.Array => data.ToArray(),
                ContainerTypes.List => data.ToList(),

                _ => throw new Exception("Unknown ContainerType")
            };
        }

        private static IEnumerable<int> Create(int size)
        {
            for (var i = 0; i < size; ++i)
                yield return i % 10000;
        }

        internal static void SanityCheck()
        {
            var check = new Sum();

            check.Length = 10000;
            check.ContainerType = ContainerTypes.Array;

            check.SetupData();

            var baseline = check.Linq();

            var baseline_foreach = check.Linq_Foreach();
            if (baseline != baseline_foreach) throw new Exception();

#if LINQAF
            var linqaf = check.LinqAF();
            if (baseline != linqaf) throw new Exception();
#endif

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

            var cisternvaluelinq_fastest = check.CisternValueLinq_SIMDOptions_Fastest();
            if (baseline != cisternvaluelinq_fastest) throw new Exception();

            var cisternvaluelinq_overflowhandling = check.CisternValueLinq_SIMDOptions_WithOverflowHandling();
            if (baseline != cisternvaluelinq_overflowhandling) throw new Exception();

            var cisternvaluelinq_foreach = check.CisternValueLinq_Foreach();
            if (baseline != cisternvaluelinq_foreach) throw new Exception();

#if CISTERNLINQ
            var cisternlinq = check.CisternLinq();
            if (cisternlinq != baseline) throw new Exception();
#endif
        }
    }
}
