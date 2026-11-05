using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B7
    {
        [Benchmark]
        public int B56a_CisternValueLinq() =>
            _list
            .Append(_x)
            .Count();

        [Benchmark]
        public int B56b_CisternValueLinq() =>
            _list
            .Prepend(_x)
            .Append(_y)
            .Append(_z)
            .Count();

        [Benchmark]
        public int B56c_CisternValueLinq() =>
            _list
            .Append(_x)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int[] B57a_CisternValueLinq() =>
            _array
            .Append(_x)
            .ToArray();

        [Benchmark]
        public int[] B57b_CisternValueLinq() =>
            _list
            .Prepend(_x)
            .Append(_y)
            .ToArray();

        [Benchmark]
        public int[] B57c_CisternValueLinq() =>
            Data.Iterator(_array)
            .Append(_x)
            .ToArray();

        [Benchmark]
        public List<int> B57d_CisternValueLinq() =>
            _list
            .Append(_x)
            .ToList();

        [Benchmark]
        public int B58a_CisternValueLinq() =>
            Data.Iterator(_array)
            .Prepend(_x)
            .First();

        [Benchmark]
        public int B58b_CisternValueLinq() =>
            Data.Iterator(_array)
            .Append(_x)
            .Last();

        [Benchmark]
        public int B59a_CisternValueLinq() =>
            _list
            .Append(_x)
            .First();

        [Benchmark]
        public int B59b_CisternValueLinq() =>
            _list
            .Prepend(_x)
            .Last();

        [Benchmark]
        public int B59c_CisternValueLinq() =>
            _list
            .Prepend(_x)
            .ElementAt(Length / 2);

        [Benchmark]
        public bool B60a_CisternValueLinq() =>
            _list
            .Append(_x)
            .Contains(_x);

        [Benchmark]
        public bool B60b_CisternValueLinq() =>
            _list
            .Append(_x)
            .Contains(_present);

        [Benchmark]
        public bool B60c_CisternValueLinq() =>
            _set
            .Append(_x)
            .Contains(_present);
    }
}
