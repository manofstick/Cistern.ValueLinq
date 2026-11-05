using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AllPredicate
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            _data
            .All(x => x >= 0);
    }
}
