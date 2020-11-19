using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class Max
    {
        [Benchmark]
        public double CisternLinq() => _double.Max();
    }
#endif
}
