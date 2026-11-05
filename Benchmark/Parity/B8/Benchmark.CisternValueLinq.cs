using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B8
    {
        [Benchmark]
        public int B61a_CisternValueLinq() =>
            _array
            .OrderBy(_key)
            .Reverse()
            .First();

        [Benchmark]
        public int B61b_CisternValueLinq() =>
            Cistern.ValueLinq.Enumerable.Reverse(_list)
            .First();

        [Benchmark]
        public int B61c_CisternValueLinq() =>
            Data.Iterator(_array)
            .Reverse()
            .First();

        [Benchmark]
        public int B62a_CisternValueLinq() =>
            _array
            .OrderBy(_key)
            .Reverse()
            .Last();

        [Benchmark]
        public int B62b_CisternValueLinq() =>
            Cistern.ValueLinq.Enumerable.Reverse(_list)
            .Last();

        [Benchmark]
        public int B62c_CisternValueLinq() =>
            Data.Iterator(_array)
            .Reverse()
            .Last();

        [Benchmark]
        public int B63a_CisternValueLinq() =>
            Cistern.ValueLinq.Enumerable.Reverse(_list)
            .ElementAt(Length / 3);

        [Benchmark]
        public int B63b_CisternValueLinq() =>
            Data.Iterator(_array)
            .Reverse()
            .ElementAt(Length / 3);

        [Benchmark]
        public int B64a_CisternValueLinq() =>
            Cistern.ValueLinq.Enumerable.Reverse(_list)
            .Count();

        [Benchmark]
        public int B64b_CisternValueLinq() =>
            Data.Iterator(_array)
            .Reverse()
            .Count();

        [Benchmark]
        public int B64c_CisternValueLinq() =>
            Cistern.ValueLinq.Enumerable.Reverse(_list)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public bool B64d_CisternValueLinq() =>
            _set
            .Reverse()
            .Contains(_present);

        [Benchmark]
        public bool B64e_CisternValueLinq() =>
            Cistern.ValueLinq.Enumerable.Reverse(_list)
            .Contains(_present);

        [Benchmark]
        public int[] B65a_CisternValueLinq() =>
            Data.Iterator(_array)
            .Reverse()
            .ToArray();

        [Benchmark]
        public int[] B65b_CisternValueLinq() =>
            Cistern.ValueLinq.Enumerable.Reverse(_list)
            .ToArray();

        [Benchmark]
        public List<int> B65c_CisternValueLinq() =>
            Data.Iterator(_array)
            .Reverse()
            .ToList();
    }
}
