using BenchmarkDotNet.Attributes;
using System;
using ZLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net6
    {
        [Benchmark] public int N01_ZLinq() => _data.AsValueEnumerable().Distinct().Count();

        [Benchmark] public bool N02_ZLinq() => _e1.AsValueEnumerable().SequenceEqual(_e2);

        [Benchmark] public TimeSpan N03a_ZLinq() => _values.AsValueEnumerable().Max();
        [Benchmark] public TimeSpan N03b_ZLinq() => _values.AsValueEnumerable().Min();

        [Benchmark]
        public int N04a_ZLinq()
        {
            int sum = 0;
            foreach ((int, int, int) values in ValueEnumerable.Range(0, 1_000).Zip(ValueEnumerable.Range(0, 1_000)).Zip(ValueEnumerable.Range(0, 1_000), (x, y) => (x.First, x.Second, y)))
                sum += values.Item1 + values.Item2 + values.Item3;
            return sum;
        }

        [Benchmark]
        public int N04b_ZLinq()
        {
            int sum = 0;
            foreach ((int, int, int) values in ValueEnumerable.Range(0, 1_000).Zip(ValueEnumerable.Range(0, 1_000), ValueEnumerable.Range(0, 1_000)))
                sum += values.Item1 + values.Item2 + values.Item3;
            return sum;
        }

        [Benchmark]
        public int N05a_ZLinq()
        {
            int sum = 0;
            foreach (int i in _iterator.AsValueEnumerable().Skip(1000).Take(10))
                sum += i;
            return sum;
        }

        [Benchmark]
        public int N05b_ZLinq()
        {
            int sum = 0;
            foreach (int i in _iterator.AsValueEnumerable().Take(1000..1010))
                sum += i;
            return sum;
        }
    }
}
