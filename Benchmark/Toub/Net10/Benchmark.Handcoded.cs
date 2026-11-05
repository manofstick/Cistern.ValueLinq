using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net10
    {
        [Benchmark]
        public bool N01a_Handcoded() =>
            Contains(_source, 999) || 100 == 999;

        [Benchmark]
        public bool N01b_Handcoded() =>
            Contains(_source, 999) || Contains(_source, 999);

        [Benchmark]
        public bool N01c_Handcoded()
        {
            using var e = _source.GetEnumerator();
            if (!e.MoveNext())
                return 42 == 999;
            do
            {
                if (e.Current == 999)
                    return true;
            }
            while (e.MoveNext());
            return false;
        }

        [Benchmark]
        public bool N01d_Handcoded() =>
            Contains(_source, 999);

        [Benchmark]
        public bool N01e_Handcoded() =>
            Contains(_source, 999);

        [Benchmark]
        public bool N01f_Handcoded() =>
            Contains(_source, 999);

        [Benchmark]
        public bool N01g_Handcoded() =>
            Contains(_source, 999) || Contains(_source, 999);

        [Benchmark]
        public bool N01h_Handcoded()
        {
            foreach (var _ in _source)
            {
                if (Contains(_source, 999))
                    return true;
            }
            return false;
        }

        [Benchmark]
        public bool N01i_Handcoded() =>
            Contains(_source, 999);

        [Benchmark]
        public List<short> N02_Handcoded()
        {
            _values.Clear();
            for (short i = 42; i <= 1042; i++)
                _values.Add(i);
            return _values;
        }

        [Benchmark]
        public int[] N03a_Handcoded()
        {
            var buffer = new List<int>();
            foreach (var x in _shuffleSource)
                buffer.Add(x);
            var array = buffer.ToArray();
            Random.Shared.Shuffle(array);
            return array.AsSpan(0, 10).ToArray();
        }

        [Benchmark]
        public bool N04a_Handcoded()
        {
            var buffer = new List<int>();
            foreach (var x in _shuffleSource)
                buffer.Add(x);
            var array = buffer.ToArray();
            Random.Shared.Shuffle(array);
            for (var i = 0; i < 10; ++i)
            {
                if (array[i] == 2000)
                    return true;
            }
            return false;
        }

        [Benchmark]
        public int N05a_Handcoded()
        {
            var innerCounts = new Dictionary<int, int>();
            foreach (var i in Stepped(0, 1000, 3))
            {
                innerCounts.TryGetValue(i, out var matches);
                innerCounts[i] = matches + 1;
            }

            var count = 0;
            foreach (var o in Stepped(0, 1000, 2))
                count += innerCounts.TryGetValue(o, out var matches) ? matches : 1;
            return count;
        }

        [Benchmark]
        public List<string> N06_Handcoded()
        {
            var result = new List<string>();
            var index = 0;
            foreach (var x in _strings)
            {
                if (index >= 400)
                    break;
                if (index >= 200)
                    result.Add(x);
                ++index;
            }
            return result;
        }

        [Benchmark]
        public int N07_Handcoded()
        {
            var sum = 0;
            for (var i = 100; i < 900; ++i)
                sum = checked(sum + _readOnly[i]);
            return sum;
        }

        [Benchmark]
        public bool N08_Handcoded()
        {
            var comparer = EqualityComparer<int>.Default;
            foreach (var x in _data)
            {
                if (comparer.Equals(x, int.MaxValue))
                    return true;
            }
            return false;
        }

        static bool Contains(IEnumerable<int> source, int value)
        {
            foreach (var x in source)
            {
                if (x == value)
                    return true;
            }
            return false;
        }
    }
}
