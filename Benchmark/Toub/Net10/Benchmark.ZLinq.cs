using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net10
    {
        [Benchmark]
        public bool N01a_ZLinq() =>
            _source
            .AsValueEnumerable()
            .Append(100)
            .Contains(999);

        [Benchmark]
        public bool N01b_ZLinq() =>
            _source
            .AsValueEnumerable()
            .Concat(_source)
            .Contains(999);

        [Benchmark]
        public bool N01c_ZLinq() =>
            _source
            .AsValueEnumerable()
            .DefaultIfEmpty(42)
            .Contains(999);

        [Benchmark]
        public bool N01d_ZLinq() =>
            _source
            .AsValueEnumerable()
            .Distinct()
            .Contains(999);

        [Benchmark]
        public bool N01e_ZLinq() =>
            _source
            .AsValueEnumerable()
            .OrderBy(x => x)
            .Contains(999);

        [Benchmark]
        public bool N01f_ZLinq() =>
            _source
            .AsValueEnumerable()
            .Reverse()
            .Contains(999);

        [Benchmark]
        public bool N01g_ZLinq() =>
            _source
            .AsValueEnumerable()
            .Union(_source)
            .Contains(999);

        [Benchmark]
        public bool N01h_ZLinq() =>
            _source
            .AsValueEnumerable()
            .SelectMany(x => _source)
            .Contains(999);

        [Benchmark]
        public bool N01i_ZLinq() =>
            _source
            .AsValueEnumerable()
            .Where(x => true)
            .Select(x => x)
            .Contains(999);

        [Benchmark]
        public List<short> N02_ZLinq()
        {
            ValueEnumerable
            .Sequence<short>(42, 1042, 1)
            .CopyTo(_values);
            return _values;
        }

        [Benchmark]
        public List<int> N03a_ZLinq() =>
            _shuffleSource
            .AsValueEnumerable()
            .Shuffle()
            .Take(10)
            .ToList();

        [Benchmark]
        public bool N04a_ZLinq() =>
            _shuffleSource
            .AsValueEnumerable()
            .Shuffle()
            .Take(10)
            .Contains(2000);

        // a ZLinq sequence is a ref struct with no IEnumerable<T> form, so both are built here
        [Benchmark]
        public int N05a_ZLinq() =>
            ValueEnumerable
            .Sequence(0, 1000, 2)
            .LeftJoin(ValueEnumerable.Sequence(0, 1000, 3), o => o, i => i, (o, i) => o + i)
            .Count();

        [Benchmark]
        public List<string> N06_ZLinq() =>
            _strings
            .AsValueEnumerable()
            .Skip(200)
            .Take(200)
            .ToList();

        [Benchmark]
        public int N07_ZLinq() =>
            _readOnly
            .AsValueEnumerable()
            .Skip(100)
            .Take(800)
            .Sum();

        [Benchmark]
        public bool N08_ZLinq() =>
            _data
            .AsValueEnumerable()
            .Contains(int.MaxValue, EqualityComparer<int>.Default);

        // N03b, N04b, N05b: the compositions the new operators replace, measured for System.Linq and Cistern
    }
}
