using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double.SelectToList
{
    [MemoryDiagnoser]
    public partial class Benchmark
    {
        IEnumerable<double> _double;

        [Params(0, 1, 10, 100, 1000, 1000000)]
        public int Length { get; set; } = 0;

        [Params(ContainerTypes.Array, ContainerTypes.Enumerable, ContainerTypes.List)]
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

        internal static void Validate()
        {
            var check = new Benchmark();

            check.Length = 100;
            check.SetupData();

            var a = check.Linq();
            var b = check.LinqAF();
            var c = check.CisternValueLinq();
            var d = check.CisternLinq();

            if (!Enumerable.SequenceEqual(a, b) || !Enumerable.SequenceEqual(b, c) || !Enumerable.SequenceEqual(c, d))
                throw new Exception($"({a} != {b} || {b} != {c} || {c} != {d})");
        }
    }
}
