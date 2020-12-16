using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Fork
{
    partial class RangeSelectWhereFork3
    {
        [Benchmark]
        public (long, int, double) CisternValueLinq()
        {
            var last = 
                Enumerable
                .Range(0, Length)
                .Select(x => (long)x)
                .Where(x => (x & 1) == 0)
                .Last();

            var count =
                Enumerable
                .Range(0, Length)
                .Select(x => (long)x)
                .Where(x => (x & 1) == 0)
                .Count();

            var average =
                Enumerable
                .Range(0, Length)
                .Select(x => (long)x)
                .Where(x => (x & 1) == 0)
                .Average();

            return (last, count, average);
        }

        [Benchmark]
        public (long, int, double) CisternValueLinq_Fork() =>
            Enumerable
            .Range(0, Length)
            .Select(x => (long)x)
            .Where(x => (x & 1) == 0)
            .Fork(
                x => x.Last(),
                x => x.Count(),
                x => x.Average())
            ;

        struct IntToLong : IFunc<int, long> { public long Invoke(int t) => (long)t; }
        struct FilterEvenInts : IFunc<long, bool> { public bool Invoke(long t) => (t & 1) == 0; }
        [Benchmark]
        public (long, int, double) CisternValueLinq_ValueLambda_Fork() =>
            Enumerable
            .Range(0, Length)
            .Select(new IntToLong(), default(long))
            .Where(new FilterEvenInts())
            .Fork(
                x => x.Last(),
                x => x.Count(),
                x => x.Average());

        [Benchmark]
        public (long, int, double) CisternValueLinq_Reify()
        {
            var data =
                Enumerable
                .Range(0, Length)
                .Select(x => (long)x)
                .Where(x => (x & 1) == 0)
                .ToArray();

            return (data.Last(), data.Count(), data.Average());
        }
    }
}
