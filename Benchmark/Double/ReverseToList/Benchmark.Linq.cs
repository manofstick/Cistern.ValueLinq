using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class ReverseToArray
    {
        [Benchmark(Baseline = true)]
        public double[] Linq() => _double.Reverse().ToArray();
    }
}
