using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B8
    {
        [Benchmark]
        public int B61a_Handcoded()
        {
            var ordered = OrderedByKey();
            return ordered[ordered.Length - 1];
        }

        [Benchmark]
        public int B61b_Handcoded() =>
            _list[_list.Count - 1];

        [Benchmark]
        public int B61c_Handcoded()
        {
            var last = 0;
            var any = false;
            foreach (var x in Data.Iterator(_array))
            {
                last = x;
                any = true;
            }
            if (!any)
                throw new InvalidOperationException();
            return last;
        }

        [Benchmark]
        public int B62a_Handcoded()
        {
            var ordered = OrderedByKey();
            return ordered[0];
        }

        [Benchmark]
        public int B62b_Handcoded() =>
            _list[0];

        [Benchmark]
        public int B62c_Handcoded()
        {
            foreach (var x in Data.Iterator(_array))
                return x;
            throw new InvalidOperationException();
        }

        [Benchmark]
        public int B63a_Handcoded() =>
            _list[_list.Count - 1 - Length / 3];

        [Benchmark]
        public int B63b_Handcoded()
        {
            var buffer = new List<int>();
            foreach (var x in Data.Iterator(_array))
                buffer.Add(x);
            return buffer[buffer.Count - 1 - Length / 3];
        }

        [Benchmark]
        public int B64a_Handcoded() =>
            _list.Count;

        [Benchmark]
        public int B64b_Handcoded()
        {
            var count = 0;
            foreach (var x in Data.Iterator(_array))
                ++count;
            return count;
        }

        [Benchmark]
        public int B64c_Handcoded() =>
            _list.Count;

        [Benchmark]
        public bool B64d_Handcoded() =>
            _set.Contains(_present);

        [Benchmark]
        public bool B64e_Handcoded() =>
            _list.Contains(_present);

        [Benchmark]
        public int[] B65a_Handcoded()
        {
            var buffer = new List<int>();
            foreach (var x in Data.Iterator(_array))
                buffer.Add(x);
            buffer.Reverse();
            return buffer.ToArray();
        }

        [Benchmark]
        public int[] B65b_Handcoded()
        {
            var result = _list.ToArray();
            Array.Reverse(result);
            return result;
        }

        [Benchmark]
        public List<int> B65c_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in Data.Iterator(_array))
                result.Add(x);
            result.Reverse();
            return result;
        }

        // the values of the array in a stable order by key: the index breaks ties
        int[] OrderedByKey()
        {
            var keys = new int[_array.Length];
            var indices = new int[_array.Length];
            for (var i = 0; i < _array.Length; ++i)
            {
                keys[i] = _key(_array[i]);
                indices[i] = i;
            }

            Array.Sort(indices, (a, b) =>
            {
                var compared = keys[a].CompareTo(keys[b]);
                return compared != 0 ? compared : a.CompareTo(b);
            });

            var result = new int[_array.Length];
            for (var i = 0; i < indices.Length; ++i)
                result[i] = _array[indices[i]];
            return result;
        }
    }
}
