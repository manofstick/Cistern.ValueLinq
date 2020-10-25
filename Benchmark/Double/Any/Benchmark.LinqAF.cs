using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double.Any
{
#if LINQAF
    partial class Benchmark
    {
        [Benchmark]
        public bool LinqAF() =>
            _filter == null
                ? _double.Any()
                : _double.Any(_filter);
    }
#endif
}
