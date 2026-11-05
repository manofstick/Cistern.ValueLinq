using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net9
    {
        IEnumerable<int> _cisternArrayDistinct;
        IEnumerable<int> _cisternAppendSelect;
        IEnumerable<int> _cisternRangeReverse;
        IEnumerable<int> _cisternListDefaultIfEmptySelect;
        IEnumerable<int> _cisternListSkipTake;
        IEnumerable<int> _cisternRangeUnion;
        IEnumerable<int> _cisternIteratorWhereSelect;
        IEnumerable<int> _cisternArrayWhereSelect;
        System.Linq.ILookup<int, string> _cisternLookup;
        IEnumerable<KeyValuePair<int, int>> _cisternPairs;

        // N01, N02 through ToEnumerable(), and through ToEnumerable(EnumerableOptions.Fuse)
        IEnumerable<int> _cisternArrayDistinctEnumerable;
        IEnumerable<int> _cisternAppendSelectEnumerable;
        IEnumerable<int> _cisternRangeReverseEnumerable;
        IEnumerable<int> _cisternListDefaultIfEmptySelectEnumerable;
        IEnumerable<int> _cisternListSkipTakeEnumerable;
        IEnumerable<int> _cisternRangeUnionEnumerable;
        IEnumerable<int> _cisternIteratorWhereSelectEnumerable;
        IEnumerable<int> _cisternArrayWhereSelectEnumerable;
        IEnumerable<int> _cisternArrayDistinctFused;
        IEnumerable<int> _cisternAppendSelectFused;
        IEnumerable<int> _cisternRangeReverseFused;
        IEnumerable<int> _cisternListDefaultIfEmptySelectFused;
        IEnumerable<int> _cisternListSkipTakeFused;
        IEnumerable<int> _cisternRangeUnionFused;
        IEnumerable<int> _cisternIteratorWhereSelectFused;
        IEnumerable<int> _cisternArrayWhereSelectFused;

        // the sources as their own types, for the Typed rows
        int[] _cisternSource10000;
        List<int> _cisternSource10000List;
        DayOfWeek[] _cisternDays;
        int[] _cisternData1000;

        partial void BuildCisternValueLinq()
        {
            _cisternSource10000 = (int[])_source10000;
            _cisternSource10000List = (List<int>)_source10000List;
            _cisternDays = (DayOfWeek[])_days;
            _cisternData1000 = (int[])_data1000;

            var arrayDistinct = Enumerable.Range(0, 1000).ToArray().Distinct();
            var appendSelect = Enumerable.Range(0, 1000).ToArray().Append(42).Select(i => i * 2);
            var rangeReverse = Enumerable.Range(0, 1000).Reverse();
            var listDefaultIfEmptySelect = Enumerable.Range(0, 1000).ToList().DefaultIfEmpty().Select(i => i * 2);
            var listSkipTake = Enumerable.Range(0, 1000).ToList().Skip(500).Take(100);
            var rangeUnion = Enumerable.Range(0, 1000).Union(Enumerable.Range(500, 1000));
            var iteratorWhereSelect = Iterations(100).Where(i => i % 2 == 0).Select(i => i);
            var arrayWhereSelect = Enumerable.Range(0, 100).ToArray().Where(i => i % 2 == 0).Select(i => i);

            _cisternArrayDistinct = arrayDistinct;
            _cisternAppendSelect = appendSelect;
            _cisternRangeReverse = rangeReverse;
            _cisternListDefaultIfEmptySelect = listDefaultIfEmptySelect;
            _cisternListSkipTake = listSkipTake;
            _cisternRangeUnion = rangeUnion;
            _cisternIteratorWhereSelect = iteratorWhereSelect;
            _cisternArrayWhereSelect = arrayWhereSelect;

            _cisternArrayDistinctEnumerable = arrayDistinct.ToEnumerable();
            _cisternAppendSelectEnumerable = appendSelect.ToEnumerable();
            _cisternRangeReverseEnumerable = rangeReverse.ToEnumerable();
            _cisternListDefaultIfEmptySelectEnumerable = listDefaultIfEmptySelect.ToEnumerable();
            _cisternListSkipTakeEnumerable = listSkipTake.ToEnumerable();
            _cisternRangeUnionEnumerable = rangeUnion.ToEnumerable();
            _cisternIteratorWhereSelectEnumerable = iteratorWhereSelect.ToEnumerable();
            _cisternArrayWhereSelectEnumerable = arrayWhereSelect.ToEnumerable();

            _cisternArrayDistinctFused = arrayDistinct.ToEnumerable(EnumerableOptions.Fuse);
            _cisternAppendSelectFused = appendSelect.ToEnumerable(EnumerableOptions.Fuse);
            _cisternRangeReverseFused = rangeReverse.ToEnumerable(EnumerableOptions.Fuse);
            _cisternListDefaultIfEmptySelectFused = listDefaultIfEmptySelect.ToEnumerable(EnumerableOptions.Fuse);
            _cisternListSkipTakeFused = listSkipTake.ToEnumerable(EnumerableOptions.Fuse);
            _cisternRangeUnionFused = rangeUnion.ToEnumerable(EnumerableOptions.Fuse);
            _cisternIteratorWhereSelectFused = iteratorWhereSelect.ToEnumerable(EnumerableOptions.Fuse);
            _cisternArrayWhereSelectFused = arrayWhereSelect.ToEnumerable(EnumerableOptions.Fuse);

            _cisternLookup = _lookupSource.ToLookup(s => s.Length);
            _cisternPairs = Enumerable.Range(0, 10_000).Select(x => new KeyValuePair<int, int>(x, x));
        }

        [Benchmark] public int N01a_CisternValueLinq() => _cisternArrayDistinct.First();
        [Benchmark] public int N01b_CisternValueLinq() => _cisternAppendSelect.Last();
        [Benchmark] public int N01c_CisternValueLinq() => _cisternRangeReverse.Count();
        [Benchmark] public int N01d_CisternValueLinq() => _cisternListDefaultIfEmptySelect.ElementAt(999);
        [Benchmark] public int N01e_CisternValueLinq() => _cisternListSkipTake.ElementAt(99);
        [Benchmark] public int N01f_CisternValueLinq() => _cisternRangeUnion.First();

        [Benchmark] public int N01a_CisternValueLinqEnumerable() => _cisternArrayDistinctEnumerable.First();
        [Benchmark] public int N01b_CisternValueLinqEnumerable() => _cisternAppendSelectEnumerable.Last();
        [Benchmark] public int N01c_CisternValueLinqEnumerable() => _cisternRangeReverseEnumerable.Count();
        [Benchmark] public int N01d_CisternValueLinqEnumerable() => _cisternListDefaultIfEmptySelectEnumerable.ElementAt(999);
        [Benchmark] public int N01e_CisternValueLinqEnumerable() => _cisternListSkipTakeEnumerable.ElementAt(99);
        [Benchmark] public int N01f_CisternValueLinqEnumerable() => _cisternRangeUnionEnumerable.First();

        [Benchmark] public int N01a_CisternValueLinqFused() => _cisternArrayDistinctFused.First();
        [Benchmark] public int N01b_CisternValueLinqFused() => _cisternAppendSelectFused.Last();
        [Benchmark] public int N01c_CisternValueLinqFused() => _cisternRangeReverseFused.Count();
        [Benchmark] public int N01d_CisternValueLinqFused() => _cisternListDefaultIfEmptySelectFused.ElementAt(999);
        [Benchmark] public int N01e_CisternValueLinqFused() => _cisternListSkipTakeFused.ElementAt(99);
        [Benchmark] public int N01f_CisternValueLinqFused() => _cisternRangeUnionFused.First();

        // built in the method over each source's own type
        [Benchmark] public int N01a_CisternValueLinqTyped() => _ints1000.Distinct().First();
        [Benchmark] public int N01b_CisternValueLinqTyped() => _ints1000.Append(42).Select(i => i * 2).Last();
        [Benchmark] public int N01c_CisternValueLinqTyped() => Enumerable.Range(0, 1000).Reverse().Count();
        [Benchmark] public int N01d_CisternValueLinqTyped() => _list1000.DefaultIfEmpty().Select(i => i * 2).ElementAt(999);
        [Benchmark] public int N01e_CisternValueLinqTyped() => _list1000.Skip(500).Take(100).ElementAt(99);
        [Benchmark] public int N01f_CisternValueLinqTyped() => Enumerable.Range(0, 1000).Union(Enumerable.Range(500, 1000)).First();

        [Benchmark] public bool N02a_CisternValueLinq() => _cisternIteratorWhereSelect.Any();
        [Benchmark] public bool N02b_CisternValueLinq() => _cisternArrayWhereSelect.Any();

        [Benchmark] public bool N02b_CisternValueLinqTyped() => _ints100.Where(i => i % 2 == 0).Select(i => i).Any();

        [Benchmark] public bool N02a_CisternValueLinqEnumerable() => _cisternIteratorWhereSelectEnumerable.Any();
        [Benchmark] public bool N02b_CisternValueLinqEnumerable() => _cisternArrayWhereSelectEnumerable.Any();

        [Benchmark] public bool N02a_CisternValueLinqFused() => _cisternIteratorWhereSelectFused.Any();
        [Benchmark] public bool N02b_CisternValueLinqFused() => _cisternArrayWhereSelectFused.Any();

        [Benchmark] public object N03a_CisternValueLinq() => _empty.Chunk(10);
        [Benchmark] public object N03b_CisternValueLinq() => _empty.Distinct();
        [Benchmark] public object N03c_CisternValueLinq() => _empty.GroupJoin(_empty, i => i, i => i, (i, j) => i);
        [Benchmark] public object N03d_CisternValueLinq() => _empty.Join(_empty, i => i, i => i, (i, j) => i);
        [Benchmark] public object N03e_CisternValueLinq() => _empty.ToLookup(i => i);
        [Benchmark] public object N03f_CisternValueLinq() => _empty.Reverse();
        [Benchmark] public object N03g_CisternValueLinq() => _empty.Select((s, i) => i);
        [Benchmark] public object N03h_CisternValueLinq() => _empty.SelectMany(i => i);
        [Benchmark] public object N03i_CisternValueLinq() => _empty.SkipWhile(i => true);
        [Benchmark] public object N03j_CisternValueLinq() => _empty.TakeWhile(i => true);
        [Benchmark] public object N03k_CisternValueLinq() => _empty.Where((s, i) => true);

        [Benchmark]
        public double N04_CisternValueLinq() =>
            _ints1000
            .DefaultIfEmpty()
            .Average();

        [Benchmark]
        public int N05_CisternValueLinq()
        {
            int count = 0;
            foreach (var chunk in _ints1000.Chunk(10))
                count += chunk.Length;
            return count;
        }

        [Benchmark] public bool N06a_CisternValueLinq() => _list1000AsEnumerable.Any(i => i == 1000);
        [Benchmark] public bool N06b_CisternValueLinq() => _list1000AsEnumerable.All(i => i >= 0);
        [Benchmark] public int N06c_CisternValueLinq() => _list1000AsEnumerable.Count(i => i == 0);
        [Benchmark] public int N06d_CisternValueLinq() => _list1000AsEnumerable.First(i => i == 999);
        [Benchmark] public int N06e_CisternValueLinq() => _list1000AsEnumerable.Single(i => i == 0);

        [Benchmark] public bool N06a_CisternValueLinqTyped() => _list1000.Any(i => i == 1000);
        [Benchmark] public bool N06b_CisternValueLinqTyped() => _list1000.All(i => i >= 0);
        [Benchmark] public int N06c_CisternValueLinqTyped() => _list1000.Count(i => i == 0);
        [Benchmark] public int N06d_CisternValueLinqTyped() => _list1000.First(i => i == 999);
        [Benchmark] public int N06e_CisternValueLinqTyped() => _list1000.Single(i => i == 0);

        [Benchmark] public bool N07_CisternValueLinq() => _source10000.SequenceEqual(_source10000List);

        [Benchmark] public bool N07_CisternValueLinqTyped() => _cisternSource10000.OfArray().SequenceEqual(_cisternSource10000List.OfList());

        [Benchmark] public string[] N08a_CisternValueLinq() => StringItems(1).ToArray();
        [Benchmark] public string[] N08b_CisternValueLinq() => StringItems(8).ToArray();
        [Benchmark] public string[] N08c_CisternValueLinq() => StringItems(500).ToArray();

        [Benchmark] public List<int> N09a_CisternValueLinq() => IntItems(8).Select(i => i).ToList();
        [Benchmark] public List<int> N09b_CisternValueLinq() => IntItems(8).Where(i => true).Select(i => i).ToList();

        [Benchmark] public int[] N10_CisternValueLinq() => IntItems(1).OrderBy(x => x).ToArray();

        [Benchmark] public Dictionary<int, KeyValuePair<int, int>> N11_CisternValueLinq() => _cisternPairs.ToDictionary(x => x.Key);

        // built in the method, as ZLinq's is
        [Benchmark] public Dictionary<int, KeyValuePair<int, int>> N11_CisternValueLinqInline() => Enumerable.Range(0, 10_000).Select(x => new KeyValuePair<int, int>(x, x)).ToDictionary(x => x.Key);

        [Benchmark] public Int128 N12_CisternValueLinq() => _int128s.Max();

        [Benchmark] public int N13_CisternValueLinq() => _ints10000.Max();

        [Benchmark] public int N14a_CisternValueLinq() => _shuffled.OrderBy(x => x).Last();
        [Benchmark] public int N14b_CisternValueLinq() => _shuffled.Order().Last();

        [Benchmark]
        public int N15_CisternValueLinq()
        {
            int sum = 0;
            foreach (DayOfWeek dow in _days.Order())
                sum += (int)dow;
            return sum;
        }

        [Benchmark]
        public int N16_CisternValueLinq()
        {
            int sum = 0;
            foreach (int value in _data1000.OrderBy(i => i))
                sum += value;
            return sum;
        }

        [Benchmark]
        public int N15_CisternValueLinqTyped()
        {
            int sum = 0;
            foreach (DayOfWeek dow in _cisternDays.Order())
                sum += (int)dow;
            return sum;
        }

        [Benchmark]
        public int N16_CisternValueLinqTyped()
        {
            int sum = 0;
            foreach (int value in _cisternData1000.OrderBy(i => i))
                sum += value;
            return sum;
        }

        [Benchmark]
        public int N17_CisternValueLinq()
        {
            int sum = 0;
            foreach (System.Linq.IGrouping<int, string> group in _cisternLookup)
            {
                foreach (string item in group)
                    sum += item.Length;
            }
            return sum;
        }
    }
}
