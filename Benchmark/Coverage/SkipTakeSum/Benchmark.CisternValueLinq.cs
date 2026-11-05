using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SkipTakeSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Skip(Length / 4)
            .Take(Length / 2)
            .Sum();
    }
}
