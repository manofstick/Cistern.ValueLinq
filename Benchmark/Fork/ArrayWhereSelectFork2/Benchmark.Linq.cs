using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Fork
{
    partial class ArrayWhereSelectFork2
    {
        [Benchmark]
        public (long, double) Linq_Reify()
        {
            var data =
                Data
                .Where(x => (x & 1) == 0)
                .Select(x => (long)x)
                .ToArray();

            return (data.Last(), data.Average());
        }

        [Benchmark(Baseline = true)]
        public (long, double) Linq()
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
    }
}
