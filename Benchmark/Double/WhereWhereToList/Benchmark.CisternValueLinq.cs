using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.WhereWhereToList
{
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.Where(x => ((int)x % 11) != 0).Where(x => ((int)x % 13) != 0).ToList();
    }
}
