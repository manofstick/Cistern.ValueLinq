using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToDictionaryCount
    {
        [Benchmark]
        public int Linq() =>
            _data
            .ToDictionary(x => x)
            .Count;
    }
}
