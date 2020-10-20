using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double.ToList
{
    [MemoryDiagnoser]
    public partial class Benchmark
    {
        IEnumerable<double> _double;

        [Params(0, 1, 10, 100, 1000, 1000000)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            var r = new Random(42);

            _double = Create(Length);
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
