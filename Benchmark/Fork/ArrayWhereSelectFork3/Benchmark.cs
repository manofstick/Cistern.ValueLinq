using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;

namespace Cistern.Benchmarks.Fork
{
    [MemoryDiagnoser]
    public partial class ArrayWhereSelectFork3
    {
        [Params(1, 100, 10000, 1000000)]
        public int Length { get; set; } = 0;

        private int[] Data;

        [GlobalSetup]
        public void SetupData()
        {
            Data =
                Enumerable
                .Range(0, Length)
                .ToArray();
        }

        internal static void SanityCheck()
        {
            var check = new ArrayWhereSelectFork3();

            check.Length = 100;
            check.SetupData();

            var baseline = check.Handcoded();

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

            var cisternvaluelinq_reify = check.CisternValueLinq_Reify();
            if (baseline != cisternvaluelinq_reify) throw new Exception();

            var cisternvaluelinq_fork = check.CisternValueLinq_Fork();
            if (baseline != cisternvaluelinq_fork) throw new Exception();

            var cisternvaluelinq_valuelambda = check.CisternValueLinq_ValueLambda_Fork();
            if (baseline != cisternvaluelinq_valuelambda) throw new Exception();

            var linq = check.Linq();
            if (linq != baseline) throw new Exception();

            var linq_reify = check.Linq_Reify();
            if (linq_reify != baseline) throw new Exception();
        }
    }
}
