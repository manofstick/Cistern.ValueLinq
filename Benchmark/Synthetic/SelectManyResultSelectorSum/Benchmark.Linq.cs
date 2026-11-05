using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyResultSelectorSum
    {
        [Benchmark]
        public int Linq() =>
            _rows
            .SelectMany(row => row, (row, x) => x + row.Length)
            .Sum();
    }
}
