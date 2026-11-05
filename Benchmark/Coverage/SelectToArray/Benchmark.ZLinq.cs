using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectToArray
    {
        [Benchmark]
        public int[] ZLinq() =>
            _data
            .AsValueEnumerable()
            .Select(x => x * 2)
            .ToArray();
    }
}
