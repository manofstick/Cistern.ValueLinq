using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectIndexSum
    {
        [Benchmark]
        public long CisternValueLinq() =>
            _data
            .Select((x, i) => (long)x * i)
            .Sum();
    }
}
