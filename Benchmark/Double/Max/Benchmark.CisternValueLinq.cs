using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Double
{
    partial class Max
    {
        [Benchmark]
        public double CisternValueLinq() => _double.Max();
    }
}
