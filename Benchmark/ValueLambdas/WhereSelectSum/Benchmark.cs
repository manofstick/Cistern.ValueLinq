using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.ValueLambdas.WhereSelectSum
{
    [MemoryDiagnoser]
    public partial class Benchmark
    {
        IEnumerable<int> _ints;

#if trueX
        [Params(0, 1, 10, 100, 1000, 1000000)]
#else
        [Params(1, 100, 1000000)]
#endif
        public int Length { get; set; } = 0;

#if trueX
        [Params(ContainerTypes.Array, ContainerTypes.Enumerable, ContainerTypes.List)]
#else
        [Params(ContainerTypes.Array)]
#endif
        public ContainerTypes ContainerType { get; set; } = ContainerTypes.Enumerable;

        [GlobalSetup]
        public void SetupData()
        {
            var data = Create(Length);

            _ints = ContainerType switch
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
                yield return i % 1000;
        }

        internal static void SanityCheck()
        {
            var check = new Benchmark();

            check.Length = 100;
            check.ContainerType = ContainerTypes.Array;
            check.SetupData();

            var baseline = check.Handcoded();

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

#if STRUCTLINQ
            var structlinq = check.StructLinq();
            if (structlinq != baseline) throw new Exception();
#endif
        }
    }
}
