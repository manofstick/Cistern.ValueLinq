using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B5
    {
        [Benchmark]
        public int B34a_Handcoded() =>
            SortedByKey(ToArray(Data.Iterator(_values)), _key, false)[0];

        [Benchmark]
        public int B34b_Handcoded() =>
            SortedByKey(ToArray(Data.Iterator(_values)), _key, true)[0];

        [Benchmark]
        public int B34c_Handcoded() =>
            SortedByKey(ToArray(Data.Iterator(_values)), _key, false)[0];

        [Benchmark]
        public int B34d_Handcoded() =>
            SortedByKey(ToArray(Data.Iterator(_values)), _key, false)[0];

        [Benchmark]
        public int B35a_Handcoded()
        {
            var sorted = SortedByKey(ToArray(Data.Iterator(_values)), _key, false);
            return sorted[sorted.Length - 1];
        }

        [Benchmark]
        public int B35b_Handcoded()
        {
            var sorted = SortedByKey(ToArray(Data.Iterator(_values)), _key, false);
            return sorted[sorted.Length - 1];
        }

        [Benchmark]
        public int B36a_Handcoded() =>
            SortedByTwoKeys(_values, _tiedKey, _key)[0];

        [Benchmark]
        public int B36b_Handcoded()
        {
            var sorted = SortedByTwoKeys(_values, _tiedKey, _key);
            return sorted[sorted.Length - 1];
        }

        [Benchmark]
        public int B37_Handcoded()
        {
            var sorted = SortedByKey(_values, _key, false);
            for (var i = sorted.Length - 1; i >= 0; --i)
            {
                if (_predicate(sorted[i]))
                    return sorted[i];
            }
            throw new InvalidOperationException("No element satisfies the condition.");
        }

        [Benchmark]
        public int B38_Handcoded() =>
            SortedByKey(_values, _key, false)[Length / 2];

        [Benchmark]
        public int[] B39a_Handcoded() =>
            Slice(SortedByKey(_values, _key, false), 100, 100);

        [Benchmark]
        public List<int> B39b_Handcoded() =>
            new List<int>(Slice(SortedByKey(_values, _key, false), 100, 100));

        [Benchmark]
        public int[] B39c_Handcoded() =>
            Slice(SortedByKey(_values, _key, false), 100, 100);

        [Benchmark]
        public int B39d_Handcoded()
        {
            var sorted = SortedByKey(_values, _key, false);
            var sum = 0;
            for (var i = 100; i < 200 && i < sorted.Length; ++i)
                sum += sorted[i];
            return sum;
        }

        [Benchmark]
        public int[] B39e_Handcoded() =>
            Slice(SortedByKey(ToArray(Data.Iterator(_values)), _key, false), 100, 100);

        [Benchmark]
        public int[] B39f_Handcoded() =>
            SortedByKey(ToArray(Data.Iterator(_values)), _key, false);

        [Benchmark]
        public int[] B40a_Handcoded() =>
            Slice(SortedByKey(_values, _key, false), 500, 1);

        [Benchmark]
        public int B40b_Handcoded() =>
            SortedByKey(_values, _key, false)[500];

        [Benchmark]
        public int B41_Handcoded()
        {
            var window = Slice(SortedByKey(_values, _key, false), 100, 100);
            return window[window.Length - 1];
        }

        [Benchmark]
        public int B42a_Handcoded() =>
            SortedCopy(_values)[0];

        [Benchmark]
        public int B42b_Handcoded()
        {
            var sorted = SortedCopy(_values);
            return sorted[sorted.Length - 1];
        }

        [Benchmark]
        public int B42c_Handcoded()
        {
            var copy = new List<int>(_list);
            copy.Sort();
            return copy[0];
        }

        [Benchmark]
        public int B42d_Handcoded()
        {
            var copy = new List<int>(_list);
            copy.Sort();
            return copy[copy.Count - 1];
        }

        [Benchmark]
        public int B43a_Handcoded()
        {
            var copy = new List<int>();
            foreach (var x in Data.Iterator(_values))
                copy.Add(x);
            copy.Sort();
            return copy[0];
        }

        [Benchmark]
        public int B43b_Handcoded()
        {
            var copy = new List<int>();
            foreach (var x in Data.Iterator(_values))
                copy.Add(x);
            copy.Sort();
            return copy[copy.Count - 1];
        }

        [Benchmark]
        public int[] B44a_Handcoded() =>
            SortedCopy(_values);

        [Benchmark]
        public List<int> B44b_Handcoded() =>
            new List<int>(SortedCopy(_values));

        [Benchmark]
        public int B45a_Handcoded() =>
            _values.Length;

        [Benchmark]
        public int B45b_Handcoded() =>
            _values.Length;

        [Benchmark]
        public bool B45c_Handcoded() =>
            _values.Length > 0;

        [Benchmark]
        public bool B46_Handcoded()
        {
            foreach (var x in _values)
            {
                if (x == _present)
                    return true;
            }
            return false;
        }

        [Benchmark]
        public int[] B47a_Handcoded() =>
            SortedByKey(_single, _key, false);

        [Benchmark]
        public List<int> B47b_Handcoded() =>
            new List<int>(SortedByKey(_single, _key, false));

        [Benchmark]
        public string[] B48a_Handcoded()
        {
            var keys = new string[_strings.Length];
            var order = new int[_strings.Length];
            for (var i = 0; i < keys.Length; ++i)
            {
                keys[i] = _stringKey(_strings[i]);
                order[i] = i;
            }
            var comparer = Comparer<string>.Default;
            Array.Sort(order, (a, b) =>
            {
                var c = comparer.Compare(keys[a], keys[b]);
                return c != 0 ? c : a.CompareTo(b);
            });
            var result = new string[order.Length];
            for (var i = 0; i < result.Length; ++i)
                result[i] = _strings[order[i]];
            return result;
        }

        [Benchmark]
        public int[] B48b_Handcoded() =>
            SortedByKey(_values, _key, false);

        static int[] ToArray(IEnumerable<int> source)
        {
            var list = new List<int>();
            foreach (var x in source)
                list.Add(x);
            return list.ToArray();
        }

        static int[] SortedCopy(int[] source)
        {
            var copy = new int[source.Length];
            Array.Copy(source, copy, source.Length);
            Array.Sort(copy);
            return copy;
        }

        // stable: equal keys keep their source order
        static int[] SortedByKey(int[] source, Func<int, int> key, bool descending)
        {
            var keys = new int[source.Length];
            var order = new int[source.Length];
            for (var i = 0; i < keys.Length; ++i)
            {
                keys[i] = key(source[i]);
                order[i] = i;
            }
            Array.Sort(order, (a, b) =>
            {
                var c = keys[a].CompareTo(keys[b]);
                if (descending)
                    c = -c;
                return c != 0 ? c : a.CompareTo(b);
            });
            var result = new int[order.Length];
            for (var i = 0; i < result.Length; ++i)
                result[i] = source[order[i]];
            return result;
        }

        static int[] SortedByTwoKeys(int[] source, Func<int, int> first, Func<int, int> second)
        {
            var firsts = new int[source.Length];
            var seconds = new int[source.Length];
            var order = new int[source.Length];
            for (var i = 0; i < order.Length; ++i)
            {
                firsts[i] = first(source[i]);
                seconds[i] = second(source[i]);
                order[i] = i;
            }
            Array.Sort(order, (a, b) =>
            {
                var c = firsts[a].CompareTo(firsts[b]);
                if (c == 0)
                    c = seconds[a].CompareTo(seconds[b]);
                return c != 0 ? c : a.CompareTo(b);
            });
            var result = new int[order.Length];
            for (var i = 0; i < result.Length; ++i)
                result[i] = source[order[i]];
            return result;
        }

        static int[] Slice(int[] source, int skip, int take)
        {
            var count = Math.Max(0, Math.Min(take, source.Length - skip));
            var result = new int[count];
            Array.Copy(source, skip, result, 0, count);
            return result;
        }
    }
}
