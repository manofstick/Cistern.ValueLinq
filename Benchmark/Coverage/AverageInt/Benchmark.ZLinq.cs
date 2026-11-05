using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AverageInt
    {
        [Benchmark]
        public double ZLinq() =>
            _data
            .AsValueEnumerable()
            .Average();
    }
}
