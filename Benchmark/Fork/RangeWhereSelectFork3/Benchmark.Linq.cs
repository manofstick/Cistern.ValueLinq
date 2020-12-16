using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Fork
{
    partial class RangeSelectWhereFork3
    {
        [Benchmark]
        public (long, int, double) Linq_Reify()
        {
            var data =
                Enumerable
                .Range(0, Length)
                .Select(x => (long)x)
                .Where(x => (x & 1) == 0)
                .ToArray();

            return (data.Last(), data.Count(), data.Average());
        }

        [Benchmark(Baseline = true)]
        public (long, int, double) Linq()
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
    }
}
