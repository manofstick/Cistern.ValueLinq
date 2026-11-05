using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToDictionaryCount
    {
        [Benchmark]
        public int ZLinq() =>
            _data
            .AsValueEnumerable()
            .ToDictionary(x => x)
            .Count;
    }
}
