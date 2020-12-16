using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Fork
{
    partial class RangeSelectWhereFork2
    {
        [Benchmark]
        public (long, double) Linq_Reify()
        {
            var data =
                Enumerable
                .Range(0, Length)
                .Select(x => (long)x)
                .Where(x => (x & 1) == 0)
                .ToArray();

            return (data.Last(), data.Average());
        }

        [Benchmark(Baseline = true)]
        public (long, double) Linq()
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
    }
}
