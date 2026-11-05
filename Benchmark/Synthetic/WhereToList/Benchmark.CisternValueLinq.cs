using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    partial class WhereToList
    {
        [Benchmark]
        public List<int> CisternValueLinq() =>
            _data
            .Where(x => x % 2 == 0)
            .ToList();
    }
}
