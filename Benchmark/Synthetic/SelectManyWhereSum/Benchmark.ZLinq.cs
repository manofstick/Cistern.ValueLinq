using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyWhereSum
    {
        [Benchmark]
        public long ZLinq() =>
            _rows
            .AsValueEnumerable()
            .SelectMany(row => row)
            .Where(x => x % 3 == 0)
            .Select(x => (long)x * x)
            .Sum();
    }
}
