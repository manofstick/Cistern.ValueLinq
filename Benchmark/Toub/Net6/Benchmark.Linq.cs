using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net6
    {
        IEnumerable<int> _linqZip1;
        IEnumerable<int> _linqZip2;
        IEnumerable<int> _linqZip3;

        partial void BuildLinq()
        {
            _linqZip1 = Enumerable.Range(0, 1_000);
            _linqZip2 = Enumerable.Range(0, 1_000);
            _linqZip3 = Enumerable.Range(0, 1_000);
        }

        [Benchmark] public int N01_Linq() => _data.Distinct().Count();

        [Benchmark] public bool N02_Linq() => _e1.SequenceEqual(_e2);

        [Benchmark] public TimeSpan N03a_Linq() => _values.Max();
        [Benchmark] public TimeSpan N03b_Linq() => _values.Min();

        [Benchmark]
        public int N04a_Linq()
        {
            int sum = 0;
            foreach ((int, int, int) values in _linqZip1.Zip(_linqZip2).Zip(_linqZip3, (x, y) => (x.First, x.Second, y)))
                sum += values.Item1 + values.Item2 + values.Item3;
            return sum;
        }

        [Benchmark]
        public int N04b_Linq()
        {
            int sum = 0;
            foreach ((int, int, int) values in _linqZip1.Zip(_linqZip2, _linqZip3))
                sum += values.Item1 + values.Item2 + values.Item3;
            return sum;
        }

        [Benchmark]
        public int N05a_Linq()
        {
            int sum = 0;
            foreach (int i in _iterator.Skip(1000).Take(10))
                sum += i;
            return sum;
        }

        [Benchmark]
        public int N05b_Linq()
        {
            int sum = 0;
            foreach (int i in _iterator.Take(1000..1010))
                sum += i;
            return sum;
        }
    }
}
