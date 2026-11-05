using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.Benchmarks.Coverage
{
    partial class ZipSum
    {
        // the loop a programmer would write for the container actually in hand, which is what the libraries detect
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
            var sum = 0L;
            var i = 0;
            foreach (var x in source)
            {
                if (i >= _other.Length)
                    break;
                sum += (long)x * _other[i];
                ++i;
            }
            return sum;
        }

        private long Loop(IEnumerable<int> source)
        {
            var sum = 0L;
            var i = 0;
            foreach (var x in source)
            {
                if (i >= _other.Length)
                    break;
                sum += (long)x * _other[i];
                ++i;
            }
            return sum;
        }
    }
}
