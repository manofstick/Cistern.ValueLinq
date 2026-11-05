using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereIndexSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Where((x, i) => i % 2 == 0)
            .Sum();
    }
}
