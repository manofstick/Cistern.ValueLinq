using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class WhereToList
    {
        [Benchmark]
        public List<int> ZLinq() =>
            _data
            .AsValueEnumerable()
            .Where(x => x % 2 == 0)
            .ToList();
    }
}
