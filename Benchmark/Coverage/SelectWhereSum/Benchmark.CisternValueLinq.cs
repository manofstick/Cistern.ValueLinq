using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectWhereSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Select(x => x * 2)
            .Where(y => y % 3 == 0)
            .Sum();
    }
}
