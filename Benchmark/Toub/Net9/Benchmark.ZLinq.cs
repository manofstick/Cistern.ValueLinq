using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net9
    {
        // N01, N02: a chain handed out as IEnumerable<T>, which ZLinq's chains are not
        // N03: an operator over an empty source returned as object, which ZLinq's chains cannot be

        [Benchmark]
        public double N04_ZLinq() =>
            _ints1000
            .AsValueEnumerable()
            .DefaultIfEmpty()
            .Average();

        [Benchmark]
        public int N05_ZLinq()
        {
            int count = 0;
            foreach (var chunk in _ints1000.AsValueEnumerable().Chunk(10))
                count += chunk.Length;
            return count;
        }

        [Benchmark] public bool N06a_ZLinq() => _list1000AsEnumerable.AsValueEnumerable().Any(i => i == 1000);
        [Benchmark] public bool N06b_ZLinq() => _list1000AsEnumerable.AsValueEnumerable().All(i => i >= 0);
        [Benchmark] public int N06c_ZLinq() => _list1000AsEnumerable.AsValueEnumerable().Count(i => i == 0);
        [Benchmark] public int N06d_ZLinq() => _list1000AsEnumerable.AsValueEnumerable().First(i => i == 999);
        [Benchmark] public int N06e_ZLinq() => _list1000AsEnumerable.AsValueEnumerable().Single(i => i == 0);

        [Benchmark] public bool N07_ZLinq() => _source10000.AsValueEnumerable().SequenceEqual(_source10000List);

        [Benchmark] public string[] N08a_ZLinq() => StringItems(1).AsValueEnumerable().ToArray();
        [Benchmark] public string[] N08b_ZLinq() => StringItems(8).AsValueEnumerable().ToArray();
        [Benchmark] public string[] N08c_ZLinq() => StringItems(500).AsValueEnumerable().ToArray();

        [Benchmark] public List<int> N09a_ZLinq() => IntItems(8).AsValueEnumerable().Select(i => i).ToList();
        [Benchmark] public List<int> N09b_ZLinq() => IntItems(8).AsValueEnumerable().Where(i => true).Select(i => i).ToList();

        [Benchmark] public int[] N10_ZLinq() => IntItems(1).AsValueEnumerable().OrderBy(x => x).ToArray();

        // a ZLinq chain is a ref struct with no IEnumerable<T> form, so it is built here
        [Benchmark] public Dictionary<int, KeyValuePair<int, int>> N11_ZLinq() => ValueEnumerable.Range(0, 10_000).Select(x => new KeyValuePair<int, int>(x, x)).ToDictionary(x => x.Key);

        [Benchmark] public Int128 N12_ZLinq() => _int128s.AsValueEnumerable().Max();

        [Benchmark] public int N13_ZLinq() => _ints10000.AsValueEnumerable().Max();

        [Benchmark] public int N14a_ZLinq() => _shuffled.AsValueEnumerable().OrderBy(x => x).Last();
        [Benchmark] public int N14b_ZLinq() => _shuffled.AsValueEnumerable().Order().Last();

        [Benchmark]
        public int N15_ZLinq()
        {
            int sum = 0;
            foreach (DayOfWeek dow in _days.AsValueEnumerable().Order())
                sum += (int)dow;
            return sum;
        }

        [Benchmark]
        public int N16_ZLinq()
        {
            int sum = 0;
            foreach (int value in _data1000.AsValueEnumerable().OrderBy(i => i))
                sum += value;
            return sum;
        }

        [Benchmark]
        public int N17_ZLinq()
        {
            int sum = 0;
            foreach (System.Linq.IGrouping<int, string> group in _zlinqLookup)
            {
                foreach (string item in group)
                    sum += item.Length;
            }
            return sum;
        }

        System.Linq.ILookup<int, string> _zlinqLookup;

        partial void BuildZLinq()
        {
            _zlinqLookup = _lookupSource.AsValueEnumerable().ToLookup(s => s.Length);
        }
    }
}
