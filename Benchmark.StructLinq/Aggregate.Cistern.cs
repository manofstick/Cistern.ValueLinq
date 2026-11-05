using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Aggregate
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _enumerable
            .Aggregate(0, (accu, elt) => accu + elt);
    }
}
