using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyEnumerableSum
    {
        [Benchmark]
        public int Linq() =>
            _rows
            .SelectMany(row => row)
            .Sum();
    }
}
