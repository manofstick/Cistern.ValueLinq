using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B9
    {
        [Benchmark]
        public int B66a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Shuffle()
            .First();

        [Benchmark]
        public int B66b_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Shuffle()
            .First();

        [Benchmark]
        public int B66c_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Shuffle()
            .Last();

        [Benchmark]
        public int B66d_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Shuffle()
            .ElementAt(Length / 2);

        [Benchmark]
        public int[] B67a_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Shuffle()
            .Take(10)
            .ToArray();

        [Benchmark]
        public List<int> B67b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Shuffle()
            .Take(10)
            .ToList();

        [Benchmark]
        public int B67c_ZLinq()
        {
            var sum = 0;
            foreach (var x in _array.AsValueEnumerable().Shuffle().Take(10))
                sum += x;
            return sum;
        }

        [Benchmark]
        public int B68a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Shuffle()
            .Count();

        [Benchmark]
        public int B68b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Shuffle()
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int B68c_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Shuffle()
            .Take(10)
            .Count();

        [Benchmark]
        public bool B69a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Shuffle()
            .Contains(_present);

        [Benchmark]
        public bool B70a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Shuffle()
            .Take(10)
            .Contains(_absent);

        [Benchmark]
        public int[] B71a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Shuffle()
            .ToArray();

        [Benchmark]
        public List<int> B71b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Shuffle()
            .ToList();
    }
}
