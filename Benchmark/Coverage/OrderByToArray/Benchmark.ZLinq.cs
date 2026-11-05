using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class OrderByToArray
    {
        [Benchmark]
        public int[] ZLinq() =>
            _data
            .AsValueEnumerable()
            .OrderBy(x => x)
            .ToArray();
    }
}
