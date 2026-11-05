using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class DistinctCount
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Distinct()
            .Count();
    }
}
