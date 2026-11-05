using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectDoubleSum
    {
        [Benchmark]
        public double CisternValueLinq() =>
            _data
            .Select(x => x * 0.5)
            .Sum();
    }
}
