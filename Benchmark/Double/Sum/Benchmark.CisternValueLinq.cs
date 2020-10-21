using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Double.Sum
{
    partial class Benchmark
    {
        [Benchmark]
        public double CisternValueLinq() => _double.Sum();
    }
}
