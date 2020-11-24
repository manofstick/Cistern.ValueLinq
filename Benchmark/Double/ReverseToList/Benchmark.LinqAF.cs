using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double
{
#if LINQAFx
    partial class ReverseToArray
    {
        [Benchmark]
        public double[] LinqAF() => _double.Reverse().ToArray();
    }
#endif
}
