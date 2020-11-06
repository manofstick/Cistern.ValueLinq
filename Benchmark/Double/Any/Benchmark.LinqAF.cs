using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class Any
    {
        [Benchmark]
        public bool LinqAF() =>
            _filter == null
                ? _double.Any()
                : _double.Any(_filter);
    }
#endif
}
