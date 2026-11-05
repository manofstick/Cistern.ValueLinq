using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class MinInt
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Min();
    }
}
