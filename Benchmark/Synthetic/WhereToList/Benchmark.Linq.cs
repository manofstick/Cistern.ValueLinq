using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class WhereToList
    {
        [Benchmark]
        public List<int> Linq() =>
            _data
            .Where(x => x % 2 == 0)
            .ToList();
    }
}
