using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B2
    {
        [Benchmark]
        public int B10_Handcoded()
        {
            var last = 0;
            foreach (var x in _array)
                if (_predicate(x))
                    last = x;
            return last;
        }

        [Benchmark]
        public int B11_Handcoded()
        {
            var last = 0;
            foreach (var x in _iterator)
                if (_predicate(x))
                    last = _selector(x);
            return last;
        }

        [Benchmark]
        public int B12_Handcoded()
        {
            var index = 0;
            foreach (var x in _array)
                if (_predicate(x) && index++ == 100)
                    return _selector(x);
            throw new InvalidOperationException();
        }

        [Benchmark]
        public int[] B13_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _array)
                if (_predicate(x))
                    result.Add(_selector(x));
            return result.ToArray();
        }

        [Benchmark]
        public int B14_Handcoded()
        {
            var count = 0;
            foreach (var x in _array)
                if (_predicate(x))
                    ++count;
            return count;
        }
    }
}
