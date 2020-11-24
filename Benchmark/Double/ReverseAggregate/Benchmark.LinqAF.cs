using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class ReverseAggregate
    {
        [Benchmark]
        public double LinqAF() =>  _double.Reverse().Aggregate(0.0, (a, c) => (a*a)-(c*c));
    }
#endif
}
