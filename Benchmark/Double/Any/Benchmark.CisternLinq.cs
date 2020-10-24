using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double.Any
{
#if CISTERNLINQ
    partial class Benchmark
    {
        [Benchmark]
        public bool CisternLinq() => _double.Any();
    }
#endif
}
