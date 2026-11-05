using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class ElementAt
    {
        [Benchmark]
        public int Linq() =>
            _data
            .ElementAt(Length / 2);
    }
}
