using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class CountPredicate
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Count(x => x % 2 == 0);
    }
}
