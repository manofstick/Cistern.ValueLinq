using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.ToList
{
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.ToList();
    }
}
