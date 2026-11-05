using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class VisitorOnArray
    {
        [Benchmark]
        public int CisternValueLinq() =>
            array.Sum(x => x);

        [Benchmark]
        public int CisternValueLinqVisitor() =>
            array.Sum();

        [Benchmark]
        public int CisternValueLinqVisitorOnTake() =>
            array
            .Take(900)
            .Sum();

        [Benchmark]
        public int CisternValueLinqVisitorOnSkip() =>
            array
            .Skip(100)
            .Sum();

        [Benchmark]
        public int CisternValueLinqVisitorOnSkipAndTake() =>
            array
            .Take(900)
            .Skip(100)
            .Sum();
    }
}
