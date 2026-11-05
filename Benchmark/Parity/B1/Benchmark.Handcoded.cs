using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B1
    {
        [Benchmark]
        public int B01_Handcoded() =>
            _list.Count;

        [Benchmark]
        public int B03_Handcoded() =>
            _selector(_array[Length / 2]);

        [Benchmark]
        public int B04_Handcoded() =>
            _selector(_list[_list.Count - 1]);

        [Benchmark]
        public int B05_Handcoded()
        {
            var last = 0;
            foreach (var x in _iterator)
                last = _selector(x);
            return last;
        }

        [Benchmark]
        public int B06_Handcoded()
        {
            var index = 0;
            foreach (var x in _iterator)
            {
                var value = _selector(x);
                if (index++ == Length / 2)
                    return value;
            }
            throw new InvalidOperationException();
        }

        [Benchmark]
        public bool B07_Handcoded()
        {
            foreach (var x in _iterator)
                return true;
            return false;
        }

        [Benchmark]
        public int B08_Handcoded()
        {
            var all = new List<int>();
            foreach (var x in _iterator)
                all.Add(x);
            return _selector(all[all.Count - 1]);
        }

        [Benchmark]
        public List<int> B09_Handcoded()
        {
            var result = new List<int>(_array.Length);
            foreach (var x in _array)
                result.Add(_selector(x));
            return result;
        }
    }
}
