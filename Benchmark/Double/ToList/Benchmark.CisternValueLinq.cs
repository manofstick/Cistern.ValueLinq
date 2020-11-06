using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
    partial class ToList
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.ToList();
    }
}
