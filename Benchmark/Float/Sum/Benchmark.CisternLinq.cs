using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Float
{
#if CISTERNLINQ
    partial class Sum
    {
        [Benchmark]
        public double CisternLinq() => _data.Sum();
    }
#endif
}
