using BenchmarkDotNet.Attributes;
using ZLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectToList
    {
        [Benchmark]
        public List<int> ZLinq() =>
            _data
            .AsValueEnumerable()
            .Select(x => x * 2)
            .ToList();
    }
}
