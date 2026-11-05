using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class FirstOrDefaultPredicate
    {
        [Benchmark]
        public int Linq() =>
            _data
            .FirstOrDefault(x => x < 0);
    }
}
