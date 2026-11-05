using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class B3
    {
        [Benchmark]
        public int B15_Linq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .Count();

        [Benchmark]
        public int B16_Linq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .Last();

        [Benchmark]
        public int[] B17_Linq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .ToArray();

        [Benchmark]
        public bool B18_Linq() =>
            _list
            .Skip(100)
            .Take(Length / 2)
            .Contains(-1);

        [Benchmark]
        public int[] B19_Linq() =>
            _list
            .Take(Length / 2)
            .Append(1)
            .ToArray();

        [Benchmark]
        public int B20_Linq() =>
            _iterator
            .Skip(100)
            .Take(100)
            .Count();

        [Benchmark]
        public int B21_Linq() =>
            _hashSet
            .Skip(100)
            .Count();

        [Benchmark]
        public int B22_Linq() =>
            _array
            .Where(_predicate)
            .Skip(100)
            .First();

        [Benchmark]
        public int B23_Linq() =>
            _list
            .Append(1)
            .Skip(100)
            .Take(Length / 2)
            .Last();

        [Benchmark]
        public int[] B24_Linq() =>
            _iterator
            .Skip(100)
            .Take(100)
            .ToArray();

        [Benchmark]
        public int[] B25_Linq() =>
            _list
            .TakeLast(100)
            .ToArray();
    }
}
