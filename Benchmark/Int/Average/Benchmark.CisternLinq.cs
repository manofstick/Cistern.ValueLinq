using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.Int
{
#if CISTERNLINQ
    partial class Average
    {
        [Benchmark]
        public double CisternLinq() => _data.Average();
    }
#endif
}
