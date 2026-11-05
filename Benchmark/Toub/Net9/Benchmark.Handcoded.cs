using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net9
    {
        // N01: the elements each chain would produce, computed from the data it was built over
        [Benchmark]
        public int N01a_Handcoded()
        {
            foreach (var x in _ints1000)
                return x;
            throw new InvalidOperationException();
        }

        [Benchmark] public int N01b_Handcoded() => 42 * 2;

        [Benchmark]
        public int N01c_Handcoded()
        {
            var count = 0;
            for (var i = 999; i >= 0; --i)
                ++count;
            return count;
        }

        [Benchmark]
        public int N01d_Handcoded() =>
            _list1000.Count == 0
                ? 0
                : _list1000[999] * 2;

        [Benchmark] public int N01e_Handcoded() => _list1000[500 + 99];

        [Benchmark] public int N01f_Handcoded() => 0;

        [Benchmark]
        public bool N02a_Handcoded()
        {
            foreach (var i in Iterations(100))
            {
                if (i % 2 == 0)
                    return true;
            }
            return false;
        }

        [Benchmark]
        public bool N02b_Handcoded()
        {
            foreach (var i in _ints100)
            {
                if (i % 2 == 0)
                    return true;
            }
            return false;
        }

        // N03: building an operator over a source does no work to write by hand

        [Benchmark]
        public double N04_Handcoded()
        {
            if (_ints1000.Length == 0)
                return 0;

            var sum = 0L;
            foreach (var x in _ints1000)
                sum += x;
            return (double)sum / _ints1000.Length;
        }

        [Benchmark]
        public int N05_Handcoded()
        {
            var count = 0;
            for (var i = 0; i < _ints1000.Length; i += 10)
                count += Math.Min(10, _ints1000.Length - i);
            return count;
        }

        [Benchmark]
        public bool N06a_Handcoded()
        {
            foreach (var i in _list1000AsEnumerable)
            {
                if (i == 1000)
                    return true;
            }
            return false;
        }

        [Benchmark]
        public bool N06b_Handcoded()
        {
            foreach (var i in _list1000AsEnumerable)
            {
                if (!(i >= 0))
                    return false;
            }
            return true;
        }

        [Benchmark]
        public int N06c_Handcoded()
        {
            var count = 0;
            foreach (var i in _list1000AsEnumerable)
            {
                if (i == 0)
                    ++count;
            }
            return count;
        }

        [Benchmark]
        public int N06d_Handcoded()
        {
            foreach (var i in _list1000AsEnumerable)
            {
                if (i == 999)
                    return i;
            }
            throw new InvalidOperationException();
        }

        [Benchmark]
        public int N06e_Handcoded()
        {
            var found = false;
            var result = 0;
            foreach (var i in _list1000AsEnumerable)
            {
                if (i == 0)
                {
                    if (found)
                        throw new InvalidOperationException();
                    found = true;
                    result = i;
                }
            }
            if (!found)
                throw new InvalidOperationException();
            return result;
        }

        [Benchmark]
        public bool N07_Handcoded()
        {
            using var first = _source10000.GetEnumerator();
            using var second = _source10000List.GetEnumerator();
            while (first.MoveNext())
            {
                if (!second.MoveNext() || first.Current != second.Current)
                    return false;
            }
            return !second.MoveNext();
        }

        [Benchmark] public string[] N08a_Handcoded() => new List<string>(StringItems(1)).ToArray();
        [Benchmark] public string[] N08b_Handcoded() => new List<string>(StringItems(8)).ToArray();
        [Benchmark] public string[] N08c_Handcoded() => new List<string>(StringItems(500)).ToArray();

        [Benchmark]
        public List<int> N09a_Handcoded()
        {
            var result = new List<int>();
            foreach (var i in IntItems(8))
                result.Add(i);
            return result;
        }

        [Benchmark]
        public List<int> N09b_Handcoded()
        {
            var result = new List<int>();
            foreach (var i in IntItems(8))
            {
                if (true)
                    result.Add(i);
            }
            return result;
        }

        [Benchmark]
        public int[] N10_Handcoded()
        {
            var array = new List<int>(IntItems(1)).ToArray();
            Array.Sort(array);
            return array;
        }

        [Benchmark]
        public Dictionary<int, KeyValuePair<int, int>> N11_Handcoded()
        {
            var result = new Dictionary<int, KeyValuePair<int, int>>();
            foreach (var pair in Pairs(10_000))
                result.Add(pair.Key, pair);
            return result;
        }

        [Benchmark]
        public Int128 N12_Handcoded()
        {
            var max = _int128s[0];
            for (var i = 1; i < _int128s.Length; ++i)
            {
                if (_int128s[i] > max)
                    max = _int128s[i];
            }
            return max;
        }

        [Benchmark]
        public int N13_Handcoded()
        {
            var max = _ints10000[0];
            for (var i = 1; i < _ints10000.Length; ++i)
            {
                if (_ints10000[i] > max)
                    max = _ints10000[i];
            }
            return max;
        }

        [Benchmark] public int N14a_Handcoded() => MaxOf(_shuffled);
        [Benchmark] public int N14b_Handcoded() => MaxOf(_shuffled);

        [Benchmark]
        public int N15_Handcoded()
        {
            var sorted = new List<DayOfWeek>(_days).ToArray();
            Array.Sort(sorted);
            var sum = 0;
            foreach (var dow in sorted)
                sum += (int)dow;
            return sum;
        }

        [Benchmark]
        public int N16_Handcoded()
        {
            var sorted = new List<int>(_data1000).ToArray();
            Array.Sort(sorted);
            var sum = 0;
            foreach (var value in sorted)
                sum += value;
            return sum;
        }

        [Benchmark]
        public int N17_Handcoded()
        {
            var sum = 0;
            foreach (var group in _handLookup.Values)
            {
                foreach (var item in group)
                    sum += item.Length;
            }
            return sum;
        }

        private static int MaxOf(List<int> values)
        {
            var max = values[0];
            for (var i = 1; i < values.Count; ++i)
            {
                if (values[i] > max)
                    max = values[i];
            }
            return max;
        }
    }
}
