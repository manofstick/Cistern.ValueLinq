using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToArray
    {
        [Benchmark]
        public int[] ZLinq() =>
            _data
            .AsValueEnumerable()
            .ToArray();
    }
}
