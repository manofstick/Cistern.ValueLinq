using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
    partial class WhereToList
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.Where(x => x > 0).ToList();
    }
}
