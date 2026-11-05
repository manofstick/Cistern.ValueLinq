using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.Benchmarks.Coverage
{
    partial class ConcatSum
    {
        // the loop a programmer would write for the container actually in hand, which is what the libraries detect
        [Benchmark(Baseline = true)]
        public int Handcoded() =>
            _data switch
            {
                int[] array => Loop(array),
                List<int> list => Loop(CollectionsMarshal.AsSpan(list)),
                _ => Loop(_data),
            };

        private int Loop(ReadOnlySpan<int> source)
        {
            var sum = 0;
            foreach (var x in source)
                sum += x;
            foreach (var x in _other)
                sum += x;
            return sum;
        }

        private int Loop(IEnumerable<int> source)
        {
            var sum = 0;
            foreach (var x in source)
                sum += x;
            foreach (var x in _other)
                sum += x;
            return sum;
        }
    }
}
