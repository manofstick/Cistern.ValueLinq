using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class Any
    {
        [Benchmark]
        public bool CisternLinq() =>
            _filter == null
                ? _double.Any()
                : _double.Any(_filter);
    }
#endif
}
