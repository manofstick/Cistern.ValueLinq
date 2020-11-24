using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQ
    partial class ReverseAggregate
    {
        [Benchmark]
        public double CisternLinq() => _double.Reverse().Aggregate(0.0, (a, c) => (a*a)-(c*c));
    }
#endif
}
