using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class Contains
    {
        [Benchmark]
        public bool Linq() =>
            _data
            .Contains(-1);
    }
}
