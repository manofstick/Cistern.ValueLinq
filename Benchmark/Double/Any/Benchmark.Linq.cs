using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Double
{
    partial class Any
    {
        [Benchmark(Baseline = true)]
        public bool Linq() =>
            _filter == null
                ? _double.Any()
                : _double.Any(_filter);
    }
}
