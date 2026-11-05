using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ZipSum
    {
        [Benchmark]
        public long Linq() =>
            _data
            .Zip(_other)
            .Select(pair => (long)pair.First * pair.Second)
            .Sum();
    }
}
