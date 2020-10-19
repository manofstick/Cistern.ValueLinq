using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherBenchmark
{
    [MemoryDiagnoser]
    public partial class Benchmark
    {
        List<(double, double, double)> _doubledoubledoubles;
        List<double> _doubles;

        [Params(0, 1, 10, 100, 1000, 1000000)]
        //[Params(0)]
        public int Length { get; set; } = 0;

        //This is run before each iteration of a test
        [GlobalSetup]
        public void SetupData()
        {
            var r = new Random(42);

            _doubledoubledoubles =
                Enumerable
                .Range(0, Length)
                .Select(x => (r.NextDouble(), r.NextDouble(), r.NextDouble()))
                .ToList();

            _doubles =
                Enumerable
                .Range(0, Length)
                .Select(x => r.NextDouble())
                .ToList();
        }
    }
}
