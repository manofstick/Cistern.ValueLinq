using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net7
    {
        [Benchmark] public int N01a_Handcoded() => Min(_source4);
        [Benchmark] public int N01b_Handcoded() => Min(_source1024);

        [Benchmark] public int N02a_Handcoded() => Max(_source4);
        [Benchmark] public int N02b_Handcoded() => Max(_source1024);

        [Benchmark]
        public float N03a_Handcoded()
        {
            double sum = 0;
            foreach (var x in _floats)
                sum += x;
            return (float)sum;
        }

        [Benchmark]
        public float N03b_Handcoded()
        {
            double sum = 0;
            long count = 0;
            foreach (var x in _floats)
            {
                sum += x;
                ++count;
            }
            return (float)(sum / count);
        }

        [Benchmark]
        public float N03c_Handcoded()
        {
            using var e = _floats.GetEnumerator();
            e.MoveNext();
            var min = e.Current;
            while (e.MoveNext())
            {
                if (e.Current < min)
                    min = e.Current;
            }
            return min;
        }

        [Benchmark]
        public float N03d_Handcoded()
        {
            using var e = _floats.GetEnumerator();
            e.MoveNext();
            var max = e.Current;
            while (e.MoveNext())
            {
                if (e.Current > max)
                    max = e.Current;
            }
            return max;
        }

        [Benchmark]
        public int N04a_Handcoded()
        {
            var keys = (int[])_reversed1024.Clone();
            var items = (int[])_reversed1024.Clone();
            Array.Sort(keys, items);
            var sum = 0;
            foreach (var value in items)
                sum += value;
            return sum;
        }

        [Benchmark]
        public int N04b_Handcoded()
        {
            var items = (int[])_reversed1024.Clone();
            Array.Sort(items);
            var sum = 0;
            foreach (var value in items)
                sum += value;
            return sum;
        }

        private static int Min(IEnumerable<int> source)
        {
            using var e = source.GetEnumerator();
            e.MoveNext();
            var min = e.Current;
            while (e.MoveNext())
            {
                if (e.Current < min)
                    min = e.Current;
            }
            return min;
        }

        private static int Max(IEnumerable<int> source)
        {
            using var e = source.GetEnumerator();
            e.MoveNext();
            var max = e.Current;
            while (e.MoveNext())
            {
                if (e.Current > max)
                    max = e.Current;
            }
            return max;
        }
    }
}
