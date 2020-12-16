using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;

namespace Cistern.Benchmarks.Fork
{
    [MemoryDiagnoser]
    public partial class ArrayFork3Optimized
    {
        [Params(1, 100, 10000, 1000000)]
        public int Length { get; set; } = 0;

        private int[] Data;

        [GlobalSetup]
        public void SetupData()
        {
            var r = new Random();
            Data =
                Enumerable
                .Range(0, Length)
                .Select(x => r.Next())
                .ToArray();
        }

        internal static void SanityCheck()
        {
            var check = new ArrayFork3Optimized();

            check.Length = 100;
            check.SetupData();

            var baseline = check.Handcoded();

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

            var cisternvaluelinq_fork = check.CisternValueLinq_Fork();
            if (baseline != cisternvaluelinq_fork) throw new Exception();

            var linq = check.Linq();
            if (linq != baseline) throw new Exception();
        }
    }
}
