using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net10
    {
        IEnumerable<int> _cisternOuter;
        IEnumerable<int> _cisternInner;

        // the sources as their own types, for the Typed rows
        int[] _cisternSource;
        List<int> _cisternShuffleSource;
        string[] _cisternStrings;
        int[] _cisternData;

        partial void BuildCisternValueLinq()
        {
            _cisternOuter = Enumerable.Sequence(0, 1000, 2);
            _cisternInner = Enumerable.Sequence(0, 1000, 3);

            _cisternSource = (int[])_source;
            _cisternShuffleSource = (List<int>)_shuffleSource;
            _cisternStrings = (string[])_strings;
            _cisternData = (int[])_data;
        }

        [Benchmark]
        public bool N01a_CisternValueLinq() =>
            _source
            .Append(100)
            .Contains(999);

        [Benchmark]
        public bool N01b_CisternValueLinq() =>
            _source
            .Concat(_source)
            .Contains(999);

        [Benchmark]
        public bool N01c_CisternValueLinq() =>
            _source
            .DefaultIfEmpty(42)
            .Contains(999);

        [Benchmark]
        public bool N01d_CisternValueLinq() =>
            _source
            .Distinct()
            .Contains(999);

        [Benchmark]
        public bool N01e_CisternValueLinq() =>
            _source
            .OrderBy(x => x)
            .Contains(999);

        [Benchmark]
        public bool N01f_CisternValueLinq() =>
            _source
            .Reverse()
            .Contains(999);

        [Benchmark]
        public bool N01g_CisternValueLinq() =>
            _source
            .Union(_source)
            .Contains(999);

        [Benchmark]
        public bool N01h_CisternValueLinq() =>
            _source
            .SelectMany(x => _source)
            .Contains(999);

        [Benchmark]
        public bool N01i_CisternValueLinq() =>
            _source
            .Where(x => true)
            .Select(x => x)
            .Contains(999);

        [Benchmark]
        public List<short> N02_CisternValueLinq()
        {
            _values.Clear();
            _values.AddRange(Enumerable.Sequence<short>(42, 1042, 1));
            return _values;
        }

        // handed out as an ICollection<T>, which AddRange sizes from and copies in
        [Benchmark]
        public List<short> N02_CisternValueLinqCollection()
        {
            _values.Clear();
            _values.AddRange(Enumerable.Sequence<short>(42, 1042, 1).ToEnumerable());
            return _values;
        }

        [Benchmark]
        public List<int> N03a_CisternValueLinq() =>
            _shuffleSource
            .Shuffle()
            .Take(10)
            .ToList();

        [Benchmark]
        public List<int> N03b_CisternValueLinq() =>
            ShuffleManual(_shuffleSource)
            .Take(10)
            .ToList();

        [Benchmark]
        public bool N04a_CisternValueLinq() =>
            _shuffleSource
            .Shuffle()
            .Take(10)
            .Contains(2000);

        [Benchmark]
        public bool N04b_CisternValueLinq() =>
            ShuffleManual(_shuffleSource)
            .Take(10)
            .Contains(2000);

        [Benchmark]
        public int N05a_CisternValueLinq() =>
            _cisternOuter
            .LeftJoin(_cisternInner, o => o, i => i, (o, i) => o + i)
            .Count();

        [Benchmark]
        public int N05b_CisternValueLinq() =>
            _cisternOuter
            .GroupJoin(_cisternInner, o => o, i => i, (o, inners) => (o, inners))
            .SelectMany(x => x.inners.DefaultIfEmpty(), (x, i) => x.o + i)
            .Count();

        [Benchmark]
        public List<string> N06_CisternValueLinq() =>
            _strings
            .Skip(200)
            .Take(200)
            .ToList();

        [Benchmark]
        public int N07_CisternValueLinq() =>
            _readOnly
            .Skip(100)
            .Take(800)
            .Sum();

        [Benchmark]
        public bool N08_CisternValueLinq() =>
            _data
            .Contains(int.MaxValue, EqualityComparer<int>.Default);

        [Benchmark]
        public bool N01a_CisternValueLinqTyped() =>
            _cisternSource
            .Append(100)
            .Contains(999);

        [Benchmark]
        public bool N01b_CisternValueLinqTyped() =>
            _cisternSource
            .Concat(_cisternSource)
            .Contains(999);

        [Benchmark]
        public bool N01c_CisternValueLinqTyped() =>
            _cisternSource
            .DefaultIfEmpty(42)
            .Contains(999);

        [Benchmark]
        public bool N01d_CisternValueLinqTyped() =>
            _cisternSource
            .Distinct()
            .Contains(999);

        [Benchmark]
        public bool N01e_CisternValueLinqTyped() =>
            _cisternSource
            .OrderBy(x => x)
            .Contains(999);

        [Benchmark]
        public bool N01f_CisternValueLinqTyped() =>
            _cisternSource
            .Reverse()
            .Contains(999);

        [Benchmark]
        public bool N01g_CisternValueLinqTyped() =>
            _cisternSource
            .Union(_cisternSource)
            .Contains(999);

        [Benchmark]
        public bool N01h_CisternValueLinqTyped() =>
            _cisternSource
            .SelectMany(x => _cisternSource)
            .Contains(999);

        [Benchmark]
        public bool N01i_CisternValueLinqTyped() =>
            _cisternSource
            .Where(x => true)
            .Select(x => x)
            .Contains(999);

        [Benchmark]
        public List<int> N03a_CisternValueLinqTyped() =>
            _cisternShuffleSource
            .Shuffle()
            .Take(10)
            .ToList();

        [Benchmark]
        public bool N04a_CisternValueLinqTyped() =>
            _cisternShuffleSource
            .Shuffle()
            .Take(10)
            .Contains(2000);

        // the inner sequence is still met as IEnumerable<T>, the only form LeftJoin and GroupJoin take it in
        [Benchmark]
        public int N05a_CisternValueLinqTyped() =>
            Enumerable.Sequence(0, 1000, 2)
            .LeftJoin(Enumerable.Sequence(0, 1000, 3), o => o, i => i, (o, i) => o + i)
            .Count();

        [Benchmark]
        public int N05b_CisternValueLinqTyped() =>
            Enumerable.Sequence(0, 1000, 2)
            .GroupJoin(Enumerable.Sequence(0, 1000, 3), o => o, i => i, (o, inners) => (o, inners))
            .SelectMany(x => x.inners.DefaultIfEmpty(), (x, i) => x.o + i)
            .Count();

        [Benchmark]
        public List<string> N06_CisternValueLinqTyped() =>
            _cisternStrings
            .Skip(200)
            .Take(200)
            .ToList();

        [Benchmark]
        public bool N08_CisternValueLinqTyped() =>
            _cisternData
            .Contains(int.MaxValue, EqualityComparer<int>.Default);
    }
}
