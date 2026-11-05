using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectToList
    {
        [Benchmark]
        public List<int> CisternValueLinq() =>
            _data
            .Select(x => x * 2)
            .ToList();
    }
}
