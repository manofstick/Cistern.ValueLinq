using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ArrayOfClassSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array
            .Select(x => x.Element)
            .Sum();
    }
}
