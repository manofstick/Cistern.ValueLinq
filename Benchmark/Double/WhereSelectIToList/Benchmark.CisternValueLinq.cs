using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.WhereSelectIToList
{
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.Where(x => x > 0).Select((x, i) => x + i).ToList();
    }
}
