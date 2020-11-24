using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
    partial class ReverseAggregate
    {
        [Benchmark]
        public double CisternValueLinq() => _double.Reverse().Aggregate(0.0, (a, c) => (a*a)-(c*c));
    }
}
