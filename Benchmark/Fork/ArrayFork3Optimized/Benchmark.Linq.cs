using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Fork
{
    partial class ArrayFork3Optimized
    {
        [Benchmark(Baseline = true)]
        public (double, double, double) Linq()
        {
            var min =
                Data
                .Min();

            var max =
                Data
                .Max();

            var last =
                Data
                .Last();

            return (min, max, last);
        }

    }
}
