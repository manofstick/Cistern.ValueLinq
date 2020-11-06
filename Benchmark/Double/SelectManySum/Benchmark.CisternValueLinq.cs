using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Double
{
    partial class SelectManySum
    {
        [Benchmark]
        public double CisternValueLinq() => _double.SelectMany(x => _double).Sum();
    }
}
