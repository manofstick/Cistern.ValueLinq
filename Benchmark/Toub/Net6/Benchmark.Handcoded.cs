using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net6
    {
        IEnumerable<int> _handcodedZip1 = Range(1_000);
        IEnumerable<int> _handcodedZip2 = Range(1_000);
        IEnumerable<int> _handcodedZip3 = Range(1_000);

        [Benchmark]
        public int N01_Handcoded()
        {
            var set = new HashSet<string>();
            foreach (var x in _data)
                set.Add(x);
            return set.Count;
        }

        [Benchmark]
        public bool N02_Handcoded()
        {
            using var e1 = _e1.GetEnumerator();
            using var e2 = _e2.GetEnumerator();
            while (e1.MoveNext())
            {
                if (!e2.MoveNext() || e1.Current != e2.Current)
                    return false;
            }
            return !e2.MoveNext();
        }

        [Benchmark]
        public TimeSpan N03a_Handcoded()
        {
            var max = _values[0];
            for (var i = 1; i < _values.Length; ++i)
            {
                if (_values[i] > max)
                    max = _values[i];
            }
            return max;
        }

        [Benchmark]
        public TimeSpan N03b_Handcoded()
        {
            var min = _values[0];
            for (var i = 1; i < _values.Length; ++i)
            {
                if (_values[i] < min)
                    min = _values[i];
            }
            return min;
        }

        [Benchmark] public int N04a_Handcoded() => Zip3();
        [Benchmark] public int N04b_Handcoded() => Zip3();

        [Benchmark] public int N05a_Handcoded() => Slice();
        [Benchmark] public int N05b_Handcoded() => Slice();

        private int Zip3()
        {
            using var e1 = _handcodedZip1.GetEnumerator();
            using var e2 = _handcodedZip2.GetEnumerator();
            using var e3 = _handcodedZip3.GetEnumerator();
            int sum = 0;
            while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
                sum += e1.Current + e2.Current + e3.Current;
            return sum;
        }

        private int Slice()
        {
            int index = 0;
            int sum = 0;
            foreach (int i in _iterator)
            {
                if (index >= 1010)
                    break;
                if (index >= 1000)
                    sum += i;
                ++index;
            }
            return sum;
        }
    }
}
