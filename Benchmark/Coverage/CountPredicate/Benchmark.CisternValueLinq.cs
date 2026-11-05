using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class CountPredicate
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Count(x => x % 2 == 0);
    }
}
