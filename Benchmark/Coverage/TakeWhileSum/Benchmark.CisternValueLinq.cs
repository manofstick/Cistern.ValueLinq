using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class TakeWhileSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .TakeWhile(x => x < 990)
            .Sum();
    }
}
