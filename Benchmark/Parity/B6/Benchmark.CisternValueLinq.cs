using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B6
    {
        [Benchmark]
        public int[] B49a_CisternValueLinq() =>
            _a
            .Concat(_b)
            .ToArray();

        [Benchmark]
        public int[] B49b_CisternValueLinq() =>
            _listA
            .Concat(_listB)
            .ToArray();

        [Benchmark]
        public int[] B49c_CisternValueLinq() =>
            _a
            .Concat(_listB)
            .Concat(_c)
            .ToArray();

        [Benchmark]
        public int[] B50a_CisternValueLinq() =>
            _a
            .Concat(Data.Iterator(_b))
            .ToArray();

        [Benchmark]
        public int[] B50b_CisternValueLinq() =>
            Data.Iterator(_a)
            .Concat(_b)
            .ToArray();

        [Benchmark]
        public int B51a_CisternValueLinq() =>
            _listA
            .Concat(_listB)
            .Count();

        [Benchmark]
        public int B51b_CisternValueLinq() =>
            _a
            .Concat(Data.Iterator(_b))
            .Count();

        [Benchmark]
        public int B51c_CisternValueLinq() =>
            _listA
            .Concat(_listB)
            .TryGetNonEnumeratedCount(out var count)
                ? count
                : -1;

        [Benchmark]
        public int B52a_CisternValueLinq() =>
            _listA
            .Concat(_listB)
            .ElementAt(_index);

        [Benchmark]
        public int B52b_CisternValueLinq() =>
            Data.Iterator(_a)
            .Concat(_listB)
            .ElementAt(_index);

        [Benchmark]
        public int B53a_CisternValueLinq() =>
            _listA
            .Concat(_listB)
            .First();

        [Benchmark]
        public int B53b_CisternValueLinq() =>
            Data.Iterator(_a)
            .Concat(_listB)
            .Last();

        [Benchmark]
        public int B53c_CisternValueLinq() =>
            _listA
            .Concat(_listB)
            .Last();

        [Benchmark]
        public bool B54a_CisternValueLinq() =>
            _a
            .Concat(_b)
            .Contains(_presentB);

        [Benchmark]
        public bool B54b_CisternValueLinq() =>
            _setA
            .Concat(_setB)
            .Contains(_presentB);

        [Benchmark]
        public bool B54c_CisternValueLinq() =>
            _a
            .Concat(_b)
            .Concat(_c)
            .Contains(_presentC);

        [Benchmark]
        public List<int> B55a_CisternValueLinq() =>
            _listA
            .Concat(_listB)
            .ToList();

        [Benchmark]
        public List<int> B55b_CisternValueLinq() =>
            _a
            .Concat(Data.Iterator(_b))
            .ToList();
    }
}
