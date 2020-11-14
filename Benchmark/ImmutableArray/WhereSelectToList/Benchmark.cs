using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Cistern.Benchmarks.ImmutableArray
{
    [MemoryDiagnoser]
    public partial class WhereSelectToList
    {
        ImmutableArray<double> _double;

        [Params(0, 1, 10, 100, 1000, 1000000)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            var data = Create(Length);

            _double = data.ToImmutableArray();
        }

        private static IEnumerable<double> Create(int size)
        {
            for (var i = 0; i < size; ++i)
                yield return (double)i;
        }

        internal static void SanityCheck()
        {
            var check = new WhereSelectToList();

            check.Length = 0;
            check.SetupData();

            var baseline = check.Linq();
#if HYPERLINQ_tmp
            var hyperlinq = check.HyperLinq();
            if (!Enumerable.SequenceEqual(baseline, hyperlinq)) throw new Exception();
#endif

            var cisternvaluelinq = check.CisternValueLinq();
            if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq)) throw new Exception();

            var cisternvaluelinq_viaspan = check.CisternValueLinq_ViaSpan();
            if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq_viaspan)) throw new Exception();

            var cisternvaluelinq_viamemorysharedpool_pull = check.CisternValueLinq_ViaMemorySharedPool_Pull();
            if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq_viamemorysharedpool_pull)) throw new Exception();

            var cisternvaluelinq_viamemorysharedpool_push = check.CisternValueLinq_ViaMemorySharedPool_Push();
            if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq_viamemorysharedpool_push)) throw new Exception();

            var cisternvaluelinq_viamemorysharedpoolvaluelambda_pull = check.CisternValueLinq_ViaMemorySharedPoolValueLambda_Pull();
            if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq_viamemorysharedpoolvaluelambda_pull)) throw new Exception();

            var cisternvaluelinq_viamemorysharedpoolvaluelambda_push = check.CisternValueLinq_ViaMemorySharedPoolValueLambda_Push();
            if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq_viamemorysharedpoolvaluelambda_push)) throw new Exception();
        }
    }
}
