using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B1
    {
        [Benchmark]
        public int B01_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Select(_selector)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int B03_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Select(_selector)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B04_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B05_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B06_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Select(_selector)
            .ElementAt(Length / 2);

        [Benchmark]
        public bool B07_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Select(_selector)
            .Any();

        [Benchmark]
        public int B08_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Reverse()
            .Select(_selector)
            .First();

        [Benchmark]
        public List<int> B09_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Select(_selector)
            .ToList();
    }
}
