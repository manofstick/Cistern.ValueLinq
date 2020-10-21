using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.SelectWhereToList
{
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.Select(x => x + 1).Where(x => x > 20).ToList();
    }
}
