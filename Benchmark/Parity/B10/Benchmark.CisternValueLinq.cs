using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B10
    {
        [Benchmark]
        public int B72a_CisternValueLinq() =>
            _list
            .Distinct()
            .First();

        [Benchmark]
        public int B72b_CisternValueLinq() =>
            Data.Iterator(_array)
            .Distinct()
            .First();

        [Benchmark]
        public bool B72c_CisternValueLinq() =>
            _list
            .Distinct()
            .Any();

        [Benchmark]
        public bool B73a_CisternValueLinq() =>
            _list
            .Distinct()
            .Contains(_present);

        [Benchmark]
        public bool B73b_CisternValueLinq() =>
            _set
            .Distinct()
            .Contains(_present);

        [Benchmark]
        public int[] B74a_CisternValueLinq() =>
            _array
            .Distinct()
            .ToArray();

        [Benchmark]
        public List<int> B74b_CisternValueLinq() =>
            _array
            .Distinct()
            .ToList();

        [Benchmark]
        public int B74c_CisternValueLinq() =>
            _array
            .Distinct()
            .Count();

        [Benchmark]
        public int B75a_CisternValueLinq() =>
            _array
            .Union(_second)
            .First();

        // an iterator binds the IEnumerable<T> Union, so its time is building that query and First's requests through its
        // SystemIEnumerable parts, then a push of the iterator for its first element, as it has no count to skip it by
        [Benchmark]
        public int B75b_CisternValueLinq() =>
            Data.Iterator(_array)
            .Union(_second)
            .First();

        [Benchmark]
        public bool B76a_CisternValueLinq() =>
            _array
            .Union(_second)
            .Contains(_presentInSecond);

        [Benchmark]
        public bool B76b_CisternValueLinq() =>
            _set
            .Union(_secondSet)
            .Contains(_presentInSecond);

        [Benchmark]
        public int[] B77a_CisternValueLinq() =>
            _array
            .Union(_second)
            .ToArray();

        [Benchmark]
        public List<int> B77b_CisternValueLinq() =>
            _array
            .Union(_second)
            .ToList();

        [Benchmark]
        public int B77c_CisternValueLinq() =>
            _array
            .Union(_second)
            .Count();
    }
}
