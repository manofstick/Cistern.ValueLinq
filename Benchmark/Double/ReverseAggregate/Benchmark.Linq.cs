using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class ReverseAggregate
    {
        [Benchmark(Baseline = true)]
        public double Linq() => _double.Reverse().Aggregate(0.0, (a, c) => (a * a) - (c * c));
    }
}
