using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.ImmutableArray
{
    partial class SkipToArray
    {
        [Benchmark(Baseline = true)]
        public double[] Linq() => _double.Skip(Length/2).ToArray();
    }
}
