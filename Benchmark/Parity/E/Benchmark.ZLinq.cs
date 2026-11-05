using BenchmarkDotNet.Attributes;
using ZLinq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class E
    {
        [Benchmark]
        public double E01a_ZLinq() =>
            _doublesNaN
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public double? E01b_ZLinq() =>
            _nullableDoublesNaN
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public double E01c_ZLinq() =>
            _ints10
            .AsValueEnumerable()
            .Min(_nanAtMinusOne);

        [Benchmark]
        public double E01d_ZLinq() =>
            _doublesNaNIterator
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public double E02a_ZLinq() =>
            _leadingNaN
            .AsValueEnumerable()
            .Max();

        [Benchmark]
        public double E02b_ZLinq() =>
            _leadingNaNList
            .AsValueEnumerable()
            .Max();

        [Benchmark]
        public int? E03a_ZLinq() =>
            _nullableInts
            .AsValueEnumerable()
            .Max();

        [Benchmark]
        public int? E03b_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Max(_nullTenth);

        [Benchmark]
        public short E04a_ZLinq() =>
            _shortIterator
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public ulong E04b_ZLinq() =>
            _ulongIterator
            .AsValueEnumerable()
            .Max();

        [Benchmark]
        public int E05a_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .MinBy(_key);

        [Benchmark]
        public int E05b_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .MaxBy(_key);

        [Benchmark]
        public TimeSpan E05c_ZLinq() =>
            _timeSpans
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public short E05d_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .Min(_toShort);

        [Benchmark]
        public int? E06a_ZLinq() =>
            _nullableInts
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public long? E06b_ZLinq() =>
            _nullableLongs
            .AsValueEnumerable()
            .Max();

        [Benchmark]
        public decimal? E06c_ZLinq() =>
            _nullableDecimals
            .AsValueEnumerable()
            .Min();

        [Benchmark]
        public int[] E07a_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .TakeLast(10)
            .ToArray();

        [Benchmark]
        public int E07b_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .SkipLast(10)
            .Sum();

        [Benchmark]
        public int[] E07c_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Take(^10..)
            .ToArray();

        [Benchmark]
        public int E08_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .ElementAt(^10);

        [Benchmark]
        public int[][] E09a_ZLinq() =>
            _tenIterator
            .AsValueEnumerable()
            .Chunk(10_000)
            .ToArray();

        [Benchmark]
        public int E09b_ZLinq() =>
            _iterator
            .AsValueEnumerable()
            .Chunk(100)
            .Count();

        [Benchmark]
        public int E10a_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .Distinct()
            .Count();

        [Benchmark]
        public int E10b_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .DistinctBy(_key)
            .Count();

        [Benchmark]
        public int E10c_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .Union(_empty)
            .Count();

        [Benchmark]
        public int E10d_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .CountBy(_key)
            .Count();

        [Benchmark]
        public int E10e_ZLinq() =>
            _empty
            .AsValueEnumerable()
            .AggregateBy(_key, 0, _add)
            .Count();

        [Benchmark]
        public KeyValuePair<int, int>[] E11a_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .CountBy(_key)
            .ToArray();

        [Benchmark]
        public KeyValuePair<int, int>[] E11b_ZLinq() =>
            _ints
            .AsValueEnumerable()
            .AggregateBy(_key, 0, _add)
            .ToArray();
    }
}
