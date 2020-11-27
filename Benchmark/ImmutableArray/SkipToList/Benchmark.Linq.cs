using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.ImmutableArray
{
    partial class SkipToList
    {
        [Benchmark(Baseline = true)]
        public List<double> Linq() => _double.Skip(Length/2).ToList();
    }
}
