using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B9
    {
        [Benchmark]
        public int B66a_Handcoded() =>
            _list[Random.Shared.Next(_list.Count)];

        [Benchmark]
        public int B66b_Handcoded()
        {
            var buffer = new List<int>();
            foreach (var x in Data.Iterator(_array))
                buffer.Add(x);
            return buffer[Random.Shared.Next(buffer.Count)];
        }

        [Benchmark]
        public int B66c_Handcoded() =>
            _list[Random.Shared.Next(_list.Count)];

        [Benchmark]
        public int B66d_Handcoded() =>
            _list[Random.Shared.Next(_list.Count)];

        [Benchmark]
        public int[] B67a_Handcoded()
        {
            var buffer = new List<int>();
            foreach (var x in Data.Iterator(_array))
                buffer.Add(x);
            return ShuffledPrefix(buffer.ToArray(), 10);
        }

        [Benchmark]
        public List<int> B67b_Handcoded() =>
            new List<int>(ShuffledPrefix(_list.ToArray(), 10));

        [Benchmark]
        public int B67c_Handcoded()
        {
            var sum = 0;
            foreach (var x in ShuffledPrefix((int[])_array.Clone(), 10))
                sum += x;
            return sum;
        }

        [Benchmark]
        public int B68a_Handcoded() =>
            _list.Count;

        [Benchmark]
        public int B68b_Handcoded() =>
            _list.Count;

        [Benchmark]
        public int B68c_Handcoded() =>
            Math.Min(10, _list.Count);

        [Benchmark]
        public bool B69a_Handcoded() =>
            _list.Contains(_present);

        [Benchmark]
        public bool B70a_Handcoded()
        {
            foreach (var x in ShuffledPrefix((int[])_array.Clone(), 10))
                if (x == _absent)
                    return true;
            return false;
        }

        [Benchmark]
        public int[] B71a_Handcoded()
        {
            var result = (int[])_array.Clone();
            Random.Shared.Shuffle(result);
            return result;
        }

        [Benchmark]
        public List<int> B71b_Handcoded()
        {
            var result = _list.ToArray();
            Random.Shared.Shuffle(result);
            return new List<int>(result);
        }

        // the first count elements of a shuffle of the values: a Fisher-Yates shuffle stopped after count steps
        static int[] ShuffledPrefix(int[] values, int count)
        {
            count = Math.Min(count, values.Length);
            for (var i = 0; i < count; ++i)
            {
                var j = Random.Shared.Next(i, values.Length);
                (values[i], values[j]) = (values[j], values[i]);
            }

            var result = new int[count];
            Array.Copy(values, result, count);
            return result;
        }
    }
}
