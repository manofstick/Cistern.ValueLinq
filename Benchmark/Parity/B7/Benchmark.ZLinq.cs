using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B7
    {
        [Benchmark]
        public int B56a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Append(_x)
            .Count();

        [Benchmark]
        public int B56b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Prepend(_x)
            .Append(_y)
            .Append(_z)
            .Count();

        [Benchmark]
        public int B56c_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Append(_x)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int[] B57a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Append(_x)
            .ToArray();

        [Benchmark]
        public int[] B57b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Prepend(_x)
            .Append(_y)
            .ToArray();

        [Benchmark]
        public int[] B57c_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Append(_x)
            .ToArray();

        [Benchmark]
        public List<int> B57d_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Append(_x)
            .ToList();

        [Benchmark]
        public int B58a_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Prepend(_x)
            .First();

        [Benchmark]
        public int B58b_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Append(_x)
            .Last();

        [Benchmark]
        public int B59a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Append(_x)
            .First();

        [Benchmark]
        public int B59b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Prepend(_x)
            .Last();

        [Benchmark]
        public int B59c_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Prepend(_x)
            .ElementAt(Length / 2);

        [Benchmark]
        public bool B60a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Append(_x)
            .Contains(_x);

        [Benchmark]
        public bool B60b_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Append(_x)
            .Contains(_present);

        [Benchmark]
        public bool B60c_ZLinq() =>
            _set
            .AsValueEnumerable()
            .Append(_x)
            .Contains(_present);
    }
}
