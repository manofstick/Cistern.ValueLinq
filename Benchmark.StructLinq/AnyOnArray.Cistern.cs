using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class AnyOnArray
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            array
            .Any(x => x >= Count / 2);
    }
}
