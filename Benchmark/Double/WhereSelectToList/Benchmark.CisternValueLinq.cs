using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.WhereSelectToList
{
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.Where(x => x > 0).Select(x => x + 1).ToList();
    }
}
