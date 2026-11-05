using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net10
    {
        IEnumerable<int> _linqOuter;
        IEnumerable<int> _linqInner;

        partial void BuildLinq()
        {
            _linqOuter = Enumerable.Sequence(0, 1000, 2);
            _linqInner = Enumerable.Sequence(0, 1000, 3);
        }

        [Benchmark]
        public bool N01a_Linq() =>
            _source
            .Append(100)
            .Contains(999);

        [Benchmark]
        public bool N01b_Linq() =>
            _source
            .Concat(_source)
            .Contains(999);

        [Benchmark]
        public bool N01c_Linq() =>
            _source
            .DefaultIfEmpty(42)
            .Contains(999);

        [Benchmark]
        public bool N01d_Linq() =>
            _source
            .Distinct()
            .Contains(999);

        [Benchmark]
        public bool N01e_Linq() =>
            _source
            .OrderBy(x => x)
            .Contains(999);

        [Benchmark]
        public bool N01f_Linq() =>
            _source
            .Reverse()
            .Contains(999);

        [Benchmark]
        public bool N01g_Linq() =>
            _source
            .Union(_source)
            .Contains(999);

        [Benchmark]
        public bool N01h_Linq() =>
            _source
            .SelectMany(x => _source)
            .Contains(999);

        [Benchmark]
        public bool N01i_Linq() =>
            _source
            .Where(x => true)
            .Select(x => x)
            .Contains(999);

        [Benchmark]
        public List<short> N02_Linq()
        {
            _values.Clear();
            _values.AddRange(Enumerable.Sequence<short>(42, 1042, 1));
            return _values;
        }

        [Benchmark]
        public List<int> N03a_Linq() =>
            _shuffleSource
            .Shuffle()
            .Take(10)
            .ToList();

        [Benchmark]
        public List<int> N03b_Linq() =>
            ShuffleManual(_shuffleSource)
            .Take(10)
            .ToList();

        [Benchmark]
        public bool N04a_Linq() =>
            _shuffleSource
            .Shuffle()
            .Take(10)
            .Contains(2000);

        [Benchmark]
        public bool N04b_Linq() =>
            ShuffleManual(_shuffleSource)
            .Take(10)
            .Contains(2000);

        [Benchmark]
        public int N05a_Linq() =>
            _linqOuter
            .LeftJoin(_linqInner, o => o, i => i, (o, i) => o + i)
            .Count();

        [Benchmark]
        public int N05b_Linq() =>
            ManualLeftJoin(_linqOuter, _linqInner, o => o, i => i, (o, i) => o + i)
            .Count();

        [Benchmark]
        public List<string> N06_Linq() =>
            _strings
            .Skip(200)
            .Take(200)
            .ToList();

        [Benchmark]
        public int N07_Linq() =>
            _readOnly
            .Skip(100)
            .Take(800)
            .Sum();

        [Benchmark]
        public bool N08_Linq() =>
            _data
            .Contains(int.MaxValue, EqualityComparer<int>.Default);
    }
}
