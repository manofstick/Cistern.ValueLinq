using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class OfTypeSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .OfType<int>()
            .Sum();
    }
}
