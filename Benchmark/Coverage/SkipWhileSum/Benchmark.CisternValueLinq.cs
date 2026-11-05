using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SkipWhileSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .SkipWhile(x => x < 500)
            .Sum();
    }
}
