using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToHashSetCount
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .ToHashSet()
            .Count;
    }
}
