using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Double.Any
{
    partial class Benchmark
    {
        [Benchmark(Baseline = true)]
        public bool Linq() =>
            _filter == null
                ? _double.Any()
                : _double.Any(_filter);
    }
}
