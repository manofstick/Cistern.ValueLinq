using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class OrderByToArray
    {
        [Benchmark]
        public int[] CisternValueLinq() =>
            _data
            .OrderBy(x => x)
            .ToArray();
    }
}
