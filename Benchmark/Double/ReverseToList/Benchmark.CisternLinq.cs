using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if CISTERNLINQx
    partial class ReverseToArray
    {
        [Benchmark]
        public double[] CisternLinq() => _double.Reverse().ToArray();
    }
#endif
}
