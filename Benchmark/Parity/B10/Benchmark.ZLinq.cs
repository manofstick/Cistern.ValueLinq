using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Parity
{
    partial class B10
    {
        [Benchmark]
        public int B72a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Distinct()
            .First();

        [Benchmark]
        public int B72b_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Distinct()
            .First();

        [Benchmark]
        public bool B72c_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Distinct()
            .Any();

        [Benchmark]
        public bool B73a_ZLinq() =>
            _list
            .AsValueEnumerable()
            .Distinct()
            .Contains(_present);

        [Benchmark]
        public bool B73b_ZLinq() =>
            _set
            .AsValueEnumerable()
            .Distinct()
            .Contains(_present);

        [Benchmark]
        public int[] B74a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Distinct()
            .ToArray();

        [Benchmark]
        public List<int> B74b_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Distinct()
            .ToList();

        [Benchmark]
        public int B74c_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Distinct()
            .Count();

        [Benchmark]
        public int B75a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Union(_second)
            .First();

        [Benchmark]
        public int B75b_ZLinq() =>
            Data.Iterator(_array)
            .AsValueEnumerable()
            .Union(_second)
            .First();

        [Benchmark]
        public bool B76a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Union(_second)
            .Contains(_presentInSecond);

        [Benchmark]
        public bool B76b_ZLinq() =>
            _set
            .AsValueEnumerable()
            .Union(_secondSet)
            .Contains(_presentInSecond);

        [Benchmark]
        public int[] B77a_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Union(_second)
            .ToArray();

        [Benchmark]
        public List<int> B77b_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Union(_second)
            .ToList();

        [Benchmark]
        public int B77c_ZLinq() =>
            _array
            .AsValueEnumerable()
            .Union(_second)
            .Count();
    }
}
