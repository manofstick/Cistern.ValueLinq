using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.ValueLambdas
{
    partial class SelectWhereMax
    {
        [Benchmark]
        public int Linq() =>
            _ints
            .Select(x => x / 2)
            .Where(x => (x & 1) == 0)
            .Max();
    }
}
