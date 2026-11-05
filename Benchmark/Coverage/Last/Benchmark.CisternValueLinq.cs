using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class Last
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _data
            .Last();
    }
}
