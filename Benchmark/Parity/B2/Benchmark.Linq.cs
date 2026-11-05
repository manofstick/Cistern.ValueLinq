using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class B2
    {
        [Benchmark]
        public int B10_Linq() =>
            _array
            .Where(_predicate)
            .Last();

        [Benchmark]
        public int B11_Linq() =>
            _iterator
            .Where(_predicate)
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B12_Linq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .ElementAt(100);

        [Benchmark]
        public int[] B13_Linq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .ToArray();

        [Benchmark]
        public int B14_Linq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .Count();
    }
}
