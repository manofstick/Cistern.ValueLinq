using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyResultSelectorWhereSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _rows
            .SelectMany(row => row, (row, x) => x + row.Length)
            .Where(x => (x & 1) == 0)
            .Sum();
    }
}
