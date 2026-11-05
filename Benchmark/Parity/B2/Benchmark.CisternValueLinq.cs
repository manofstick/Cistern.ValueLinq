using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B2
    {
        [Benchmark]
        public int B10_CisternValueLinq() =>
            _array
            .Where(_predicate)
            .Last();

        [Benchmark]
        public int B11_CisternValueLinq() =>
            _iterator
            .Where(_predicate)
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B12_CisternValueLinq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .ElementAt(100);

        [Benchmark]
        public int[] B13_CisternValueLinq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .ToArray();

        [Benchmark]
        public int B14_CisternValueLinq() =>
            _array
            .Where(_predicate)
            .Select(_selector)
            .Count();
    }
}
