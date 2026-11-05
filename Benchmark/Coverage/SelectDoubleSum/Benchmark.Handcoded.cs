using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.Benchmarks.Coverage
{
    partial class SelectDoubleSum
    {
        // the loop a programmer would write for the container actually in hand, which is what the libraries detect
        [Benchmark(Baseline = true)]
        public double Handcoded() =>
            _data switch
            {
                int[] array => Loop(array),
                List<int> list => Loop(CollectionsMarshal.AsSpan(list)),
                _ => Loop(_data),
            };

        private double Loop(ReadOnlySpan<int> source)
        {
            var sum = 0.0;
            foreach (var x in source)
                sum += x * 0.5;
            return sum;
        }

        private double Loop(IEnumerable<int> source)
        {
            var sum = 0.0;
            foreach (var x in source)
                sum += x * 0.5;
            return sum;
        }
    }
}
