using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectToArray
    {
        [Benchmark]
        public int[] CisternValueLinq() =>
            _data
            .Select(x => x * 2)
            .ToArray();
    }
}
