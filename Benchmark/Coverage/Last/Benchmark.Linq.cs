using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class Last
    {
        [Benchmark]
        public int Linq() =>
            _data
            .Last();
    }
}
