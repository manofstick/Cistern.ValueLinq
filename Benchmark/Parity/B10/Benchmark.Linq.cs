using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    partial class B10
    {
        [Benchmark]
        public int B72a_Linq() =>
            _list
            .Distinct()
            .First();

        [Benchmark]
        public int B72b_Linq() =>
            Data.Iterator(_array)
            .Distinct()
            .First();

        [Benchmark]
        public bool B72c_Linq() =>
            _list
            .Distinct()
            .Any();

        [Benchmark]
        public bool B73a_Linq() =>
            _list
            .Distinct()
            .Contains(_present);

        [Benchmark]
        public bool B73b_Linq() =>
            _set
            .Distinct()
            .Contains(_present);

        [Benchmark]
        public int[] B74a_Linq() =>
            _array
            .Distinct()
            .ToArray();

        [Benchmark]
        public List<int> B74b_Linq() =>
            _array
            .Distinct()
            .ToList();

        [Benchmark]
        public int B74c_Linq() =>
            _array
            .Distinct()
            .Count();

        [Benchmark]
        public int B75a_Linq() =>
            _array
            .Union(_second)
            .First();

        [Benchmark]
        public int B75b_Linq() =>
            Data.Iterator(_array)
            .Union(_second)
            .First();

        [Benchmark]
        public bool B76a_Linq() =>
            _array
            .Union(_second)
            .Contains(_presentInSecond);

        [Benchmark]
        public bool B76b_Linq() =>
            _set
            .Union(_secondSet)
            .Contains(_presentInSecond);

        [Benchmark]
        public int[] B77a_Linq() =>
            _array
            .Union(_second)
            .ToArray();

        [Benchmark]
        public List<int> B77b_Linq() =>
            _array
            .Union(_second)
            .ToList();

        [Benchmark]
        public int B77c_Linq() =>
            _array
            .Union(_second)
            .Count();
    }
}
