using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class AnyPredicate
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            _data
            .Any(x => x < 0);
    }
}
