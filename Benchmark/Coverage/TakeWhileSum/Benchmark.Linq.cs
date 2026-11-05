using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class TakeWhileSum
    {
        [Benchmark]
        public int Linq() =>
            _data
            .TakeWhile(x => x < 990)
            .Sum();
    }
}
