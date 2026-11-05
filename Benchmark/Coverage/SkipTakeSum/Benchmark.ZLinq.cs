using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SkipTakeSum
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .Skip(Length / 4)
            .Take(Length / 2)
            .Sum();
    }
}
