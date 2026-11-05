using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B7
    {
        [Benchmark]
        public int B56a_Handcoded() =>
            _list.Count + 1;

        [Benchmark]
        public int B56b_Handcoded() =>
            _list.Count + 3;

        [Benchmark]
        public int B56c_Handcoded() =>
            _list.Count + 1;

        [Benchmark]
        public int[] B57a_Handcoded()
        {
            var result = new int[_array.Length + 1];
            for (var i = 0; i < _array.Length; ++i)
                result[i] = _array[i];
            result[_array.Length] = _x;
            return result;
        }

        [Benchmark]
        public int[] B57b_Handcoded()
        {
            var result = new int[_list.Count + 2];
            result[0] = _x;
            var index = 1;
            foreach (var x in _list)
                result[index++] = x;
            result[index] = _y;
            return result;
        }

        [Benchmark]
        public int[] B57c_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in Data.Iterator(_array))
                result.Add(x);
            result.Add(_x);
            return result.ToArray();
        }

        [Benchmark]
        public List<int> B57d_Handcoded()
        {
            var result = new List<int>(_list.Count + 1);
            result.AddRange(_list);
            result.Add(_x);
            return result;
        }

        // the prepended element is the first, whatever the iterator holds
        [Benchmark]
        public int B58a_Handcoded() =>
            _x;

        // the appended element is the last, whatever the iterator holds
        [Benchmark]
        public int B58b_Handcoded() =>
            _x;

        [Benchmark]
        public int B59a_Handcoded() =>
            _list[0];

        [Benchmark]
        public int B59b_Handcoded() =>
            _list[_list.Count - 1];

        [Benchmark]
        public int B59c_Handcoded() =>
            _list[Length / 2 - 1];

        [Benchmark]
        public bool B60a_Handcoded()
        {
            foreach (var x in _list)
                if (x == _x)
                    return true;
            return true;
        }

        [Benchmark]
        public bool B60b_Handcoded()
        {
            foreach (var x in _list)
                if (x == _present)
                    return true;
            return _present == _x;
        }

        [Benchmark]
        public bool B60c_Handcoded() =>
            _set.Contains(_present) || _present == _x;
    }
}
