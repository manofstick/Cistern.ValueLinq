using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class CastSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Cast<int>()
            .Sum();
    }
}
