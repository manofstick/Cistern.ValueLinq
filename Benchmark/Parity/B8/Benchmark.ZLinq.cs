using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B8
    {
        [Benchmark]
        public int B61a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .OrderBy(_key)
            .Reverse()
            .First();

        [Benchmark]
        public int B61b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Reverse()
            .First();

        [Benchmark]
        public int B61c_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Reverse()
            .First();

        [Benchmark]
        public int B62a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .OrderBy(_key)
            .Reverse()
            .Last();

        [Benchmark]
        public int B62b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Reverse()
            .Last();

        [Benchmark]
        public int B62c_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Reverse()
            .Last();

        [Benchmark]
        public int B63a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Reverse()
            .ElementAt(Length / 3);

        [Benchmark]
        public int B63b_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Reverse()
            .ElementAt(Length / 3);

        [Benchmark]
        public int B64a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Reverse()
            .Count();

        [Benchmark]
        public int B64b_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Reverse()
            .Count();

        [Benchmark]
        public int B64c_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Reverse()
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public bool B64d_ZLinq() =>
            _set
            .AsValueEnumerable()
            .Reverse()
            .Contains(_present);

        [Benchmark]
        public bool B64e_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Reverse()
            .Contains(_present);

        [Benchmark]
        public int[] B65a_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Reverse()
            .ToArray();

        [Benchmark]
        public int[] B65b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Reverse()
            .ToArray();

        [Benchmark]
        public List<int> B65c_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Reverse()
            .ToList();
    }
}
