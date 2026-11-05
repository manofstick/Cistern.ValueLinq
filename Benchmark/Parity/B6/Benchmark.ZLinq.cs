using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B6
    {
        [Benchmark]
        public int[] B49a_ZLinq() =>
            _a
            .AsValueEnumerable()
            .Concat(_b.AsValueEnumerable())
            .ToArray();

        [Benchmark]
        public int[] B49b_ZLinq() =>
            _listA
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .ToArray();

        [Benchmark]
        public int[] B49c_ZLinq() =>
            _a
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .Concat(_c.AsValueEnumerable())
            .ToArray();

        [Benchmark]
        public int[] B50a_ZLinq() =>
            _a
            .AsValueEnumerable()
            .Concat(Data.Iterator(_b).AsValueEnumerable())
            .ToArray();

        [Benchmark]
        public int[] B50b_ZLinq() =>
            Data.Iterator(_a)
            .AsValueEnumerable()
            .Concat(_b.AsValueEnumerable())
            .ToArray();

        [Benchmark]
        public int B51a_ZLinq() =>
            _listA
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .Count();

        [Benchmark]
        public int B51b_ZLinq() =>
            _a
            .AsValueEnumerable()
            .Concat(Data.Iterator(_b).AsValueEnumerable())
            .Count();

        [Benchmark]
        public int B51c_ZLinq() =>
            _listA
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int B52a_ZLinq() =>
            _listA
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .ElementAt(_index);

        [Benchmark]
        public int B52b_ZLinq() =>
            Data.Iterator(_a)
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .ElementAt(_index);

        [Benchmark]
        public int B53a_ZLinq() =>
            _listA
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .First();

        [Benchmark]
        public int B53b_ZLinq() =>
            Data.Iterator(_a)
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .Last();

        [Benchmark]
        public int B53c_ZLinq() =>
            _listA
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .Last();

        [Benchmark]
        public bool B54a_ZLinq() =>
            _a
            .AsValueEnumerable()
            .Concat(_b.AsValueEnumerable())
            .Contains(_presentB);

        [Benchmark]
        public bool B54b_ZLinq() =>
            _setA
            .AsValueEnumerable()
            .Concat(_setB.AsValueEnumerable())
            .Contains(_presentB);

        [Benchmark]
        public bool B54c_ZLinq() =>
            _a
            .AsValueEnumerable()
            .Concat(_b.AsValueEnumerable())
            .Concat(_c.AsValueEnumerable())
            .Contains(_presentC);

        [Benchmark]
        public List<int> B55a_ZLinq() =>
            _listA
            .AsValueEnumerable()
            .Concat(_listB.AsValueEnumerable())
            .ToList();

        [Benchmark]
        public List<int> B55b_ZLinq() =>
            _a
            .AsValueEnumerable()
            .Concat(Data.Iterator(_b).AsValueEnumerable())
            .ToList();
    }
}
