using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToArray
    {
        [Benchmark]
        public int[] CisternValueLinq() =>
            _data
            .ToArray();
    }
}
