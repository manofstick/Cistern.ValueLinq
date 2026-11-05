using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyEnumerableSum
    {
        [Benchmark]
        public int CisternValueLinq() =>
            _rows
            .SelectMany(row => row)
            .Sum();
    }
}
