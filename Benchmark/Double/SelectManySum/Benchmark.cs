﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double.SelectManySum
{
    [MemoryDiagnoser]
    public partial class Benchmark
    {
        IEnumerable<double> _double;

#if true
        [Params(0, 1, 10, 100, 1000)]
#else
        [Params(0, 1, 10, 100)]
#endif
        public int Length { get; set; } = 0;

#if true
        [Params(ContainerTypes.Array, ContainerTypes.Enumerable, ContainerTypes.List)]
#else
        [Params(ContainerTypes.Array)]
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
            var check = new Benchmark();

            check.Length = 100;
            check.ContainerType = ContainerTypes.Array;
            check.SetupData();

            var baseline = check.Linq();
#if LINQAF
            var linqaf = check.LinqAF();
            if (baseline != linqaf) throw new Exception();
#endif

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

#if CISTERNLINQ
            var cisternlinq = check.CisternLinq();
            if (cisternlinq != baseline) throw new Exception();
#endif

            // check.HyperLinq(); // doesn't support Aggregate
        }
    }
}