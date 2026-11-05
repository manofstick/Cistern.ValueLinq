using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AllPredicate
    {
        [Benchmark]
        public bool Linq() =>
            _data
            .All(x => x >= 0);
    }
}
