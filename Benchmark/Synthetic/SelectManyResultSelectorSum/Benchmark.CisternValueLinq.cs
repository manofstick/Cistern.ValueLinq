using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyResultSelectorSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _rows
            .SelectMany(row => row, (row, x) => x + row.Length)
            .Sum();
    }
}
