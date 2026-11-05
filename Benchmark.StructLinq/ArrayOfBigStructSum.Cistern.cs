using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ArrayOfBigStructSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array
            .Sum(x => x.Element);
    }
}
