using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ExceptCount
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Except(_other)
            .Count();
    }
}
