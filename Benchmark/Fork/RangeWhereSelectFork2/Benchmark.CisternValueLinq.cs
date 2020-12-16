using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Fork
{
    partial class RangeSelectWhereFork2
    {
        [Benchmark]
        public (long, double) CisternValueLinq()
        {
            var last = 
                Enumerable
                .Range(0, Length)
                .Select(x => (long)x)
                .Where(x => (x & 1) == 0)
                .Last();

            var average =
                Enumerable
                .Range(0, Length)
                .Select(x => (long)x)
                .Where(x => (x & 1) == 0)
                .Average();

            return (last, average);
        }

        [Benchmark]
        public (long, double) CisternValueLinq_Fork() =>
            Enumerable
            .Range(0, Length)
            .Select(x => (long)x)
            .Where(x => (x & 1) == 0)
            .Fork(
                x => x.Last(),
                x => x.Average())
            ;

        struct IntToLong : IFunc<int, long> { public long Invoke(int t) => (long)t; }
        struct FilterEvenInts : IFunc<long, bool> { public bool Invoke(long t) => (t & 1) == 0; }
        [Benchmark]
        public (long, double) CisternValueLinq_ValueLambda_Fork() =>
            Enumerable
            .Range(0, Length)
            .Select(new IntToLong(), default(long))
            .Where(new FilterEvenInts())
            .Fork(
                x => x.Last(),
                x => x.Average());

        [Benchmark]
        public (long, double) CisternValueLinq_Reify()
        {
            var data =
                Enumerable
                .Range(0, Length)
                .Select(x => (long)x)
                .Where(x => (x & 1) == 0)
                .ToArray();

            return (data.Last(), data.Average());
        }
    }
}
