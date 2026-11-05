using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ConcatSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Concat(_other)
            .Sum();
    }
}
