using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class B1
    {
        [Benchmark]
        public int B01_Linq() =>
            _list
            .Select(_selector)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int B03_Linq() =>
            _array
            .Select(_selector)
            .ElementAt(Length / 2);

        [Benchmark]
        public int B04_Linq() =>
            _list
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B05_Linq() =>
            _iterator
            .Select(_selector)
            .Last();

        [Benchmark]
        public int B06_Linq() =>
            _iterator
            .Select(_selector)
            .ElementAt(Length / 2);

        [Benchmark]
        public bool B07_Linq() =>
            _iterator
            .Select(_selector)
            .Any();

        [Benchmark]
        public int B08_Linq() =>
            _iterator
            .Reverse()
            .Select(_selector)
            .First();

        [Benchmark]
        public List<int> B09_Linq() =>
            _array
            .Select(_selector)
            .ToList();
    }
}
