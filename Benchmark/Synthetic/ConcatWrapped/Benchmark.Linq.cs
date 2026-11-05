using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class ConcatWrapped
    {
        private static IEnumerable<int> BuildLinq(int[][] parts)
        {
            IEnumerable<int> chain = Array.Empty<int>();
            foreach (var part in parts)
                chain = new Forwarding(chain).Concat(part);
            return chain;
        }

        [Benchmark, BenchmarkCategory("Sum")]
        public int Linq_Sum() =>
            _linq
            .Sum();

        [Benchmark, BenchmarkCategory("Count")]
        public int Linq_Count() =>
            _linq
            .Count();

        [Benchmark, BenchmarkCategory("Foreach")]
        public int Linq_Foreach()
        {
            var sum = 0;
            foreach (var x in _linq)
                sum += x;
            return sum;
        }
    }
}
