using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
    partial class SelectSelectToList
    {
        [Benchmark]
        public List<int> CisternValueLinq() => _double.Select(x => (float)x).Select(x => (int)x).ToList();
    }
}
