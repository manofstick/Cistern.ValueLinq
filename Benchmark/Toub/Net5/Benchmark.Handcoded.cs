using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net5
    {
        [Benchmark]
        public int N01_Handcoded()
        {
            var keys = (int[])_array.Clone();
            var items = (int[])_array.Clone();
            Array.Sort(keys, items);
            var sum = 0;
            foreach (var value in items)
                sum += value;
            return sum;
        }

        // the source's length is not known from its declared type, so the last five are held back as it is read
        [Benchmark]
        public int N02_Handcoded()
        {
            var held = new Queue<int>(6);
            var sum = 0;
            foreach (var x in _data)
            {
                held.Enqueue(x);
                if (held.Count > 5)
                    sum = checked(sum + held.Dequeue());
            }
            return sum;
        }
    }
}
