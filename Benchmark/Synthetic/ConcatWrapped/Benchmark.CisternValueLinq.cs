using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    partial class ConcatWrapped
    {
        private static IEnumerable<int> BuildCisternValueLinq(int[][] parts)
        {
            IEnumerable<int> chain = Array.Empty<int>();
            foreach (var part in parts)
                chain = new Forwarding(chain).Concat(part);
            return chain;
        }

        [Benchmark, BenchmarkCategory("Sum")]
        public int CisternValueLinq_Sum() =>
            _cisternValueLinq
            .Sum();

        [Benchmark, BenchmarkCategory("Count")]
        public int CisternValueLinq_Count() =>
            _cisternValueLinq
            .Count();

        [Benchmark, BenchmarkCategory("Foreach")]
        public int CisternValueLinq_Foreach()
        {
            var sum = 0;
            foreach (var x in _cisternValueLinq)
                sum += x;
            return sum;
        }
    }
}
