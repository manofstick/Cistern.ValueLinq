using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ConcatSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Concat(_other)
            .Sum();
    }
}
