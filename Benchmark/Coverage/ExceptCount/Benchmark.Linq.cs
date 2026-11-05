using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ExceptCount
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Except(_other)
            .Count();
    }
}
