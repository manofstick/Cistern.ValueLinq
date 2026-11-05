using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B10
    {
        [Benchmark]
        public int B72a_Handcoded()
        {
            foreach (var x in _list)
                return x;
            throw new InvalidOperationException();
        }

        [Benchmark]
        public int B72b_Handcoded()
        {
            foreach (var x in Data.Iterator(_array))
                return x;
            throw new InvalidOperationException();
        }

        [Benchmark]
        public bool B72c_Handcoded() =>
            _list.Count > 0;

        [Benchmark]
        public bool B73a_Handcoded() =>
            _list.Contains(_present);

        [Benchmark]
        public bool B73b_Handcoded() =>
            _set.Contains(_present);

        [Benchmark]
        public int[] B74a_Handcoded() =>
            Distinct(_array).ToArray();

        [Benchmark]
        public List<int> B74b_Handcoded() =>
            Distinct(_array);

        [Benchmark]
        public int B74c_Handcoded() =>
            new HashSet<int>(_array).Count;

        [Benchmark]
        public int B75a_Handcoded() =>
            _array[0];

        [Benchmark]
        public int B75b_Handcoded()
        {
            foreach (var x in Data.Iterator(_array))
                return x;
            throw new InvalidOperationException();
        }

        [Benchmark]
        public bool B76a_Handcoded() =>
            Array.IndexOf(_array, _presentInSecond) >= 0 || Array.IndexOf(_second, _presentInSecond) >= 0;

        [Benchmark]
        public bool B76b_Handcoded() =>
            _set.Contains(_presentInSecond) || _secondSet.Contains(_presentInSecond);

        [Benchmark]
        public int[] B77a_Handcoded() =>
            Union(_array, _second).ToArray();

        [Benchmark]
        public List<int> B77b_Handcoded() =>
            Union(_array, _second);

        [Benchmark]
        public int B77c_Handcoded()
        {
            var seen = new HashSet<int>(_array);
            seen.UnionWith(_second);
            return seen.Count;
        }

        // each value once, in order of first appearance
        static List<int> Distinct(int[] values)
        {
            var seen = new HashSet<int>();
            var result = new List<int>();
            foreach (var x in values)
                if (seen.Add(x))
                    result.Add(x);
            return result;
        }

        static List<int> Union(int[] first, int[] second)
        {
            var seen = new HashSet<int>();
            var result = new List<int>();
            foreach (var x in first)
                if (seen.Add(x))
                    result.Add(x);
            foreach (var x in second)
                if (seen.Add(x))
                    result.Add(x);
            return result;
        }
    }
}
