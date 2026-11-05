using BenchmarkDotNet.Attributes;
using System.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectToList
    {
        [Benchmark]
        public List<int> Linq() =>
            _data
            .Select(x => x * 2)
            .ToList();
    }
}
