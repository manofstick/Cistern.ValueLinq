using BenchmarkDotNet.Attributes;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class E
    {
        [Benchmark]
        public double E01a_Linq() =>
            _doublesNaN
            .Min();

        [Benchmark]
        public double? E01b_Linq() =>
            _nullableDoublesNaN
            .Min();

        [Benchmark]
        public double E01c_Linq() =>
            _ints10
            .Min(_nanAtMinusOne);

        [Benchmark]
        public double E01d_Linq() =>
            _doublesNaNIterator
            .Min();

        [Benchmark]
        public double E02a_Linq() =>
            _leadingNaN
            .Max();

        [Benchmark]
        public double E02b_Linq() =>
            _leadingNaNList
            .Max();

        [Benchmark]
        public int? E03a_Linq() =>
            _nullableInts
            .Max();

        [Benchmark]
        public int? E03b_Linq() =>
            _ints
            .Max(_nullTenth);

        [Benchmark]
        public short E04a_Linq() =>
            _shortIterator
            .Min();

        [Benchmark]
        public ulong E04b_Linq() =>
            _ulongIterator
            .Max();

        [Benchmark]
        public int E05a_Linq() =>
            _ints
            .MinBy(_key);

        [Benchmark]
        public int E05b_Linq() =>
            _iterator
            .MaxBy(_key);

        [Benchmark]
        public TimeSpan E05c_Linq() =>
            _timeSpans
            .Min();

        [Benchmark]
        public short E05d_Linq() =>
            _ints
            .Min(_toShort);

        [Benchmark]
        public int? E06a_Linq() =>
            _nullableInts
            .Min();

        [Benchmark]
        public long? E06b_Linq() =>
            _nullableLongs
            .Max();

        [Benchmark]
        public decimal? E06c_Linq() =>
            _nullableDecimals
            .Min();

        [Benchmark]
        public int[] E07a_Linq() =>
            _iterator
            .TakeLast(10)
            .ToArray();

        [Benchmark]
        public int E07b_Linq() =>
            _iterator
            .SkipLast(10)
            .Sum();

        [Benchmark]
        public int[] E07c_Linq() =>
            _iterator
            .Take(^10..)
            .ToArray();

        [Benchmark]
        public int E08_Linq() =>
            _iterator
            .ElementAt(^10);

        [Benchmark]
        public int[][] E09a_Linq() =>
            _tenIterator
            .Chunk(10_000)
            .ToArray();

        [Benchmark]
        public int E09b_Linq() =>
            _iterator
            .Chunk(100)
            .Count();

        [Benchmark]
        public int E10a_Linq() =>
            _empty
            .Distinct()
            .Count();

        [Benchmark]
        public int E10b_Linq() =>
            _empty
            .DistinctBy(_key)
            .Count();

        [Benchmark]
        public int E10c_Linq() =>
            _empty
            .Union(_empty)
            .Count();

        [Benchmark]
        public int E10d_Linq() =>
            _empty
            .CountBy(_key)
            .Count();

        [Benchmark]
        public int E10e_Linq() =>
            _empty
            .AggregateBy(_key, 0, _add)
            .Count();

        [Benchmark]
        public KeyValuePair<int, int>[] E11a_Linq() =>
            _ints
            .CountBy(_key)
            .ToArray();

        [Benchmark]
        public KeyValuePair<int, int>[] E11b_Linq() =>
            _ints
            .AggregateBy(_key, 0, _add)
            .ToArray();
    }
}
