using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ZipSum
    {
        [Benchmark]
        public long CisternValueLinq() =>
            _data
            .Zip(_other)
            .Select(pair => (long)pair.First * pair.Second)
            .Sum();
    }
}
