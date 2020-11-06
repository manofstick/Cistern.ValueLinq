using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
    partial class WhereWhereToList
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.Where(x => ((int)x % 11) != 0).Where(x => ((int)x % 13) != 0).ToList();
    }
}
