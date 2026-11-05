using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ContainsWhere
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            array
            .Where(x => true)
            .Contains(5_000);

        [Benchmark]
        public bool CisternValueLinqEnumerable() =>
            enumerable
            .Where(x => true)
            .Contains(5_000);
    }
}
