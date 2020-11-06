using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
    partial class WhereSelectToList
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.Where(x => x > 0).Select(x => x + 1).ToList();
    }
}
