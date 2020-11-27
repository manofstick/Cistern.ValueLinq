using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQx
    partial class SkipReverseSkipToArray
    {
        [Benchmark]
        public double[] CisternLinq() => _double.Skip(Skip1).Reverse().Skip(Skip2).ToArray();
    }
#endif
}
