using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class NetCore20
    {
        IEnumerable<int> _handcodedZeroToTen;
        IEnumerable<int> _handcodedTenMillionToZero;
        IEnumerable<int> _handcodedHundredMillion;

        partial void BuildHandcoded()
        {
            _handcodedZeroToTen = Ascending(10);
            _handcodedTenMillionToZero = Descending(10_000_000);
            _handcodedHundredMillion = Ascending(100_000_000);
        }

        private static IEnumerable<int> Ascending(int count)
        {
            for (var i = 0; i < count; ++i)
                yield return i;
        }

        private static IEnumerable<int> Descending(int count)
        {
            for (var i = count - 1; i >= 0; --i)
                yield return i;
        }

        [Benchmark]
        public int N01_Handcoded()
        {
            int sum = 0;
            for (var part = 0; part <= 10_000; ++part)
            {
                foreach (int i in _handcodedZeroToTen)
                    sum += i;
            }
            return sum;
        }

        [Benchmark]
        public int N02_Handcoded()
        {
            var sorted = new List<int>(_handcodedTenMillionToZero).ToArray();
            Array.Sort(sorted);
            return sorted[4];
        }

        [Benchmark]
        public List<int> N03_Handcoded()
        {
            var list = new List<int>();
            foreach (var x in _zeroToTenMillion)
                list.Add(x);
            return list;
        }

        [Benchmark] public int[] N04_Handcoded() => new List<int>(_handcodedHundredMillion).ToArray();
    }
}
