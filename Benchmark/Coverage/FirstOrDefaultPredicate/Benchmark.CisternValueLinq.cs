using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class FirstOrDefaultPredicate
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .FirstOrDefault(x => x < 0);
    }
}
