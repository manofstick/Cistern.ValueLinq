using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Double.Any
{
    partial class Benchmark
    {
        [Benchmark]
        public bool CisternValueLinq() => _double.Any();
    }
}
