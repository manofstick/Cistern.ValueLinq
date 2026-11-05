using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ReverseToArray
    {
        [Benchmark]
        public int[] CisternValueLinq() =>
            _data
            .Reverse()
            .ToArray();
    }
}
