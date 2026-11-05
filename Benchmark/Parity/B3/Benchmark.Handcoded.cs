using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B3
    {
        [Benchmark]
        public int B15_Handcoded()
        {
            var count = 0;
            for (var i = 100; i < _list.Count && count < Length / 2; i++)
                count++;
            return count;
        }

        [Benchmark]
        public int B16_Handcoded()
        {
            var found = false;
            var last = 0;
            for (var i = 100; i < _list.Count && i < 100 + Length / 2; i++)
            {
                found = true;
                last = _list[i];
            }
            if (!found)
                throw new InvalidOperationException("Sequence contains no elements");
            return last;
        }

        [Benchmark]
        public int[] B17_Handcoded()
        {
            var length = Math.Min(Length / 2, Math.Max(_list.Count - 100, 0));
            var result = new int[length];
            for (var i = 0; i < length; i++)
                result[i] = _list[100 + i];
            return result;
        }

        [Benchmark]
        public bool B18_Handcoded()
        {
            for (var i = 100; i < _list.Count && i < 100 + Length / 2; i++)
            {
                if (_list[i] == -1)
                    return true;
            }
            return false;
        }

        [Benchmark]
        public int[] B19_Handcoded()
        {
            var taken = Math.Min(Length / 2, _list.Count);
            var result = new int[taken + 1];
            for (var i = 0; i < taken; i++)
                result[i] = _list[i];
            result[taken] = 1;
            return result;
        }

        [Benchmark]
        public int B20_Handcoded()
        {
            var index = 0;
            var count = 0;
            foreach (var x in _iterator)
            {
                if (index++ < 100)
                    continue;
                if (count == 100)
                    break;
                count++;
            }
            return count;
        }

        [Benchmark]
        public int B21_Handcoded()
        {
            var index = 0;
            var count = 0;
            foreach (var x in _hashSet)
            {
                if (index++ >= 100)
                    count++;
            }
            return count;
        }

        [Benchmark]
        public int B22_Handcoded()
        {
            var passed = 0;
            foreach (var x in _array)
            {
                if (_predicate(x) && passed++ == 100)
                    return x;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        [Benchmark]
        public int B23_Handcoded()
        {
            // the list followed by the appended 1
            var found = false;
            var last = 0;
            for (var i = 100; i < _list.Count + 1 && i < 100 + Length / 2; i++)
            {
                found = true;
                last = i < _list.Count ? _list[i] : 1;
            }
            if (!found)
                throw new InvalidOperationException("Sequence contains no elements");
            return last;
        }

        [Benchmark]
        public int[] B24_Handcoded()
        {
            var result = new List<int>();
            var index = 0;
            foreach (var x in _iterator)
            {
                if (index++ < 100)
                    continue;
                if (result.Count == 100)
                    break;
                result.Add(x);
            }
            return result.ToArray();
        }

        [Benchmark]
        public int[] B25_Handcoded()
        {
            var length = Math.Min(100, _list.Count);
            var result = new int[length];
            for (var i = 0; i < length; i++)
                result[i] = _list[_list.Count - length + i];
            return result;
        }
    }
}
