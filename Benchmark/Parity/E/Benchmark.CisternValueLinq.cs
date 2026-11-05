using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class E
    {
        [Benchmark]
        public double E01a_CisternValueLinq() =>
            _doublesNaN
            .Min();

        [Benchmark]
        public double? E01b_CisternValueLinq() =>
            _nullableDoublesNaN
            .Min();

        [Benchmark]
        public double E01c_CisternValueLinq() =>
            _ints10
            .Min(_nanAtMinusOne);

        [Benchmark]
        public double E01d_CisternValueLinq() =>
            _doublesNaNIterator
            .Min();

        [Benchmark]
        public double E02a_CisternValueLinq() =>
            _leadingNaN
            .Max();

        [Benchmark]
        public double E02b_CisternValueLinq() =>
            _leadingNaNList
            .Max();

        [Benchmark]
        public int? E03a_CisternValueLinq() =>
            _nullableInts
            .Max();

        [Benchmark]
        public int? E03b_CisternValueLinq() =>
            _ints
            .Max(_nullTenth);

        [Benchmark]
        public short E04a_CisternValueLinq() =>
            _shortIterator
            .Min();

        [Benchmark]
        public ulong E04b_CisternValueLinq() =>
            _ulongIterator
            .Max();

        [Benchmark]
        public int E05a_CisternValueLinq() =>
            _ints
            .MinBy(_key);

        [Benchmark]
        public int E05b_CisternValueLinq() =>
            _iterator
            .MaxBy(_key);

        [Benchmark]
        public TimeSpan E05c_CisternValueLinq() =>
            _timeSpans
            .Min();

        [Benchmark]
        public short E05d_CisternValueLinq() =>
            _ints
            .Min(_toShort);

        [Benchmark]
        public int? E06a_CisternValueLinq() =>
            _nullableInts
            .Min();

        [Benchmark]
        public long? E06b_CisternValueLinq() =>
            _nullableLongs
            .Max();

        [Benchmark]
        public decimal? E06c_CisternValueLinq() =>
            _nullableDecimals
            .Min();

        [Benchmark]
        public int[] E07a_CisternValueLinq() =>
            _iterator
            .TakeLast(10)
            .ToArray();

        [Benchmark]
        public int E07b_CisternValueLinq() =>
            _iterator
            .SkipLast(10)
            .Sum();

        [Benchmark]
        public int[] E07c_CisternValueLinq() =>
            _iterator
            .Take(^10..)
            .ToArray();

        [Benchmark]
        public int E08_CisternValueLinq() =>
            _iterator
            .ElementAt(^10);

        [Benchmark]
        public int[][] E09a_CisternValueLinq() =>
            _tenIterator
            .Chunk(10_000)
            .ToArray();

        [Benchmark]
        public int E09b_CisternValueLinq() =>
            _iterator
            .Chunk(100)
            .Count();

        [Benchmark]
        public int E10a_CisternValueLinq() =>
            _empty
            .Distinct()
            .Count();

        [Benchmark]
        public int E10b_CisternValueLinq() =>
            _empty
            .DistinctBy(_key)
            .Count();

        [Benchmark]
        public int E10c_CisternValueLinq() =>
            _empty
            .Union(_empty)
            .Count();

        [Benchmark]
        public int E10d_CisternValueLinq() =>
            _empty
            .CountBy(_key)
            .Count();

        [Benchmark]
        public int E10e_CisternValueLinq() =>
            _empty
            .AggregateBy(_key, 0, _add)
            .Count();

        [Benchmark]
        public KeyValuePair<int, int>[] E11a_CisternValueLinq() =>
            _ints
            .CountBy(_key)
            .ToArray();

        [Benchmark]
        public KeyValuePair<int, int>[] E11b_CisternValueLinq() =>
            _ints
            .AggregateBy(_key, 0, _add)
            .ToArray();
    }
}
