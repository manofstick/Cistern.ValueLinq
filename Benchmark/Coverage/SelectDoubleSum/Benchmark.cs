using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    [MemoryDiagnoser]
    public partial class SelectDoubleSum
    {
        IEnumerable<int> _data;

        [ParamsSource(typeof(Lengths), nameof(Lengths.Values))]
        public int Length { get; set; } = 0;

        [Params(ContainerTypes.Array, ContainerTypes.Enumerable, ContainerTypes.List)]
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
                yield return (i * 31) % 1000;
        }

        internal static void SanityCheck()
        {
            var check = new SelectDoubleSum();

            check.Length = 1000;
            check.ContainerType = ContainerTypes.Array;
            check.SetupData();

            var baseline = check.Handcoded();

            var cisternvaluelinq = check.CisternValueLinq();
            if (Math.Abs(baseline - cisternvaluelinq) > 1e-9 * Math.Abs(baseline)) throw new Exception();

            var linq = check.Linq();
            if (Math.Abs(baseline - linq) > 1e-9 * Math.Abs(baseline)) throw new Exception();

            var zlinq = check.ZLinq();
            if (Math.Abs(baseline - zlinq) > 1e-9 * Math.Abs(baseline)) throw new Exception();
        }
    }
}
