using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class WhereToArray
    {
        [Benchmark]
        public int[] ZLinq() =>
            _data
            .AsValueEnumerable()
            .Where(x => x % 2 == 0)
            .ToArray();
    }
}
