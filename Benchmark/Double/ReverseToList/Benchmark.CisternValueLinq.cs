using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
    partial class ReverseToArray
    {
        [Benchmark]
        public double[] CisternValueLinq() => _double.Reverse().ToArray();
    }
}
