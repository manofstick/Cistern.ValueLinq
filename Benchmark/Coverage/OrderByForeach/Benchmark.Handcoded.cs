using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.Benchmarks.Coverage
{
    partial class OrderByForeach
    {
        // the loop a programmer would write for the container actually in hand, which is what the libraries detect;
        // the hash depends on the order, so each library's is checked against it
        [Benchmark(Baseline = true)]
        public long Handcoded() =>
            _data switch
            {
                int[] array => Loop(array),
                List<int> list => Loop(CollectionsMarshal.AsSpan(list)),
                _ => Loop(_data),
            };

        private long Loop(ReadOnlySpan<int> source)
        {
            var sorted = source.ToArray();
            Array.Sort(sorted);
            return Hash(sorted);
        }

        private long Loop(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var x in source)
                items.Add(x);
            var sorted = items.ToArray();
            Array.Sort(sorted);
            return Hash(sorted);
        }

        private static long Hash(int[] sorted)
        {
            var hash = 0L;
            foreach (var x in sorted)
                hash = hash * 31 + x;
            return hash;
        }
    }
}
