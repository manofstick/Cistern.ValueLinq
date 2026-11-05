using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ContainsWhereOnBigStruct
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            array
            .Where(x => true)
            .Contains(StructContainer.Create(5_000));

        [Benchmark]
        public bool CisternValueLinqEnumerable() =>
            enumerable
            .Where(x => true)
            .Contains(StructContainer.Create(5_000));
    }
}
