using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class Contains
    {
        [Benchmark]
        public bool ZLinq() =>
            _data
            .AsValueEnumerable()
            .Contains(-1);
    }
}
