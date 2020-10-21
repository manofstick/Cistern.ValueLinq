using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.SelectSelectToList
{
    partial class Benchmark
    {
        [Benchmark]
        public List<int> CisternValueLinq() => _double.Select(x => (float)x).Select(x => (int)x).ToList();
    }
}
