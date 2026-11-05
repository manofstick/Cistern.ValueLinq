using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyEnumerableSum
    {
        [Benchmark]
        public int ZLinq() =>
            _rows
            .AsValueEnumerable()
            .SelectMany(row => row)
            .Sum();
    }
}
