using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net8
    {
        [Benchmark]
        public List<int> N01a_Handcoded()
        {
            var list = new List<int>();
            foreach (var x in _source1024)
                list.Add(x * 2);
            return list;
        }

        [Benchmark]
        public List<byte> N01b_Handcoded()
        {
            var list = new List<byte>(1024);
            for (var i = 0; i < 1024; ++i)
                list.Add((byte)'a');
            return list;
        }

        [Benchmark]
        public List<int> N01c_Handcoded()
        {
            var list = new List<int>(1024);
            for (var i = 0; i < 1024; ++i)
                list.Add(i * 2);
            return list;
        }

        [Benchmark]
        public List<int> N02_Handcoded()
        {
            var list = new List<int>(16_384);
            for (var i = 0; i < 16_384; ++i)
                list.Add(i);
            return list;
        }

        [Benchmark]
        public List<byte> N03_Handcoded()
        {
            _bytesList.Clear();
            for (var i = 0; i < 1024; ++i)
                _bytesList.Add((byte)'a');
            return _bytesList;
        }

        [Benchmark]
        public byte N04_Handcoded()
        {
            var max = _bytes4096[0];
            for (var i = 1; i < _bytes4096.Length; ++i)
            {
                if (_bytes4096[i] > max)
                    max = _bytes4096[i];
            }
            return max;
        }

        [Benchmark]
        public int N05_Handcoded()
        {
            var sum = 0;
            foreach (var x in _source1024)
                sum = checked(sum + x);
            return sum;
        }

        [Benchmark]
        public int N06_Handcoded()
        {
            var sorted = new List<int>(Descending(1000)).ToArray();
            Array.Sort(sorted);
            var sum = 0;
            foreach (var value in sorted)
                sum += value;
            return sum;
        }

        [Benchmark]
        public int N07_Handcoded()
        {
            var keys = new int[_reversed1000000.Length];
            for (var i = 0; i < keys.Length; ++i)
                keys[i] = _reversed1000000[i] * 2;
            var items = (int[])_reversed1000000.Clone();
            Array.Sort(keys, items);
            var sum = 0;
            foreach (var value in items)
                sum += value;
            return sum;
        }

        [Benchmark]
        public bool N08_Handcoded() =>
            _str.Length != 0 &&
            _list3.Count != 0 &&
            _array3.Length != 0;
    }
}
