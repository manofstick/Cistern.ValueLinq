using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.DummyData
{
    [MemoryDiagnoser]
    public partial class WhereSelectOrderByToArray
    {
        List<DataSetA.Data> _data;

#if true
        [Params(0, 1, 5, 10, 20, 50, 100)]
#else
        [Params(100)]
#endif
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            _data =
                DataSetA
                .Get()
                .Take(Length)
                .ToList();
        }

        internal static void SanityCheck()
        {
            var check = new WhereSelectOrderByToArray();

            check.Length = 100;
            check.SetupData();

            var baseline = check.Linq();

            var cisternvaluelinq = check.CisternValueLinq();
            if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq)) throw new Exception();

#if CISTERNLINQx
            var cisternlinq = check.CisternLinq();
            if (!Enumerable.SequenceEqual(baseline, cisternlinq)) throw new Exception();
#endif

#if LINQAFx
            var linqaf = check.LinqAF();
            if (!Enumerable.SequenceEqual(baseline, linqaf)) throw new Exception();
#endif
        }
    }
}
