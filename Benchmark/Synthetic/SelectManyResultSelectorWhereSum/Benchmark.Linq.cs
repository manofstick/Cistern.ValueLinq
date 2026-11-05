using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyResultSelectorWhereSum
    {
        [Benchmark]
        public int Linq() =>
            _rows
            .SelectMany(row => row, (row, x) => x + row.Length)
            .Where(x => (x & 1) == 0)
            .Sum();
    }
}
