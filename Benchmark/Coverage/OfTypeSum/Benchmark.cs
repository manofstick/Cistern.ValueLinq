using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    [MemoryDiagnoser]
    public partial class OfTypeSum
    {
        IEnumerable<object> _data;

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

        private static IEnumerable<object> Create(int size)
        {
            for (var i = 0; i < size; ++i)
                yield return i % 7 == 0 ? "text" : (object)((i * 31) % 1000);
        }

        internal static void SanityCheck()
        {
            var check = new OfTypeSum();

            check.Length = 1000;
            check.ContainerType = ContainerTypes.Array;
            check.SetupData();

            var baseline = check.Handcoded();

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

            var linq = check.Linq();
            if (baseline != linq) throw new Exception();

            var zlinq = check.ZLinq();
            if (baseline != zlinq) throw new Exception();
        }
    }
}
