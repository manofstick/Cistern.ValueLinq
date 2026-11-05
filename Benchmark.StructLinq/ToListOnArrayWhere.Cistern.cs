using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ToListOnArrayWhere
    {
        [Benchmark]
        public List<int> CisternValueLinq() =>
            array
            .Where(x => (x & 1) == 0)
            .ToList();
    }
}
