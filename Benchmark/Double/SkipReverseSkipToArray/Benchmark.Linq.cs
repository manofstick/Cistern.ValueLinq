using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class SkipReverseSkipToArray
    {
        [Benchmark(Baseline = true)]
        public double[] Linq() => _double.Skip(Skip1).Reverse().Skip(Skip2).ToArray();
    }
}
