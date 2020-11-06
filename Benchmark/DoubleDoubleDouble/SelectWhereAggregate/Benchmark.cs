using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.DoubleDoubleDouble
{
    [MemoryDiagnoser]
    public partial class SelectWhereAggregate
    {
        IEnumerable<(double, double, double)> _doubledoubledoubles;

        [Params(0, 1, 10, 100, 1000, 1000000)]
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
            var data = CreateData();

            _doubledoubledoubles = ContainerType switch
            {
                ContainerTypes.Enumerable => data,
                ContainerTypes.Array => data.ToArray(),
                ContainerTypes.List => data.ToList(),

                _ => throw new Exception("Unknown ContainerType")
            };
        }

        private IEnumerable<(double, double, double)> CreateData()
        {
            var r = new Random(42);
            for(var i=0; i < Length; ++i)
                yield return (r.NextDouble(), r.NextDouble(), r.NextDouble());
        }

        internal static void SanityCheck()
        {
            var check = new SelectWhereAggregate();

            check.Length = 100;
            check.SetupData();

            var baseline = check.Linq();
#if LINQAF
            var linqaf = check.LinqAF();
            if (baseline != linqaf) throw new Exception();
#endif

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

            var cisternvaluelinqbyref = check.CisternValueLinqByRef();
            if (baseline != cisternvaluelinqbyref) throw new Exception();

#if CISTERNLINQ
            var cisternlinq = check.CisternLinq();
            if (cisternlinq != baseline) throw new Exception();
#endif

            // check.HyperLinq(); // doesn't support Aggregate
        }
    }
}
