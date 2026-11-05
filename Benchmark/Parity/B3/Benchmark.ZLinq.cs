using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B3
    {
        [Benchmark]
        public int B15_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Skip(100)
            .Take(Length / 2)
            .Count();

        [Benchmark]
        public int B16_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Skip(100)
            .Take(Length / 2)
            .Last();

        [Benchmark]
        public int[] B17_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Skip(100)
            .Take(Length / 2)
            .ToArray();

        [Benchmark]
        public bool B18_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Skip(100)
            .Take(Length / 2)
            .Contains(-1);

        [Benchmark]
        public int[] B19_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Take(Length / 2)
            .Append(1)
            .ToArray();

        [Benchmark]
        public int B20_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Skip(100)
            .Take(100)
            .Count();

        [Benchmark]
        public int B21_ZLinq() =>
            _hashSet
            .AsValueEnumerable()
            .Skip(100)
            .Count();

        [Benchmark]
        public int B22_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Where(_predicate)
            .Skip(100)
            .First();

        [Benchmark]
        public int B23_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Append(1)
            .Skip(100)
            .Take(Length / 2)
            .Last();

        [Benchmark]
        public int[] B24_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Skip(100)
            .Take(100)
            .ToArray();

        [Benchmark]
        public int[] B25_ZLinq() =>
            _list
            .AsValueEnumerable()
            .TakeLast(100)
            .ToArray();
    }
}
