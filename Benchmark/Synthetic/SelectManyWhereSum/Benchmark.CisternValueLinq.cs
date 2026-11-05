using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyWhereSum
    {
        [Benchmark]
        public long CisternValueLinq() =>
            _rows
            .SelectMany(row => row)
            .Where(x => x % 3 == 0)
            .Select(x => (long)x * x)
            .Sum();
    }
}
