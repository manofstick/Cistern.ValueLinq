using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
    partial class SkipReverseSkipToArray
    {
        [Benchmark]
        public double[] CisternValueLinq() => _double.Skip(Skip1).Reverse().Skip(Skip2).ToArray();
    }
}
