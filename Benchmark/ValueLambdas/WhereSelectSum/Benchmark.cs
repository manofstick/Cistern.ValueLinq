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

#if true
        [Params(0, 1, 10, 100, 1000, 1000000)]
#else
        [Params(1, 100, 1000000)]
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

            var baseline = check.Linq();

            var flatout = check.Flatout();
            if (baseline != flatout) throw new Exception();

            //var baseline_foreach = check.Linq_Foreach();
            //if (baseline != baseline_foreach) throw new Exception();

//#if LINQAF
//            var linqaf = check.LinqAF();
//            if (baseline != linqaf) throw new Exception();

//            var linqaf_foreach = check.LinqAF_Foreach();
//            if (baseline != linqaf_foreach) throw new Exception();
//#endif

            var cisternvaluelinq = check.CisternValueLinq_normal();
            if (baseline != cisternvaluelinq) throw new Exception();

            var cisternvaluelinq2 = check.CisternValueLinq_struct();
            if (baseline != cisternvaluelinq2) throw new Exception();

            var cisternvaluelinq_foreach = check.CisternValueLinq_struct_nothing_up_my_sleve();
            if (baseline != cisternvaluelinq_foreach) throw new Exception();

//#if CISTERNLINQ
//            var cisternlinq = check.CisternLinq();
//            if (cisternlinq != baseline) throw new Exception();
//#endif

#if STRUCTLINQ
            var structlinq = check.Flatout_cast_to_array();
            if (structlinq != baseline) throw new Exception();

            //var structlinq_foreach = check.StructLinqLinq_Foreach();
            //if (structlinq_foreach != baseline) throw new Exception();
#endif

            // check.HyperLinq(); // doesn't support Aggregate
        }
    }
}
