using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B3
    {
        [Benchmark]
        public int B15_CisternValueLinq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .Count();

        [Benchmark]
        public int B16_CisternValueLinq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .Last();

        [Benchmark]
        public int[] B17_CisternValueLinq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .ToArray();

        [Benchmark]
        public bool B18_CisternValueLinq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .Contains(-1);

        [Benchmark]
        public int[] B19_CisternValueLinq() =>
            _list
            .Take(Length / 2)
            .Append(1)
            .ToArray();

        [Benchmark]
        public int B20_CisternValueLinq() =>
            _iterator
            .Skip(100)
            .Take(100)
            .Count();

        [Benchmark]
        public int B21_CisternValueLinq() =>
            _hashSet
            .Skip(100)
            .Count();

        [Benchmark]
        public int B22_CisternValueLinq() =>
            _array
            .Where(_predicate)
            .Skip(100)
            .First();

        [Benchmark]
        public int B23_CisternValueLinq() =>
            _list
            .Append(1)
            .Skip(100)
            .Take(Length / 2)
            .Last();

        [Benchmark]
        public int[] B24_CisternValueLinq() =>
            _iterator
            .Skip(100)
            .Take(100)
            .ToArray();

        [Benchmark]
        public int[] B25_CisternValueLinq() =>
            _list
            .TakeLast(100)
            .ToArray();
    }
}
