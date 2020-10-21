using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double.Sum
{
    partial class Benchmark
    {
        [Benchmark]
        public double LinqAF() => _double.Sum();
    }
}
