using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyWhereSum
    {
        [Benchmark]
        public long Linq() =>
            _rows
            .SelectMany(row => row)
            .Where(x => x % 3 == 0)
            .Select(x => (long)x * x)
            .Sum();
    }
}
