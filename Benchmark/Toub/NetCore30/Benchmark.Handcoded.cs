using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class NetCore30
    {
        IEnumerable<int> _handcodedEnumerable;

        partial void BuildHandcoded()
        {
            _handcodedEnumerable = Identity(_zeros);
        }

        private static IEnumerable<int> Identity(int[] source)
        {
            foreach (var x in source)
                yield return x;
        }

        // the source's length is not known from its declared type, so the last ten are kept as it is read
        [Benchmark]
        public int N01_Handcoded()
        {
            var last = new Queue<int>(11);
            foreach (var x in _handcodedEnumerable)
            {
                last.Enqueue(x);
                if (last.Count > 10)
                    last.Dequeue();
            }
            var sum = 0;
            foreach (var x in last)
                sum = checked(sum + x);
            return sum;
        }

        [Benchmark]
        public int[] N02_Handcoded()
        {
            var result = new int[100];
            for (var i = 0; i < result.Length; ++i)
                result[i] = i * 2;
            return result;
        }

        [Benchmark] public int[] N03_Handcoded() => Array.Empty<int>();
    }
}
