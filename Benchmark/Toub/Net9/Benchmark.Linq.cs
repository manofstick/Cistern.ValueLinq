using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net9
    {
        IEnumerable<int> _linqArrayDistinct;
        IEnumerable<int> _linqAppendSelect;
        IEnumerable<int> _linqRangeReverse;
        IEnumerable<int> _linqListDefaultIfEmptySelect;
        IEnumerable<int> _linqListSkipTake;
        IEnumerable<int> _linqRangeUnion;
        IEnumerable<int> _linqIteratorWhereSelect;
        IEnumerable<int> _linqArrayWhereSelect;
        ILookup<int, string> _linqLookup;
        IEnumerable<KeyValuePair<int, int>> _linqPairs;

        partial void BuildLinq()
        {
            _linqArrayDistinct = Enumerable.Range(0, 1000).ToArray().Distinct();
            _linqAppendSelect = Enumerable.Range(0, 1000).ToArray().Append(42).Select(i => i * 2);
            _linqRangeReverse = Enumerable.Range(0, 1000).Reverse();
            _linqListDefaultIfEmptySelect = Enumerable.Range(0, 1000).ToList().DefaultIfEmpty().Select(i => i * 2);
            _linqListSkipTake = Enumerable.Range(0, 1000).ToList().Skip(500).Take(100);
            _linqRangeUnion = Enumerable.Range(0, 1000).Union(Enumerable.Range(500, 1000));
            _linqIteratorWhereSelect = Iterations(100).Where(i => i % 2 == 0).Select(i => i);
            _linqArrayWhereSelect = Enumerable.Range(0, 100).ToArray().Where(i => i % 2 == 0).Select(i => i);
            _linqLookup = _lookupSource.ToLookup(s => s.Length);
            _linqPairs = Enumerable.Range(0, 10_000).Select(x => new KeyValuePair<int, int>(x, x));
        }

        [Benchmark] public int N01a_Linq() => _linqArrayDistinct.First();
        [Benchmark] public int N01b_Linq() => _linqAppendSelect.Last();
        [Benchmark] public int N01c_Linq() => _linqRangeReverse.Count();
        [Benchmark] public int N01d_Linq() => _linqListDefaultIfEmptySelect.ElementAt(999);
        [Benchmark] public int N01e_Linq() => _linqListSkipTake.ElementAt(99);
        [Benchmark] public int N01f_Linq() => _linqRangeUnion.First();

        [Benchmark] public bool N02a_Linq() => _linqIteratorWhereSelect.Any();
        [Benchmark] public bool N02b_Linq() => _linqArrayWhereSelect.Any();

        [Benchmark] public object N03a_Linq() => _empty.Chunk(10);
        [Benchmark] public object N03b_Linq() => _empty.Distinct();
        [Benchmark] public object N03c_Linq() => _empty.GroupJoin(_empty, i => i, i => i, (i, j) => i);
        [Benchmark] public object N03d_Linq() => _empty.Join(_empty, i => i, i => i, (i, j) => i);
        [Benchmark] public object N03e_Linq() => _empty.ToLookup(i => i);
        [Benchmark] public object N03f_Linq() => _empty.Reverse();
        [Benchmark] public object N03g_Linq() => _empty.Select((s, i) => i);
        [Benchmark] public object N03h_Linq() => _empty.SelectMany(i => i);
        [Benchmark] public object N03i_Linq() => _empty.SkipWhile(i => true);
        [Benchmark] public object N03j_Linq() => _empty.TakeWhile(i => true);
        [Benchmark] public object N03k_Linq() => _empty.Where((s, i) => true);

        [Benchmark]
        public double N04_Linq() =>
            _ints1000
            .DefaultIfEmpty()
            .Average();

        [Benchmark]
        public int N05_Linq()
        {
            int count = 0;
            foreach (var chunk in _ints1000.Chunk(10))
                count += chunk.Length;
            return count;
        }

        [Benchmark] public bool N06a_Linq() => _list1000AsEnumerable.Any(i => i == 1000);
        [Benchmark] public bool N06b_Linq() => _list1000AsEnumerable.All(i => i >= 0);
        [Benchmark] public int N06c_Linq() => _list1000AsEnumerable.Count(i => i == 0);
        [Benchmark] public int N06d_Linq() => _list1000AsEnumerable.First(i => i == 999);
        [Benchmark] public int N06e_Linq() => _list1000AsEnumerable.Single(i => i == 0);

        [Benchmark] public bool N07_Linq() => _source10000.SequenceEqual(_source10000List);

        [Benchmark] public string[] N08a_Linq() => StringItems(1).ToArray();
        [Benchmark] public string[] N08b_Linq() => StringItems(8).ToArray();
        [Benchmark] public string[] N08c_Linq() => StringItems(500).ToArray();

        [Benchmark] public List<int> N09a_Linq() => IntItems(8).Select(i => i).ToList();
        [Benchmark] public List<int> N09b_Linq() => IntItems(8).Where(i => true).Select(i => i).ToList();

        [Benchmark] public int[] N10_Linq() => IntItems(1).OrderBy(x => x).ToArray();

        [Benchmark] public Dictionary<int, KeyValuePair<int, int>> N11_Linq() => _linqPairs.ToDictionary(x => x.Key);

        [Benchmark] public Int128 N12_Linq() => _int128s.Max();

        [Benchmark] public int N13_Linq() => _ints10000.Max();

        [Benchmark] public int N14a_Linq() => _shuffled.OrderBy(x => x).Last();
        [Benchmark] public int N14b_Linq() => _shuffled.Order().Last();

        [Benchmark]
        public int N15_Linq()
        {
            int sum = 0;
            foreach (DayOfWeek dow in _days.Order())
                sum += (int)dow;
            return sum;
        }

        [Benchmark]
        public int N16_Linq()
        {
            int sum = 0;
            foreach (int value in _data1000.OrderBy(i => i))
                sum += value;
            return sum;
        }

        [Benchmark]
        public int N17_Linq()
        {
            int sum = 0;
            foreach (IGrouping<int, string> group in _linqLookup)
            {
                foreach (string item in group)
                    sum += item.Length;
            }
            return sum;
        }
    }
}
