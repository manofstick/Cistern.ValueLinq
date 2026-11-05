using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net6
    {
        IEnumerable<int> _cisternZip1;
        IEnumerable<int> _cisternZip2;
        IEnumerable<int> _cisternZip3;

        // the sources as their own types, for the Typed rows
        string[] _cisternData;
        int[] _cisternE1;
        int[] _cisternE2;

        partial void BuildCisternValueLinq()
        {
            _cisternZip1 = Enumerable.Range(0, 1_000);
            _cisternZip2 = Enumerable.Range(0, 1_000);
            _cisternZip3 = Enumerable.Range(0, 1_000);

            _cisternData = (string[])_data;
            _cisternE1 = (int[])_e1;
            _cisternE2 = (int[])_e2;
        }

        [Benchmark] public int N01_CisternValueLinq() => _data.Distinct().Count();

        [Benchmark] public bool N02_CisternValueLinq() => _e1.SequenceEqual(_e2);

        [Benchmark] public int N01_CisternValueLinqTyped() => _cisternData.Distinct().Count();

        [Benchmark] public bool N02_CisternValueLinqTyped() => _cisternE1.OfArray().SequenceEqual(_cisternE2.OfArray());

        [Benchmark] public TimeSpan N03a_CisternValueLinq() => _values.Max();
        [Benchmark] public TimeSpan N03b_CisternValueLinq() => _values.Min();

        [Benchmark]
        public int N04a_CisternValueLinq()
        {
            int sum = 0;
            foreach ((int, int, int) values in _cisternZip1.Zip(_cisternZip2).Zip(_cisternZip3, (x, y) => (x.First, x.Second, y)))
                sum += values.Item1 + values.Item2 + values.Item3;
            return sum;
        }

        [Benchmark]
        public int N04b_CisternValueLinq()
        {
            int sum = 0;
            foreach ((int, int, int) values in _cisternZip1.Zip(_cisternZip2, _cisternZip3))
                sum += values.Item1 + values.Item2 + values.Item3;
            return sum;
        }

        // built in the method, as ZLinq's is
        [Benchmark]
        public int N04a_CisternValueLinqInline()
        {
            int sum = 0;
            foreach ((int, int, int) values in Enumerable.Range(0, 1_000).Zip(Enumerable.Range(0, 1_000)).Zip(Enumerable.Range(0, 1_000), (x, y) => (x.First, x.Second, y)))
                sum += values.Item1 + values.Item2 + values.Item3;
            return sum;
        }

        // built in the method, as ZLinq's is
        [Benchmark]
        public int N04b_CisternValueLinqInline()
        {
            int sum = 0;
            foreach ((int, int, int) values in Enumerable.Range(0, 1_000).Zip(Enumerable.Range(0, 1_000), Enumerable.Range(0, 1_000)))
                sum += values.Item1 + values.Item2 + values.Item3;
            return sum;
        }

        [Benchmark]
        public int N05a_CisternValueLinq()
        {
            int sum = 0;
            foreach (int i in _iterator.Skip(1000).Take(10))
                sum += i;
            return sum;
        }

        [Benchmark]
        public int N05b_CisternValueLinq()
        {
            int sum = 0;
            foreach (int i in _iterator.Take(1000..1010))
                sum += i;
            return sum;
        }
    }
}
