using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ToArrayOnArraySelectOfClass
    {
        [Benchmark]
        public int[] CisternValueLinq() =>
            array
            .Select(x => x.Element)
            .ToArray();
    }
}
