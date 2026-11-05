using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class LastOnArray
    {
        [Benchmark]
        public int CisternValueLinq() => array.Last();

        [Benchmark]
        public int CisternValueLinqEnumerable() => enumerable.Last();
    }
}
