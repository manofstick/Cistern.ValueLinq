using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Where(x => x % 3 == 0)
            .Sum();
    }
}
