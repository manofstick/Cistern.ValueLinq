using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ReverseToArray
    {
        [Benchmark]
        public int[] ZLinq() =>
            _data
            .AsValueEnumerable()
            .Reverse()
            .ToArray();
    }
}
