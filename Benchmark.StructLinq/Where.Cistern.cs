using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Where
    {
        [Benchmark]
        public int CisternValueLinq() =>
            Enumerable.Range(0, Count)
            .Where(x => x > 0)
            .Sum();
    }
}
