using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B9
    {
        [Benchmark]
        public int B66a_CisternValueLinq() =>
            _list
            .Shuffle()
            .First();

        [Benchmark]
        public int B66b_CisternValueLinq() =>
            Data.Iterator(_array)
            .Shuffle()
            .First();

        [Benchmark]
        public int B66c_CisternValueLinq() =>
            _list
            .Shuffle()
            .Last();

        [Benchmark]
        public int B66d_CisternValueLinq() =>
            _list
            .Shuffle()
            .ElementAt(Length / 2);

        [Benchmark]
        public int[] B67a_CisternValueLinq() =>
            Data.Iterator(_array)
            .Shuffle()
            .Take(10)
            .ToArray();

        [Benchmark]
        public List<int> B67b_CisternValueLinq() =>
            _list
            .Shuffle()
            .Take(10)
            .ToList();

        [Benchmark]
        public int B67c_CisternValueLinq()
        {
            var sum = 0;
            foreach (var x in _array.Shuffle().Take(10))
                sum += x;
            return sum;
        }

        [Benchmark]
        public int B68a_CisternValueLinq() =>
            _list
            .Shuffle()
            .Count();

        [Benchmark]
        public int B68b_CisternValueLinq() =>
            _list
            .Shuffle()
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int B68c_CisternValueLinq() =>
            _list
            .Shuffle()
            .Take(10)
            .Count();

        [Benchmark]
        public bool B69a_CisternValueLinq() =>
            _list
            .Shuffle()
            .Contains(_present);

        [Benchmark]
        public bool B70a_CisternValueLinq() =>
            _array
            .Shuffle()
            .Take(10)
            .Contains(_absent);

        [Benchmark]
        public int[] B71a_CisternValueLinq() =>
            _array
            .Shuffle()
            .ToArray();

        [Benchmark]
        public List<int> B71b_CisternValueLinq() =>
            _list
            .Shuffle()
            .ToList();
    }
}
