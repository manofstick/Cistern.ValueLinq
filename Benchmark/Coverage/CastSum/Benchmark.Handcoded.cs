using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.Benchmarks.Coverage
{
    partial class CastSum
    {
        // the loop a programmer would write for the container actually in hand, which is what the libraries detect
        [Benchmark(Baseline = true)]
        public int Handcoded() =>
            _data switch
            {
                object[] array => Loop(array),
                List<object> list => Loop(CollectionsMarshal.AsSpan(list)),
                _ => Loop(_data),
            };

        private int Loop(ReadOnlySpan<object> source)
        {
            var sum = 0;
            foreach (var item in source)
                sum += (int)item;
            return sum;
        }

        private int Loop(IEnumerable<object> source)
        {
            var sum = 0;
            foreach (var item in source)
                sum += (int)item;
            return sum;
        }
    }
}
