using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class GroupByCount
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .GroupBy(x => x % 100)
            .Count();
    }
}
