using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.Benchmarks.Coverage
{
    partial class AllPredicate
    {
        // the loop a programmer would write for the container actually in hand, which is what the libraries detect
        [Benchmark(Baseline = true)]
        public bool Handcoded() =>
            _data switch
            {
                int[] array => Loop(array),
                List<int> list => Loop(CollectionsMarshal.AsSpan(list)),
                _ => Loop(_data),
            };

        private bool Loop(ReadOnlySpan<int> source)
        {
            foreach (var x in source)
            {
                if (x < 0)
                    return false;
            }
            return true;
        }

        private bool Loop(IEnumerable<int> source)
        {
            foreach (var x in source)
            {
                if (x < 0)
                    return false;
            }
            return true;
        }
    }
}
