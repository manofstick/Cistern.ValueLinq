using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.DoubleDoubleDouble.SelectWhereAggregate
{
    [MemoryDiagnoser]
    public partial class Benchmark
    {
        List<(double, double, double)> _doubledoubledoubles;

        [Params(0, 1, 10, 100, 1000, 1000000)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            var r = new Random(42);

            _doubledoubledoubles =
                Enumerable
                .Range(0, Length)
                .Select(x => (r.NextDouble(), r.NextDouble(), r.NextDouble()))
                .ToList();
        }

        internal static void Validate()
        {
            var check = new Benchmark();

            check.Length = 100;
            check.SetupData();

            var a = check.Linq();
            var b = check.LinqAF();
            var c = check.CisternValueLinq();
            var d = check.CisternValueLinqByRef();
            var e = check.CisternLinq();
            // check.HyperLinq(); // doesn't support Aggregate

            if (a != b || b != c || c != d)
                throw new Exception($"({a} != {b} || {b} != {c} || {c} != {d})");
        }
    }
}
