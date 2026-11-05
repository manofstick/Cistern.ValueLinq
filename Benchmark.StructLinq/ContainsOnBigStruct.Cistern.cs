using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ContainsOnBigStruct
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            array
            .Contains(StructContainer.Create(5_000));

        [Benchmark]
        public bool CisternValueLinqEnumerable() =>
            enumerable
            .Contains(StructContainer.Create(5_000));
    }
}
