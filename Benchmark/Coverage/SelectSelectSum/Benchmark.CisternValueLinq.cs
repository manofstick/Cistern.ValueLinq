using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectSelectSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Select(x => x + 1)
            .Select(x => x * 2)
            .Sum();
    }
}
