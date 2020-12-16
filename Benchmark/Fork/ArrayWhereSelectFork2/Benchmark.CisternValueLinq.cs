using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Fork
{
    partial class ArrayWhereSelectFork2
    {
        [Benchmark]
        public (long, double) CisternValueLinq()
        {
            var last = 
                Data
                .Where(x => (x & 1) == 0)
                .Select(x => (long)x)
                .Last();

            var average =
                Data
                .Where(x => (x & 1) == 0)
                .Select(x => (long)x)
                .Average();

            return (last, average);
        }

        [Benchmark]
        public (long, double) CisternValueLinq_Fork() =>
            Data
            .Where(x => (x & 1) == 0)
            .Select(x => (long)x)
            .Fork(
                x => x.Last(),
                x => x.Average())
            ;

        struct IntToLong : IFunc<int, long> { public long Invoke(int t) => (long)t; }
        struct FilterEvenInts : IFunc<int, bool> { public bool Invoke(int t) => (t & 1) == 0; }
        [Benchmark]
        public (long, double) CisternValueLinq_ValueLambda_Fork() =>
            Data
            .Where(new FilterEvenInts())
            .Select(new IntToLong(), default(long))
            .Fork(
                x => x.Last(),
                x => x.Average());

        [Benchmark]
        public (long, double) CisternValueLinq_Reify()
        {
            var data =
                Data
                .Where(x => (x & 1) == 0)
                .Select(x => (long)x)
                .ToArray();

            return (data.Last(), data.Average());
        }
    }
}
