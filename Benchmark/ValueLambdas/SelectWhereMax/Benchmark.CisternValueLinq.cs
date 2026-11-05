using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.ValueLambdas
{
    partial class SelectWhereMax
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _ints
            .Select(x => x / 2)
            .Where(x => (x & 1) == 0)
            .Max();
    }
}
