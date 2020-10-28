using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Double.SelectManySum
{
    partial class Benchmark
    {
        [Benchmark]
        public double CisternValueLinq() => _double.SelectMany(x => _double).Sum();
    }
}
