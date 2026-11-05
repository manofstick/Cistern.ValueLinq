using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AverageInt
    {
        [Benchmark]
        public double CisternValueLinq() =>
            _data
            .Average();
    }
}
