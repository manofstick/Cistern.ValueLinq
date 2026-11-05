using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class A
    {
        [Benchmark]
        public int A01_Handcoded()
        {
            var sum = 0;
            foreach (var x in _array)
                sum += _selector2(_selector(x));
            return sum;
        }

        [Benchmark]
        public int A02_Handcoded()
        {
            var sum = 0;
            foreach (var x in _array)
                if (_predicate(x) && _predicate2(x))
                    sum += x;
            return sum;
        }

        [Benchmark]
        public int A03_Handcoded()
        {
            var sum = 0;
            foreach (var x in _array)
                if (_predicate(x))
                    sum += _selector(x);
            return sum;
        }

        [Benchmark]
        public int A04_Handcoded()
        {
            var sum = 0;
            foreach (var x in _array)
                if (_predicate(x))
                    sum += _selector2(_selector(x));
            return sum;
        }

        [Benchmark]
        public int A05_Handcoded()
        {
            var sum = 0;
            foreach (var o in _objects)
                if (o is string s)
                    sum += _stringLength(s);
            return sum;
        }

        [Benchmark]
        public int A06_Handcoded()
        {
            var sorted = SortedBy(_array, _key);
            return _selector(sorted[0]);
        }

        [Benchmark]
        public int A07_Handcoded()
        {
            var sum = 0;
            for (var i = 0; i < Length; ++i)
                sum += _selector(i);
            return sum;
        }

        [Benchmark]
        public int A08_Handcoded()
        {
            var sum = 0;
            for (var i = 0; i < _array.Length; ++i)
            {
                var value = _selector(_array[i]);
                if (i >= Length / 2)
                    sum += value;
            }
            return sum;
        }

        [Benchmark]
        public int A09_Handcoded()
        {
            var sum = 0;
            var end = Math.Min(_list.Count, Length / 4 + Length / 2);
            for (var i = Length / 4; i < end; ++i)
                sum += _selector(_list[i]);
            return sum;
        }

        [Benchmark]
        public int A10_Handcoded()
        {
            var sum = 0;
            for (var i = Length - 10; i < _array.Length; ++i)
                sum += _array[i];
            return sum;
        }

        [Benchmark]
        public int A11_Handcoded()
        {
            var sum = 0;
            var end = Math.Min(_list.Count, Length / 2);
            for (var i = 0; i < end; ++i)
                sum += _list[i];
            return sum;
        }

        [Benchmark]
        public int A12_Handcoded()
        {
            var sum = 0;
            var end = Math.Min(Math.Min(_list.Count, 100 + Length / 2), 200 + Length / 4);
            for (var i = 200; i < end; ++i)
                sum += _list[i];
            return sum;
        }

        [Benchmark]
        public int A13_Handcoded()
        {
            var sum = 0;
            var skipped = 0;
            var taken = 0;
            foreach (var x in _array)
            {
                if (!_predicate(x))
                    continue;
                if (skipped < 100)
                {
                    ++skipped;
                    continue;
                }
                if (taken == Length / 4)
                    break;
                sum += x;
                ++taken;
            }
            return sum;
        }

        [Benchmark]
        public int[] A14_Handcoded()
        {
            var sorted = SortedBy(_array, _key);
            return sorted[100..110];
        }

        [Benchmark]
        public int A15_Handcoded()
        {
            var sum = 0;
            for (var i = 0; i < Length; ++i)
            {
                var value = _selector(i);
                if (i >= Length - 10)
                    sum += value;
            }
            return sum;
        }

        [Benchmark]
        public int[] A16_Handcoded()
        {
            var sorted = SortedBy(_array, _key);
            return sorted[100..110];
        }

        [Benchmark]
        public int[] A17_Handcoded()
        {
            return SortedBy(_array, _key, _key2);
        }

        [Benchmark]
        public int[] A18_Handcoded()
        {
            var all = new List<int>();
            foreach (var x in _iterator)
                all.Add(x);
            for (var i = all.Count - 1; i > 0; --i)
            {
                var j = Random.Shared.Next(i + 1);
                (all[i], all[j]) = (all[j], all[i]);
            }
            var result = new int[Math.Min(10, all.Count)];
            all.CopyTo(0, result, 0, result.Length);
            return result;
        }

        [Benchmark]
        public int A19_Handcoded()
        {
            var sum = 0;
            foreach (var part in _parts)
                foreach (var x in part)
                    sum += x;
            return sum;
        }

        [Benchmark]
        public int A20_Handcoded()
        {
            var sum = 5 + 6 + 7 + 8;
            foreach (var x in _array)
                sum += x;
            sum += 1 + 2 + 3 + 4;
            return sum;
        }

        [Benchmark]
        public int A21_Handcoded()
        {
            var set = new HashSet<int>();
            foreach (var x in _parts[0])
                set.Add(x);
            foreach (var x in _parts[1])
                set.Add(x);
            foreach (var x in _parts[2])
                set.Add(x);
            return set.Count;
        }

        [Benchmark]
        public int A22_Handcoded()
        {
            var sum = 0;
            for (var i = Length - 10; i <= Length - 1; ++i)
                sum += i;
            return sum;
        }

        [Benchmark]
        public int[] A23_Handcoded()
        {
            var sorted = new int[_array.Length];
            _array.CopyTo(sorted, 0);
            Array.Sort(sorted);
            return sorted;
        }

        [Benchmark]
        public int A24_Handcoded()
        {
            var sum = 0;
            foreach (var x in _list)
                sum += x;
            return sum;
        }

        [Benchmark]
        public int[] A25_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _empty)
            {
                var value = _selector(x);
                if (_predicate(value))
                    result.Add(value);
            }
            return result.ToArray();
        }

        // A stable sort by the first key, then the second where given: indices sorted by key with the index as the last tiebreak.
        int[] SortedBy(int[] source, Func<int, int> key, Func<int, int> thenKey = null)
        {
            var keys = new int[source.Length];
            var thenKeys = new int[source.Length];
            var indices = new int[source.Length];
            for (var i = 0; i < source.Length; ++i)
            {
                keys[i] = key(source[i]);
                thenKeys[i] = thenKey is null ? 0 : thenKey(source[i]);
                indices[i] = i;
            }

            Array.Sort(indices, (a, b) =>
            {
                var c = keys[a].CompareTo(keys[b]);
                if (c == 0)
                    c = thenKeys[a].CompareTo(thenKeys[b]);
                return c != 0 ? c : a.CompareTo(b);
            });

            var result = new int[source.Length];
            for (var i = 0; i < result.Length; ++i)
                result[i] = source[indices[i]];
            return result;
        }
    }
}
