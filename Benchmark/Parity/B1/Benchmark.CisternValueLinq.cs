using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B1
    {
        [Benchmark]
        public int B01_CisternValueLinq() =>
            _list
            .Select(_selector)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int B03_CisternValueLinq() =>
            _array
            .Select(_selector)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B04_CisternValueLinq() =>
            _list
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B05_CisternValueLinq() =>
            _iterator
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B06_CisternValueLinq() =>
            _iterator
            .Select(_selector)
            .ElementAt(Length / 2);

        [Benchmark]
        public bool B07_CisternValueLinq() =>
            _iterator
            .Select(_selector)
            .Any();

        [Benchmark]
        public int B08_CisternValueLinq() =>
            _iterator
            .Reverse()
            .Select(_selector)
            .First();

        [Benchmark]
        public List<int> B09_CisternValueLinq() =>
            _array
            .Select(_selector)
            .ToList();
    }
}
