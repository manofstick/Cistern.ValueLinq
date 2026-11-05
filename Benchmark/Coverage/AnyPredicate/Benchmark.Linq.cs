using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AnyPredicate
    {
        [Benchmark]
        public bool Linq() =>
            _data
            .Any(x => x < 0);
    }
}
