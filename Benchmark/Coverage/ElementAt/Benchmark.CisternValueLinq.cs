using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ElementAt
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .ElementAt(Length / 2);
    }
}
