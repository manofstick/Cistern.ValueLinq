using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SkipTakeSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Skip(Length / 4)
            .Take(Length / 2)
            .Sum();
    }
}
