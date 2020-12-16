using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Fork
{
    partial class ArrayWhereSelectFork3
    {
        [Benchmark]
        public (long, int, double) Linq_Reify()
        {
            var data =
                Data
                .Where(x => (x & 1) == 0)
                .Select(x => (long)x)
                .ToArray();

            return (data.Last(), data.Count(), data.Average());
        }

        [Benchmark(Baseline = true)]
        public (long, int, double) Linq()
        {
            var last =
                Data
                .Where(x => (x & 1) == 0)
                .Select(x => (long)x)
                .Last();

            var count =
                Data
                .Where(x => (x & 1) == 0)
                .Select(x => (long)x)
                .Count();

            var average =
                Data
                .Where(x => (x & 1) == 0)
                .Select(x => (long)x)
                .Average();

            return (last, count, average);
        }
    }
}
