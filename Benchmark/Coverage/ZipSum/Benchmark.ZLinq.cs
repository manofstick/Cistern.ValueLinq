using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ZipSum
    {
        [Benchmark]
        public long ZLinq() =>
            _data
            .AsValueEnumerable()
            .Zip(_other.AsValueEnumerable())
            .Select(pair => (long)pair.First * pair.Second)
            .Sum();
    }
}
