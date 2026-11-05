using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Contains
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            array
            .Contains(5_000);
    }
}
