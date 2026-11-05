using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class B7
    {
        [Benchmark]
        public int B56a_Linq() =>
            _list
            .Append(_x)
            .Count();

        [Benchmark]
        public int B56b_Linq() =>
            _list
            .Prepend(_x)
            .Append(_y)
            .Append(_z)
            .Count();

        [Benchmark]
        public int B56c_Linq() =>
            _list
            .Append(_x)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int[] B57a_Linq() =>
            _array
            .Append(_x)
            .ToArray();

        [Benchmark]
        public int[] B57b_Linq() =>
            _list
            .Prepend(_x)
            .Append(_y)
            .ToArray();

        [Benchmark]
        public int[] B57c_Linq() =>
            Data.Iterator(_array)
            .Append(_x)
            .ToArray();

        [Benchmark]
        public List<int> B57d_Linq() =>
            _list
            .Append(_x)
            .ToList();

        [Benchmark]
        public int B58a_Linq() =>
            Data.Iterator(_array)
            .Prepend(_x)
            .First();

        [Benchmark]
        public int B58b_Linq() =>
            Data.Iterator(_array)
            .Append(_x)
            .Last();

        [Benchmark]
        public int B59a_Linq() =>
            _list
            .Append(_x)
            .First();

        [Benchmark]
        public int B59b_Linq() =>
            _list
            .Prepend(_x)
            .Last();

        [Benchmark]
        public int B59c_Linq() =>
            _list
            .Prepend(_x)
            .ElementAt(Length / 2);

        [Benchmark]
        public bool B60a_Linq() =>
            _list
            .Append(_x)
            .Contains(_x);

        [Benchmark]
        public bool B60b_Linq() =>
            _list
            .Append(_x)
            .Contains(_present);

        [Benchmark]
        public bool B60c_Linq() =>
            _set
            .Append(_x)
            .Contains(_present);
    }
}
