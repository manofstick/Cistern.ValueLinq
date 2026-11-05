using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B4
    {
        [Benchmark]
        public int B26a_Handcoded()
        {
            var count = 0;
            for (var i = 0; i < Length; i++)
                count++;
            return count;
        }

        [Benchmark]
        public bool B26b_Handcoded()
        {
            for (var i = _start; i < _start + Length; i++)
            {
                if (i == _start + Length - 10)
                    return true;
            }
            return false;
        }

        [Benchmark]
        public int B26c_Handcoded()
        {
            for (var i = 0; i < Length; i++)
            {
                if (i == Length / 2)
                    return _start + i;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        [Benchmark]
        public int B26d_Handcoded()
        {
            if (Length == 0)
                throw new InvalidOperationException("Sequence contains no elements");
            return _start;
        }

        [Benchmark]
        public int B26e_Handcoded()
        {
            if (Length == 0)
                throw new InvalidOperationException("Sequence contains no elements");
            var last = 0;
            for (var i = _start; i < _start + Length; i++)
                last = i;
            return last;
        }

        [Benchmark]
        public int[] B27a_Handcoded()
        {
            var result = new int[Length];
            for (var i = 0; i < Length; i++)
                result[i] = _start + i;
            return result;
        }

        [Benchmark]
        public List<int> B27b_Handcoded()
        {
            var result = new List<int>();
            for (var i = 0; i < Length; i++)
                result.Add(_start + i);
            return result;
        }

        [Benchmark]
        public List<int> B28_Handcoded()
        {
            var result = new List<int>();
            for (var i = 0; i < Length; i++)
                result.Add(_start + i);
            return result;
        }

        [Benchmark]
        public int B29a_Handcoded()
        {
            var count = 0;
            for (var i = 0; i < Length; i++)
                count++;
            return count;
        }

        [Benchmark]
        public bool B29b_Handcoded()
        {
            for (var i = 0; i < Length; i++)
            {
                if (_element == _absent)
                    return true;
            }
            return false;
        }

        [Benchmark]
        public int B29c_Handcoded()
        {
            for (var i = 0; i < Length; i++)
            {
                if (i == Length / 2)
                    return _element;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        [Benchmark]
        public int B29d_Handcoded()
        {
            if (Length == 0)
                throw new InvalidOperationException("Sequence contains no elements");
            return _element;
        }

        [Benchmark]
        public int B29e_Handcoded()
        {
            if (Length == 0)
                throw new InvalidOperationException("Sequence contains no elements");
            var last = 0;
            for (var i = 0; i < Length; i++)
                last = _element;
            return last;
        }

        [Benchmark]
        public int[] B30a_Handcoded()
        {
            var result = new int[Length];
            for (var i = 0; i < Length; i++)
                result[i] = _element;
            return result;
        }

        [Benchmark]
        public List<int> B30b_Handcoded()
        {
            var result = new List<int>();
            for (var i = 0; i < Length; i++)
                result.Add(_element);
            return result;
        }

        [Benchmark]
        public string[] B30c_Handcoded()
        {
            var result = new string[Length];
            for (var i = 0; i < Length; i++)
                result[i] = _nullString;
            return result;
        }

        [Benchmark]
        public List<string> B30d_Handcoded()
        {
            var result = new List<string>();
            for (var i = 0; i < Length; i++)
                result.Add(_nullString);
            return result;
        }

        [Benchmark]
        public List<int> B31_Handcoded()
        {
            var result = new List<int>();
            for (var i = 0; i < Length; i++)
                result.Add(_element);
            return result;
        }

        [Benchmark]
        public int B32a_Handcoded()
        {
            for (var i = 0; i < Length; i++)
            {
                var selected = _selector(_start + i);
                if (i == Length / 2)
                    return selected;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        [Benchmark]
        public int B32b_Handcoded()
        {
            if (Length == 0)
                throw new InvalidOperationException("Sequence contains no elements");
            return _selector(_start);
        }

        [Benchmark]
        public int B32c_Handcoded()
        {
            if (Length == 0)
                throw new InvalidOperationException("Sequence contains no elements");
            var last = 0;
            for (var i = 0; i < Length; i++)
                last = _selector(_start + i);
            return last;
        }

        [Benchmark]
        public int B32d_Handcoded() =>
            Length;
    }
}
