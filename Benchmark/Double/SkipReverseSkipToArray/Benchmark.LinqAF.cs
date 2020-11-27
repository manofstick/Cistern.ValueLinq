using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double
{
#if LINQAFx
    partial class SkipReverseSkipToArray
    {
        [Benchmark]
        public double[] LinqAF() => _double.Skip(Skip1).Reverse().Skip(Skip2).ToArray();
    }
#endif
}
