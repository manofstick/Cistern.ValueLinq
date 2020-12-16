using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Fork
{
    partial class ArrayFork3Optimized
    {
        [Benchmark]
        public (double, double, double) CisternValueLinq()
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

        [Benchmark]
        public (double, double, double) CisternValueLinq_Fork() =>
            Data
            .OfArray()
            .Fork(
                x => x.Min(),
                x => x.Max(),
                x => x.Last());
    }
}
