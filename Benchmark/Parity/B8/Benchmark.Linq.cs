using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class B8
    {
        [Benchmark]
        public int B61a_Linq() =>
            _array
            .OrderBy(_key)
            .Reverse()
            .First();

        [Benchmark]
        public int B61b_Linq() =>
            Enumerable.Reverse(_list)
            .First();

        [Benchmark]
        public int B61c_Linq() =>
            Data.Iterator(_array)
            .Reverse()
            .First();

        [Benchmark]
        public int B62a_Linq() =>
            _array
            .OrderBy(_key)
            .Reverse()
            .Last();

        [Benchmark]
        public int B62b_Linq() =>
            Enumerable.Reverse(_list)
            .Last();

        [Benchmark]
        public int B62c_Linq() =>
            Data.Iterator(_array)
            .Reverse()
            .Last();

        [Benchmark]
        public int B63a_Linq() =>
            Enumerable.Reverse(_list)
            .ElementAt(Length / 3);

        [Benchmark]
        public int B63b_Linq() =>
            Data.Iterator(_array)
            .Reverse()
            .ElementAt(Length / 3);

        [Benchmark]
        public int B64a_Linq() =>
            Enumerable.Reverse(_list)
            .Count();

        [Benchmark]
        public int B64b_Linq() =>
            Data.Iterator(_array)
            .Reverse()
            .Count();

        [Benchmark]
        public int B64c_Linq() =>
            Enumerable.Reverse(_list)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public bool B64d_Linq() =>
            _set
            .Reverse()
            .Contains(_present);

        [Benchmark]
        public bool B64e_Linq() =>
            Enumerable.Reverse(_list)
            .Contains(_present);

        [Benchmark]
        public int[] B65a_Linq() =>
            Data.Iterator(_array)
            .Reverse()
            .ToArray();

        [Benchmark]
        public int[] B65b_Linq() =>
            Enumerable.Reverse(_list)
            .ToArray();

        [Benchmark]
        public List<int> B65c_Linq() =>
            Data.Iterator(_array)
            .Reverse()
            .ToList();
    }
}
