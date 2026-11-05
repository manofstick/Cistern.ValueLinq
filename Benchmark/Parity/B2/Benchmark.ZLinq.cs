using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B2
    {
        [Benchmark]
        public int B10_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Where(_predicate)
            .Last();

        [Benchmark]
        public int B11_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B12_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .ElementAt(100);

        [Benchmark]
        public int[] B13_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .ToArray();

        [Benchmark]
        public int B14_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Where(_predicate)
            .Select(_selector)
            .Count();
    }
}
