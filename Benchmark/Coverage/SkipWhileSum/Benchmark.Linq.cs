using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SkipWhileSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .SkipWhile(x => x < 500)
            .Sum();
    }
}
