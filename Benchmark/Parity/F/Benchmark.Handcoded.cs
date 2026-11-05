using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class F
    {
        [Benchmark]
        public int[] F01a_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _iterator)
                if (_half(x))
                    result.Add(x);
            return result.ToArray();
        }

        [Benchmark]
        public List<int> F01b_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _iterator)
                result.Add(_double(x));
            return result;
        }

        [Benchmark]
        public string[] F01c_Handcoded()
        {
            var result = new List<string>();
            foreach (var x in _mixed)
                if (x is string s)
                    result.Add(s);
            return result.ToArray();
        }

        [Benchmark]
        public List<int> F01d_Handcoded()
        {
            var result = new List<int>();
            var index = 0;
            foreach (var x in _iterator)
            {
                if (index++ < 10)
                    continue;
                if (result.Count == 5000)
                    break;
                result.Add(x);
            }
            return result;
        }

        [Benchmark]
        public int[] F02a_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _tenIterator);
            AddAll(result, _collection);
            return result.ToArray();
        }

        [Benchmark]
        public int[] F02b_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _tenIterator);
            AddAll(result, _set);
            return result.ToArray();
        }

        [Benchmark]
        public int[] F02c_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _tenIterator);
            AddAll(result, _collection);
            AddAll(result, _collection);
            return result.ToArray();
        }

        [Benchmark]
        public int[] F02d_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _tenIterator);
            AddAll(result, _tenIterator);
            AddAll(result, _collection);
            return result.ToArray();
        }

        [Benchmark]
        public int[] F02e_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _tenIterator);
            AddAll(result, _collection);
            AddAll(result, _tenIterator);
            return result.ToArray();
        }

        [Benchmark]
        public int[] F02f_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _collection);
            AddAll(result, _tenIterator);
            return result.ToArray();
        }

        [Benchmark]
        public List<int> F02g_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _tenIterator);
            AddAll(result, _collection);
            AddAll(result, _collection);
            return result;
        }

        [Benchmark]
        public int[] F03a_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _iterator);
            result.Add(1);
            result.Add(2);
            return result.ToArray();
        }

        [Benchmark]
        public List<int> F03b_Handcoded()
        {
            var result = new List<int>();
            result.Add(0);
            AddAll(result, _iterator);
            result.Add(1);
            return result;
        }

        [Benchmark]
        public List<int> F04a_Handcoded()
        {
            var result = new List<int>();
            for (var i = 0; i < Length; ++i)
                result.Add(_double(i));
            return result;
        }

        [Benchmark]
        public List<int> F04b_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _collection)
                result.Add(_double(x));
            return result;
        }

        [Benchmark]
        public int[] F05a_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _set);
            return result.ToArray();
        }

        [Benchmark]
        public int[] F05b_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _emptyCollection);
            return result.ToArray();
        }

        [Benchmark]
        public int[] F05c_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _collection);
            return result.ToArray();
        }

        [Benchmark]
        public int[] F06a_Handcoded()
        {
            var result = new int[_ints.Length];
            for (var i = 0; i < result.Length; ++i)
                result[i] = _identity(_ints[i]);
            Array.Sort(result);
            return result;
        }

        [Benchmark]
        public List<int> F06b_Handcoded()
        {
            var result = new List<int>(_ints.Length);
            foreach (var x in _ints)
                result.Add(_identity(x));
            result.Sort();
            return result;
        }

        [Benchmark]
        public int F06c_Handcoded()
        {
            var sorted = new int[_ints.Length];
            for (var i = 0; i < sorted.Length; ++i)
                sorted[i] = _identity(_ints[i]);
            Array.Sort(sorted);
            var sum = 0;
            foreach (var x in sorted)
                sum += x;
            return sum;
        }

        [Benchmark]
        public int F07a_Handcoded()
        {
            var keys = new HashSet<int>();
            foreach (var x in _ints)
                keys.Add(_key(x));
            var sum = 0;
            foreach (var k in keys)
                sum += k;
            return sum;
        }

        [Benchmark]
        public int F07b_Handcoded()
        {
            var keys = new HashSet<int>();
            foreach (var x in _ints)
                keys.Add(_key(x));
            return keys.Count;
        }

        [Benchmark]
        public int F08a_Handcoded()
        {
            var lookup = InnerByKey();
            var sum = 0;
            foreach (var x in _ints)
                if (lookup.TryGetValue(_key(x), out var matches))
                    foreach (var y in matches)
                        sum += _add(x, y);
            return sum;
        }

        [Benchmark]
        public int F08b_Handcoded()
        {
            var lookup = InnerByKey();
            var sum = 0;
            foreach (var x in _ints)
            {
                if (lookup.TryGetValue(_key(x), out var matches))
                    foreach (var y in matches)
                        sum += _add(x, y);
                else
                    sum += _add(x, default);
            }
            return sum;
        }

        [Benchmark]
        public int F09_Handcoded()
        {
            var groups = new Dictionary<int, List<int>>();
            foreach (var x in _ints)
            {
                var key = _key(x);
                if (!groups.TryGetValue(key, out var group))
                    groups[key] = group = new List<int>();
                group.Add(x);
            }
            var sum = 0;
            foreach (var group in groups.Values)
                foreach (var x in group)
                    sum += x;
            return sum;
        }

        [Benchmark]
        public int[] F11a_Handcoded()
        {
            if (_intList.Count == 0)
                return new[] { 0 };
            return _intList.ToArray();
        }

        [Benchmark]
        public List<int> F11b_Handcoded()
        {
            var result = new List<int>();
            AddAll(result, _iterator);
            if (result.Count == 0)
                result.Add(0);
            return result;
        }

        [Benchmark]
        public int F12a_Handcoded()
        {
            var sum = 0;
            foreach (var x in _ints)
                if (_half(x))
                    sum += x;
            return sum;
        }

        [Benchmark]
        public int F12b_Handcoded()
        {
            var sum = 0;
            foreach (var x in _iterator)
                sum += _double(x);
            return sum;
        }

        static void AddAll(List<int> result, IEnumerable<int> source)
        {
            foreach (var x in source)
                result.Add(x);
        }

        Dictionary<int, List<int>> InnerByKey()
        {
            var lookup = new Dictionary<int, List<int>>();
            foreach (var y in _inner)
            {
                var key = _key(y);
                if (!lookup.TryGetValue(key, out var matches))
                    lookup[key] = matches = new List<int>();
                matches.Add(y);
            }
            return lookup;
        }
    }
}
