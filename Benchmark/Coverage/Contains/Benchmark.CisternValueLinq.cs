using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class Contains
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            _data
            .Contains(-1);
    }
}
