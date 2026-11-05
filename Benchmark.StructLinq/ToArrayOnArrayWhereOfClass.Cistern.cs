using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ToArrayOnArrayWhereOfClass
    {
        [Benchmark]
        public Container[] CisternValueLinq() =>
            array
            .Where(x => (x.Element & 1) == 0)
            .ToArray();
    }
}
