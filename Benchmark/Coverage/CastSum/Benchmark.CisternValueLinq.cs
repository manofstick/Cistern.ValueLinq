using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class CastSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Cast<int>()
            .Sum();
    }
}
