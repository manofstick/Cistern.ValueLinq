using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToDictionaryCount
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .ToDictionary(x => x)
            .Count;
    }
}
