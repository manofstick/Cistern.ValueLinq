using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section E: System.Linq's early exit and bounded work in its aggregates: Min and Max stopping at a NaN or
    // reading nullable and non-numeric types, TakeLast, SkipLast and ElementAt from the end, Chunk, the set and grouping
    // operators over an empty source, and CountBy and AggregateBy. One method per entry (a letter per source, type or
    // operation where an entry covers several) and library, named by the entry's ID, so a filter `*E.E01*` matches only E01.
    [MemoryDiagnoser]
    public partial class E
    {
        const int Length = 10_000;

        int[] _ints;
        int[] _ints10;
        IEnumerable<int> _iterator;
        IEnumerable<int> _empty;
        IEnumerable<int> _tenIterator;
        double[] _doublesNaN;
        IEnumerable<double> _doublesNaNIterator;
        List<double?> _nullableDoublesNaN;
        double[] _leadingNaN;
        List<double> _leadingNaNList;
        List<int?> _nullableInts;
        long?[] _nullableLongs;
        decimal?[] _nullableDecimals;
        IEnumerable<short> _shortIterator;
        IEnumerable<ulong> _ulongIterator;
        TimeSpan[] _timeSpans;
        Func<int, double> _nanAtMinusOne;
        Func<int, int?> _nullTenth;
        Func<int, int> _key;
        Func<int, short> _toShort;
        Func<int, int, int> _add;

        public E()
        {
            _ints = Data.Values(Length);
            _iterator = Data.Iterator(_ints);
            _empty = Data.Iterator(new int[0]);
            _tenIterator = Data.Iterator(_ints[..10]);

            // the selector cannot see the index, so the element at 10 is made the one value it turns into NaN
            _ints10 = (int[])_ints.Clone();
            _ints10[10] = -1;
            _nanAtMinusOne = x => x < 0 ? double.NaN : x;

            _doublesNaN = Array.ConvertAll(_ints, x => (double)x);
            _doublesNaN[10] = double.NaN;
            _doublesNaNIterator = Iterate(_doublesNaN);
            _nullableDoublesNaN = new List<double?>(Array.ConvertAll(_doublesNaN, x => (double?)x));

            _leadingNaN = Array.ConvertAll(_ints, x => (double)x);
            Array.Fill(_leadingNaN, double.NaN, 0, 100);
            _leadingNaNList = new List<double>(_leadingNaN);

            _nullableInts = new List<int?>();
            for (var i = 0; i < _ints.Length; ++i)
                _nullableInts.Add(i % 10 == 0 ? null : _ints[i]);
            _nullableLongs = new long?[_ints.Length];
            _nullableDecimals = new decimal?[_ints.Length];
            for (var i = 0; i < _ints.Length; ++i)
            {
                _nullableLongs[i] = i % 10 == 0 ? null : _ints[i];
                _nullableDecimals[i] = i % 10 == 0 ? null : _ints[i];
            }

            _shortIterator = Iterate(Array.ConvertAll(_ints, x => (short)x));
            _ulongIterator = Iterate(Array.ConvertAll(_ints, x => (ulong)x));
            _timeSpans = Array.ConvertAll(_ints, x => TimeSpan.FromTicks(x));

            _nullTenth = x => x % 10 == 0 ? null : x;
            _key = x => x % 100;
            _toShort = x => (short)x;
            _add = (a, x) => a + x;
        }

        static IEnumerable<T> Iterate<T>(T[] values)
        {
            foreach (var x in values)
                yield return x;
        }

        internal static void SanityCheck()
        {
            var check = new E();

            Check.Same(nameof(E01a_Linq), check.E01a_Linq(), check.E01a_CisternValueLinq());
            Check.Same(nameof(E01b_Linq), check.E01b_Linq(), check.E01b_CisternValueLinq());
            Check.Same(nameof(E01c_Linq), check.E01c_Linq(), check.E01c_CisternValueLinq());
            Check.Same(nameof(E01d_Linq), check.E01d_Linq(), check.E01d_CisternValueLinq());
            Check.Same(nameof(E02a_Linq), check.E02a_Linq(), check.E02a_CisternValueLinq());
            Check.Same(nameof(E02b_Linq), check.E02b_Linq(), check.E02b_CisternValueLinq());
            Check.Same(nameof(E03a_Linq), check.E03a_Linq(), check.E03a_CisternValueLinq());
            Check.Same(nameof(E03b_Linq), check.E03b_Linq(), check.E03b_CisternValueLinq());
            Check.Same(nameof(E04a_Linq), check.E04a_Linq(), check.E04a_CisternValueLinq());
            Check.Same(nameof(E04b_Linq), check.E04b_Linq(), check.E04b_CisternValueLinq());
            Check.Same(nameof(E05a_Linq), check.E05a_Linq(), check.E05a_CisternValueLinq());
            Check.Same(nameof(E05b_Linq), check.E05b_Linq(), check.E05b_CisternValueLinq());
            Check.Same(nameof(E05c_Linq), check.E05c_Linq(), check.E05c_CisternValueLinq());
            Check.Same(nameof(E05d_Linq), check.E05d_Linq(), check.E05d_CisternValueLinq());
            Check.Same(nameof(E06a_Linq), check.E06a_Linq(), check.E06a_CisternValueLinq());
            Check.Same(nameof(E06b_Linq), check.E06b_Linq(), check.E06b_CisternValueLinq());
            Check.Same(nameof(E06c_Linq), check.E06c_Linq(), check.E06c_CisternValueLinq());
            Check.Same(nameof(E07a_Linq), check.E07a_Linq(), check.E07a_CisternValueLinq());
            Check.Same(nameof(E07b_Linq), check.E07b_Linq(), check.E07b_CisternValueLinq());
            Check.Same(nameof(E07c_Linq), check.E07c_Linq(), check.E07c_CisternValueLinq());
            Check.Same(nameof(E08_Linq), check.E08_Linq(), check.E08_CisternValueLinq());
            Check.Same(nameof(E09a_Linq), check.E09a_Linq(), check.E09a_CisternValueLinq());
            Check.Same(nameof(E09b_Linq), check.E09b_Linq(), check.E09b_CisternValueLinq());
            Check.Same(nameof(E10a_Linq), check.E10a_Linq(), check.E10a_CisternValueLinq());
            Check.Same(nameof(E10b_Linq), check.E10b_Linq(), check.E10b_CisternValueLinq());
            Check.Same(nameof(E10c_Linq), check.E10c_Linq(), check.E10c_CisternValueLinq());
            Check.Same(nameof(E10d_Linq), check.E10d_Linq(), check.E10d_CisternValueLinq());
            Check.Same(nameof(E10e_Linq), check.E10e_Linq(), check.E10e_CisternValueLinq());
            Check.Same(nameof(E11a_Linq), check.E11a_Linq(), check.E11a_CisternValueLinq());
            Check.Same(nameof(E11b_Linq), check.E11b_Linq(), check.E11b_CisternValueLinq());

            Check.Same(nameof(E01a_ZLinq), check.E01a_Linq(), check.E01a_ZLinq());
            Check.Same(nameof(E01b_ZLinq), check.E01b_Linq(), check.E01b_ZLinq());
            Check.Same(nameof(E01c_ZLinq), check.E01c_Linq(), check.E01c_ZLinq());
            Check.Same(nameof(E01d_ZLinq), check.E01d_Linq(), check.E01d_ZLinq());
            Check.Same(nameof(E02a_ZLinq), check.E02a_Linq(), check.E02a_ZLinq());
            Check.Same(nameof(E02b_ZLinq), check.E02b_Linq(), check.E02b_ZLinq());
            Check.Same(nameof(E03a_ZLinq), check.E03a_Linq(), check.E03a_ZLinq());
            Check.Same(nameof(E03b_ZLinq), check.E03b_Linq(), check.E03b_ZLinq());
            Check.Same(nameof(E04a_ZLinq), check.E04a_Linq(), check.E04a_ZLinq());
            Check.Same(nameof(E04b_ZLinq), check.E04b_Linq(), check.E04b_ZLinq());
            Check.Same(nameof(E05a_ZLinq), check.E05a_Linq(), check.E05a_ZLinq());
            Check.Same(nameof(E05b_ZLinq), check.E05b_Linq(), check.E05b_ZLinq());
            Check.Same(nameof(E05c_ZLinq), check.E05c_Linq(), check.E05c_ZLinq());
            Check.Same(nameof(E05d_ZLinq), check.E05d_Linq(), check.E05d_ZLinq());
            Check.Same(nameof(E06a_ZLinq), check.E06a_Linq(), check.E06a_ZLinq());
            Check.Same(nameof(E06b_ZLinq), check.E06b_Linq(), check.E06b_ZLinq());
            Check.Same(nameof(E06c_ZLinq), check.E06c_Linq(), check.E06c_ZLinq());
            Check.Same(nameof(E07a_ZLinq), check.E07a_Linq(), check.E07a_ZLinq());
            Check.Same(nameof(E07b_ZLinq), check.E07b_Linq(), check.E07b_ZLinq());
            Check.Same(nameof(E07c_ZLinq), check.E07c_Linq(), check.E07c_ZLinq());
            Check.Same(nameof(E08_ZLinq), check.E08_Linq(), check.E08_ZLinq());
            Check.Same(nameof(E09a_ZLinq), check.E09a_Linq(), check.E09a_ZLinq());
            Check.Same(nameof(E09b_ZLinq), check.E09b_Linq(), check.E09b_ZLinq());
            Check.Same(nameof(E10a_ZLinq), check.E10a_Linq(), check.E10a_ZLinq());
            Check.Same(nameof(E10b_ZLinq), check.E10b_Linq(), check.E10b_ZLinq());
            Check.Same(nameof(E10c_ZLinq), check.E10c_Linq(), check.E10c_ZLinq());
            Check.Same(nameof(E10d_ZLinq), check.E10d_Linq(), check.E10d_ZLinq());
            Check.Same(nameof(E10e_ZLinq), check.E10e_Linq(), check.E10e_ZLinq());
            Check.Same(nameof(E11a_ZLinq), check.E11a_Linq(), check.E11a_ZLinq());
            Check.Same(nameof(E11b_ZLinq), check.E11b_Linq(), check.E11b_ZLinq());

            Check.Same(nameof(E01a_Handcoded), check.E01a_Linq(), check.E01a_Handcoded());
            Check.Same(nameof(E01b_Handcoded), check.E01b_Linq(), check.E01b_Handcoded());
            Check.Same(nameof(E01c_Handcoded), check.E01c_Linq(), check.E01c_Handcoded());
            Check.Same(nameof(E01d_Handcoded), check.E01d_Linq(), check.E01d_Handcoded());
            Check.Same(nameof(E02a_Handcoded), check.E02a_Linq(), check.E02a_Handcoded());
            Check.Same(nameof(E02b_Handcoded), check.E02b_Linq(), check.E02b_Handcoded());
            Check.Same(nameof(E03a_Handcoded), check.E03a_Linq(), check.E03a_Handcoded());
            Check.Same(nameof(E03b_Handcoded), check.E03b_Linq(), check.E03b_Handcoded());
            Check.Same(nameof(E04a_Handcoded), check.E04a_Linq(), check.E04a_Handcoded());
            Check.Same(nameof(E04b_Handcoded), check.E04b_Linq(), check.E04b_Handcoded());
            Check.Same(nameof(E05a_Handcoded), check.E05a_Linq(), check.E05a_Handcoded());
            Check.Same(nameof(E05b_Handcoded), check.E05b_Linq(), check.E05b_Handcoded());
            Check.Same(nameof(E05c_Handcoded), check.E05c_Linq(), check.E05c_Handcoded());
            Check.Same(nameof(E05d_Handcoded), check.E05d_Linq(), check.E05d_Handcoded());
            Check.Same(nameof(E06a_Handcoded), check.E06a_Linq(), check.E06a_Handcoded());
            Check.Same(nameof(E06b_Handcoded), check.E06b_Linq(), check.E06b_Handcoded());
            Check.Same(nameof(E06c_Handcoded), check.E06c_Linq(), check.E06c_Handcoded());
            Check.Same(nameof(E07a_Handcoded), check.E07a_Linq(), check.E07a_Handcoded());
            Check.Same(nameof(E07b_Handcoded), check.E07b_Linq(), check.E07b_Handcoded());
            Check.Same(nameof(E07c_Handcoded), check.E07c_Linq(), check.E07c_Handcoded());
            Check.Same(nameof(E08_Handcoded), check.E08_Linq(), check.E08_Handcoded());
            Check.Same(nameof(E09a_Handcoded), check.E09a_Linq(), check.E09a_Handcoded());
            Check.Same(nameof(E09b_Handcoded), check.E09b_Linq(), check.E09b_Handcoded());
            Check.Same(nameof(E10a_Handcoded), check.E10a_Linq(), check.E10a_Handcoded());
            Check.Same(nameof(E10b_Handcoded), check.E10b_Linq(), check.E10b_Handcoded());
            Check.Same(nameof(E10c_Handcoded), check.E10c_Linq(), check.E10c_Handcoded());
            Check.Same(nameof(E10d_Handcoded), check.E10d_Linq(), check.E10d_Handcoded());
            Check.Same(nameof(E10e_Handcoded), check.E10e_Linq(), check.E10e_Handcoded());
            Check.Same(nameof(E11a_Handcoded), check.E11a_Linq(), check.E11a_Handcoded());
            Check.Same(nameof(E11b_Handcoded), check.E11b_Linq(), check.E11b_Handcoded());

            // the entries whose saving is user code not run: Cistern should call it no more often than System.Linq
            Action<Func<int, double>> useNan = s => check._nanAtMinusOne = s;
            Check.NoMoreCalls(nameof(E01c_Linq), check._nanAtMinusOne, useNan, () => check.E01c_Linq(), () => check.E01c_CisternValueLinq());

            Action<Func<int, int?>> useNull = s => check._nullTenth = s;
            Check.NoMoreCalls(nameof(E03b_Linq), check._nullTenth, useNull, () => check.E03b_Linq(), () => check.E03b_CisternValueLinq());
        }
    }
}
