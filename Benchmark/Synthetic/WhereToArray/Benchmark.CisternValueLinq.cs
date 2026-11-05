using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class WhereToArray
    {
        [Benchmark]
        public int[] CisternValueLinq() =>
            _data
            .Where(x => x % 2 == 0)
            .ToArray();
    }
}
