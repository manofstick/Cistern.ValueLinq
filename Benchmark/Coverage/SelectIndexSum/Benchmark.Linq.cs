using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectIndexSum
    {
        [Benchmark]
        public long Linq() =>
            _data
            .Select((x, i) => (long)x * i)
            .Sum();
    }
}
