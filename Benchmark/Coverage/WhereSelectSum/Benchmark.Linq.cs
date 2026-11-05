using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereSelectSum
    {
        [Benchmark]
        public long Linq() =>
            _data
            .Where(x => x % 3 == 0)
            .Select(x => (long)x * x)
            .Sum();
    }
}
