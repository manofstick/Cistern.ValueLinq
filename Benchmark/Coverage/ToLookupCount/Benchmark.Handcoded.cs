using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.Benchmarks.Coverage
{
    partial class ToLookupCount
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
            var groups = new Dictionary<int, List<int>>();
            foreach (var x in source)
            {
                var key = x % 100;
                if (!groups.TryGetValue(key, out var group))
                {
                    group = new List<int>();
                    groups.Add(key, group);
                }
                group.Add(x);
            }
            return groups.Count;
        }

        private int Loop(IEnumerable<int> source)
        {
            var groups = new Dictionary<int, List<int>>();
            foreach (var x in source)
            {
                var key = x % 100;
                if (!groups.TryGetValue(key, out var group))
                {
                    group = new List<int>();
                    groups.Add(key, group);
                }
                group.Add(x);
            }
            return groups.Count;
        }
    }
}
