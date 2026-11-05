using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section D: System.Linq's span and vectorised paths over T[] and List<T>: Sum, Average, Min and Max over the
    // numeric types, Aggregate, the predicate operators, Where and Select feeding an aggregation, and SelectMany and
    // Concat into an array. One method per entry (a letter per source, type or operation where an entry covers several)
    // and library, named by the entry's ID. D09, D10, D13, D14, D16 and D17 are measured by earlier classes.
    [MemoryDiagnoser]
    public partial class D
    {
        const int Length = 10_000;

        int[] _ints;
        List<int> _intList;
        long[] _longs;
        List<long> _longList;
        float[] _floats;
        List<float> _floatList;
        double[] _doubles;
        List<double> _doubleList;
        decimal[] _decimals;
        List<decimal> _decimalList;
        short[] _shorts;
        int[] _found;
        List<int> _foundList;
        Func<int, bool> _isLast;
        Func<int, bool> _belowRange;
        Func<int, bool> _predicate;
        List<int> _sortedList;
        Func<int, bool> _ninety;
        Func<int, int> _selector;
        Func<int, int, int> _add;
        Func<long, int, long> _addLong;
        Func<long, int> _half;
        List<int[]> _arrays;
        List<List<int>> _lists;
        Func<int[], IEnumerable<int>> _flattenArray;
        Func<List<int>, IEnumerable<int>> _flattenList;
        IEnumerable<int> _short;

        public D()
        {
            _ints = Data.Values(Length);
            _intList = new List<int>(_ints);
            _longs = Array.ConvertAll(_ints, x => (long)x);
            _longList = new List<long>(_longs);

            // eighths are exact in binary, so every sum is exact in float, double and decimal whatever the order of addition
            _floats = Array.ConvertAll(_ints, x => x / 8f);
            _floatList = new List<float>(_floats);
            _doubles = Array.ConvertAll(_ints, x => x / 8d);
            _doubleList = new List<double>(_doubles);
            _decimals = Array.ConvertAll(_ints, x => x / 8m);
            _decimalList = new List<decimal>(_decimals);
            _shorts = Array.ConvertAll(_ints, x => (short)x);

            // only the last element is outside the value range
            _found = (int[])_ints.Clone();
            _found[^1] = Data.Range;
            _foundList = new List<int>(_found);
            _isLast = x => x == Data.Range;
            _belowRange = x => x < Data.Range;

            _predicate = Data.Passing(Selectivity.Fifty);

            // ordered: the survivors one run at the front, so the filter's branch predicts and its masks are mostly full
            _sortedList = new List<int>(_ints);
            _sortedList.Sort();
            _ninety = Data.Passing(Selectivity.Ninety);

            // odd results
            _selector = x => x * 2 + 1;
            _add = (a, x) => a + x;
            _addLong = (a, x) => a + x;
            _half = a => (int)(a / 2);

            _arrays = new List<int[]>();
            _lists = new List<List<int>>();
            for (var i = 0; i < 100; ++i)
            {
                _arrays.Add(_ints[(i * 100)..((i + 1) * 100)]);
                _lists.Add(new List<int>(_arrays[i]));
            }
            _flattenArray = a => a;
            _flattenList = l => l;
            _short = Data.Iterator(_ints[..10]);
        }

        internal static void SanityCheck()
        {
            var check = new D();

            Check.Same(nameof(D01a_Linq), check.D01a_Linq(), check.D01a_CisternValueLinq());
            Check.Same(nameof(D01b_Linq), check.D01b_Linq(), check.D01b_CisternValueLinq());
            Check.Same(nameof(D01c_Linq), check.D01c_Linq(), check.D01c_CisternValueLinq());
            Check.Same(nameof(D01d_Linq), check.D01d_Linq(), check.D01d_CisternValueLinq());
            Check.Same(nameof(D02a_Linq), check.D02a_Linq(), check.D02a_CisternValueLinq());
            Check.Same(nameof(D02b_Linq), check.D02b_Linq(), check.D02b_CisternValueLinq());
            Check.Same(nameof(D02b_CisternValueLinqFastest), check.D02b_Linq(), check.D02b_CisternValueLinqFastest());
            Check.Same(nameof(D02c_Linq), check.D02c_Linq(), check.D02c_CisternValueLinq());
            Check.Same(nameof(D03a_Linq), check.D03a_Linq(), check.D03a_CisternValueLinq());
            Check.Same(nameof(D03b_Linq), check.D03b_Linq(), check.D03b_CisternValueLinq());
            Check.Same(nameof(D04a_Linq), check.D04a_Linq(), check.D04a_CisternValueLinq());
            Check.Same(nameof(D04b_Linq), check.D04b_Linq(), check.D04b_CisternValueLinq());
            Check.Same(nameof(D04c_Linq), check.D04c_Linq(), check.D04c_CisternValueLinq());
            Check.Same(nameof(D04d_Linq), check.D04d_Linq(), check.D04d_CisternValueLinq());
            Check.Same(nameof(D05a_Linq), check.D05a_Linq(), check.D05a_CisternValueLinq());
            Check.Same(nameof(D05b_Linq), check.D05b_Linq(), check.D05b_CisternValueLinq());
            Check.Same(nameof(D06a_Linq), check.D06a_Linq(), check.D06a_CisternValueLinq());
            Check.Same(nameof(D06b_Linq), check.D06b_Linq(), check.D06b_CisternValueLinq());
            Check.Same(nameof(D07a_Linq), check.D07a_Linq(), check.D07a_CisternValueLinq());
            Check.Same(nameof(D07b_Linq), check.D07b_Linq(), check.D07b_CisternValueLinq());
            Check.Same(nameof(D07c_Linq), check.D07c_Linq(), check.D07c_CisternValueLinq());
            Check.Same(nameof(D07d_Linq), check.D07d_Linq(), check.D07d_CisternValueLinq());
            Check.Same(nameof(D08a_Linq), check.D08a_Linq(), check.D08a_CisternValueLinq());
            Check.Same(nameof(D08b_Linq), check.D08b_Linq(), check.D08b_CisternValueLinq());
            Check.Same(nameof(D08c_Linq), check.D08c_Linq(), check.D08c_CisternValueLinq());
            Check.Same(nameof(D08d_Linq), check.D08d_Linq() ? 1 : 0, check.D08d_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(D08e_Linq), check.D08e_Linq() ? 1 : 0, check.D08e_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(D08f_Linq), check.D08f_Linq(), check.D08f_CisternValueLinq());
            Check.Same(nameof(D08g_Linq), check.D08g_Linq(), check.D08g_CisternValueLinq());
            Check.Same(nameof(D08h_Linq), check.D08h_Linq(), check.D08h_CisternValueLinq());
            Check.Same(nameof(D08i_Linq), check.D08i_Linq(), check.D08i_CisternValueLinq());
            Check.Same(nameof(D08j_Linq), check.D08j_Linq(), check.D08j_CisternValueLinq());
            Check.Same(nameof(D08k_Linq), check.D08k_Linq(), check.D08k_CisternValueLinq());
            Check.Same(nameof(D08l_Linq), check.D08l_Linq(), check.D08l_CisternValueLinq());
            Check.Same(nameof(D08m_Linq), check.D08m_Linq() ? 1 : 0, check.D08m_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(D08n_Linq), check.D08n_Linq() ? 1 : 0, check.D08n_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(D08o_Linq), check.D08o_Linq(), check.D08o_CisternValueLinq());
            Check.Same(nameof(D08p_Linq), check.D08p_Linq(), check.D08p_CisternValueLinq());
            Check.Same(nameof(D08q_Linq), check.D08q_Linq(), check.D08q_CisternValueLinq());
            Check.Same(nameof(D08r_Linq), check.D08r_Linq(), check.D08r_CisternValueLinq());
            Check.Same(nameof(D11a_Linq), check.D11a_Linq(), check.D11a_CisternValueLinq());
            Check.Same(nameof(D11b_Linq), check.D11b_Linq(), check.D11b_CisternValueLinq());
            Check.Same(nameof(D11c_Linq), check.D11c_Linq(), check.D11c_CisternValueLinq());
            Check.Same(nameof(D11d_Linq), check.D11d_Linq(), check.D11d_CisternValueLinq());
            Check.Same(nameof(D11e_Linq), check.D11e_Linq(), check.D11e_CisternValueLinq());
            Check.Same(nameof(D11f_Linq), check.D11f_Linq(), check.D11f_CisternValueLinq());
            Check.Same(nameof(D11g_Linq), check.D11g_Linq() ? 1 : 0, check.D11g_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(D11h_Linq), check.D11h_Linq(), check.D11h_CisternValueLinq());
            Check.Same(nameof(D11i_Linq), check.D11i_Linq(), check.D11i_CisternValueLinq());
            Check.Same(nameof(D11j_Linq), check.D11j_Linq(), check.D11j_CisternValueLinq());
            Check.Same(nameof(D11k_Linq), check.D11k_Linq(), check.D11k_CisternValueLinq());
            Check.Same(nameof(D11l_Linq), check.D11l_Linq(), check.D11l_CisternValueLinq());
            Check.Same(nameof(D11m_Linq), check.D11m_Linq(), check.D11m_CisternValueLinq());
            Check.Same(nameof(D11n_Linq), check.D11n_Linq() ? 1 : 0, check.D11n_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(D11o_Linq), check.D11o_Linq(), check.D11o_CisternValueLinq());
            Check.Same(nameof(D11p_Linq), check.D11p_Linq(), check.D11p_CisternValueLinq());
            Check.Same(nameof(D12a_Linq), check.D12a_Linq(), check.D12a_CisternValueLinq());
            Check.Same(nameof(D12b_Linq), check.D12b_Linq(), check.D12b_CisternValueLinq());
            Check.Same(nameof(D15a_Linq), check.D15a_Linq(), check.D15a_CisternValueLinq());
            Check.Same(nameof(D15b_Linq), check.D15b_Linq(), check.D15b_CisternValueLinq());
            Check.Same(nameof(D15c_Linq), check.D15c_Linq(), check.D15c_CisternValueLinq());

            Check.Same(nameof(D01a_ZLinq), check.D01a_Linq(), check.D01a_ZLinq());
            Check.Same(nameof(D01b_ZLinq), check.D01b_Linq(), check.D01b_ZLinq());
            Check.Same(nameof(D01c_ZLinq), check.D01c_Linq(), check.D01c_ZLinq());
            Check.Same(nameof(D01d_ZLinq), check.D01d_Linq(), check.D01d_ZLinq());
            Check.Same(nameof(D02a_ZLinq), check.D02a_Linq(), check.D02a_ZLinq());
            Check.Same(nameof(D02b_ZLinq), check.D02b_Linq(), check.D02b_ZLinq());
            Check.Same(nameof(D02c_ZLinq), check.D02c_Linq(), check.D02c_ZLinq());
            Check.Same(nameof(D03a_ZLinq), check.D03a_Linq(), check.D03a_ZLinq());
            Check.Same(nameof(D03b_ZLinq), check.D03b_Linq(), check.D03b_ZLinq());
            Check.Same(nameof(D04a_ZLinq), check.D04a_Linq(), check.D04a_ZLinq());
            Check.Same(nameof(D04b_ZLinq), check.D04b_Linq(), check.D04b_ZLinq());
            Check.Same(nameof(D04c_ZLinq), check.D04c_Linq(), check.D04c_ZLinq());
            Check.Same(nameof(D04d_ZLinq), check.D04d_Linq(), check.D04d_ZLinq());
            Check.Same(nameof(D05a_ZLinq), check.D05a_Linq(), check.D05a_ZLinq());
            Check.Same(nameof(D05b_ZLinq), check.D05b_Linq(), check.D05b_ZLinq());
            Check.Same(nameof(D06a_ZLinq), check.D06a_Linq(), check.D06a_ZLinq());
            Check.Same(nameof(D06b_ZLinq), check.D06b_Linq(), check.D06b_ZLinq());
            Check.Same(nameof(D07a_ZLinq), check.D07a_Linq(), check.D07a_ZLinq());
            Check.Same(nameof(D07b_ZLinq), check.D07b_Linq(), check.D07b_ZLinq());
            Check.Same(nameof(D07c_ZLinq), check.D07c_Linq(), check.D07c_ZLinq());
            Check.Same(nameof(D07d_ZLinq), check.D07d_Linq(), check.D07d_ZLinq());
            Check.Same(nameof(D08a_ZLinq), check.D08a_Linq(), check.D08a_ZLinq());
            Check.Same(nameof(D08b_ZLinq), check.D08b_Linq(), check.D08b_ZLinq());
            Check.Same(nameof(D08c_ZLinq), check.D08c_Linq(), check.D08c_ZLinq());
            Check.Same(nameof(D08d_ZLinq), check.D08d_Linq() ? 1 : 0, check.D08d_ZLinq() ? 1 : 0);
            Check.Same(nameof(D08e_ZLinq), check.D08e_Linq() ? 1 : 0, check.D08e_ZLinq() ? 1 : 0);
            Check.Same(nameof(D08f_ZLinq), check.D08f_Linq(), check.D08f_ZLinq());
            Check.Same(nameof(D08g_ZLinq), check.D08g_Linq(), check.D08g_ZLinq());
            Check.Same(nameof(D08h_ZLinq), check.D08h_Linq(), check.D08h_ZLinq());
            Check.Same(nameof(D08i_ZLinq), check.D08i_Linq(), check.D08i_ZLinq());
            Check.Same(nameof(D08j_ZLinq), check.D08j_Linq(), check.D08j_ZLinq());
            Check.Same(nameof(D08k_ZLinq), check.D08k_Linq(), check.D08k_ZLinq());
            Check.Same(nameof(D08l_ZLinq), check.D08l_Linq(), check.D08l_ZLinq());
            Check.Same(nameof(D08m_ZLinq), check.D08m_Linq() ? 1 : 0, check.D08m_ZLinq() ? 1 : 0);
            Check.Same(nameof(D08n_ZLinq), check.D08n_Linq() ? 1 : 0, check.D08n_ZLinq() ? 1 : 0);
            Check.Same(nameof(D08o_ZLinq), check.D08o_Linq(), check.D08o_ZLinq());
            Check.Same(nameof(D08p_ZLinq), check.D08p_Linq(), check.D08p_ZLinq());
            Check.Same(nameof(D08q_ZLinq), check.D08q_Linq(), check.D08q_ZLinq());
            Check.Same(nameof(D08r_ZLinq), check.D08r_Linq(), check.D08r_ZLinq());
            Check.Same(nameof(D11a_ZLinq), check.D11a_Linq(), check.D11a_ZLinq());
            Check.Same(nameof(D11b_ZLinq), check.D11b_Linq(), check.D11b_ZLinq());
            Check.Same(nameof(D11c_ZLinq), check.D11c_Linq(), check.D11c_ZLinq());
            Check.Same(nameof(D11d_ZLinq), check.D11d_Linq(), check.D11d_ZLinq());
            Check.Same(nameof(D11e_ZLinq), check.D11e_Linq(), check.D11e_ZLinq());
            Check.Same(nameof(D11f_ZLinq), check.D11f_Linq(), check.D11f_ZLinq());
            Check.Same(nameof(D11g_ZLinq), check.D11g_Linq() ? 1 : 0, check.D11g_ZLinq() ? 1 : 0);
            Check.Same(nameof(D11h_ZLinq), check.D11h_Linq(), check.D11h_ZLinq());
            Check.Same(nameof(D11i_ZLinq), check.D11i_Linq(), check.D11i_ZLinq());
            Check.Same(nameof(D11j_ZLinq), check.D11j_Linq(), check.D11j_ZLinq());
            Check.Same(nameof(D11k_ZLinq), check.D11k_Linq(), check.D11k_ZLinq());
            Check.Same(nameof(D11l_ZLinq), check.D11l_Linq(), check.D11l_ZLinq());
            Check.Same(nameof(D11m_ZLinq), check.D11m_Linq(), check.D11m_ZLinq());
            Check.Same(nameof(D11n_ZLinq), check.D11n_Linq() ? 1 : 0, check.D11n_ZLinq() ? 1 : 0);
            Check.Same(nameof(D11o_ZLinq), check.D11o_Linq(), check.D11o_ZLinq());
            Check.Same(nameof(D11p_ZLinq), check.D11p_Linq(), check.D11p_ZLinq());
            Check.Same(nameof(D12a_ZLinq), check.D12a_Linq(), check.D12a_ZLinq());
            Check.Same(nameof(D12b_ZLinq), check.D12b_Linq(), check.D12b_ZLinq());
            Check.Same(nameof(D15a_ZLinq), check.D15a_Linq(), check.D15a_ZLinq());
            Check.Same(nameof(D15b_ZLinq), check.D15b_Linq(), check.D15b_ZLinq());
            Check.Same(nameof(D15c_ZLinq), check.D15c_Linq(), check.D15c_ZLinq());

            Check.Same(nameof(D01a_Handcoded), check.D01a_Linq(), check.D01a_Handcoded());
            Check.Same(nameof(D01b_Handcoded), check.D01b_Linq(), check.D01b_Handcoded());
            Check.Same(nameof(D01c_Handcoded), check.D01c_Linq(), check.D01c_Handcoded());
            Check.Same(nameof(D01d_Handcoded), check.D01d_Linq(), check.D01d_Handcoded());
            Check.Same(nameof(D02a_Handcoded), check.D02a_Linq(), check.D02a_Handcoded());
            Check.Same(nameof(D02b_Handcoded), check.D02b_Linq(), check.D02b_Handcoded());
            Check.Same(nameof(D02c_Handcoded), check.D02c_Linq(), check.D02c_Handcoded());
            Check.Same(nameof(D03a_Handcoded), check.D03a_Linq(), check.D03a_Handcoded());
            Check.Same(nameof(D03b_Handcoded), check.D03b_Linq(), check.D03b_Handcoded());
            Check.Same(nameof(D04a_Handcoded), check.D04a_Linq(), check.D04a_Handcoded());
            Check.Same(nameof(D04b_Handcoded), check.D04b_Linq(), check.D04b_Handcoded());
            Check.Same(nameof(D04c_Handcoded), check.D04c_Linq(), check.D04c_Handcoded());
            Check.Same(nameof(D04d_Handcoded), check.D04d_Linq(), check.D04d_Handcoded());
            Check.Same(nameof(D05a_Handcoded), check.D05a_Linq(), check.D05a_Handcoded());
            Check.Same(nameof(D05b_Handcoded), check.D05b_Linq(), check.D05b_Handcoded());
            Check.Same(nameof(D06a_Handcoded), check.D06a_Linq(), check.D06a_Handcoded());
            Check.Same(nameof(D06b_Handcoded), check.D06b_Linq(), check.D06b_Handcoded());
            Check.Same(nameof(D07a_Handcoded), check.D07a_Linq(), check.D07a_Handcoded());
            Check.Same(nameof(D07b_Handcoded), check.D07b_Linq(), check.D07b_Handcoded());
            Check.Same(nameof(D07c_Handcoded), check.D07c_Linq(), check.D07c_Handcoded());
            Check.Same(nameof(D07d_Handcoded), check.D07d_Linq(), check.D07d_Handcoded());
            Check.Same(nameof(D08a_Handcoded), check.D08a_Linq(), check.D08a_Handcoded());
            Check.Same(nameof(D08b_Handcoded), check.D08b_Linq(), check.D08b_Handcoded());
            Check.Same(nameof(D08c_Handcoded), check.D08c_Linq(), check.D08c_Handcoded());
            Check.Same(nameof(D08d_Handcoded), check.D08d_Linq() ? 1 : 0, check.D08d_Handcoded() ? 1 : 0);
            Check.Same(nameof(D08e_Handcoded), check.D08e_Linq() ? 1 : 0, check.D08e_Handcoded() ? 1 : 0);
            Check.Same(nameof(D08f_Handcoded), check.D08f_Linq(), check.D08f_Handcoded());
            Check.Same(nameof(D08g_Handcoded), check.D08g_Linq(), check.D08g_Handcoded());
            Check.Same(nameof(D08h_Handcoded), check.D08h_Linq(), check.D08h_Handcoded());
            Check.Same(nameof(D08i_Handcoded), check.D08i_Linq(), check.D08i_Handcoded());
            Check.Same(nameof(D08j_Handcoded), check.D08j_Linq(), check.D08j_Handcoded());
            Check.Same(nameof(D08k_Handcoded), check.D08k_Linq(), check.D08k_Handcoded());
            Check.Same(nameof(D08l_Handcoded), check.D08l_Linq(), check.D08l_Handcoded());
            Check.Same(nameof(D08m_Handcoded), check.D08m_Linq() ? 1 : 0, check.D08m_Handcoded() ? 1 : 0);
            Check.Same(nameof(D08n_Handcoded), check.D08n_Linq() ? 1 : 0, check.D08n_Handcoded() ? 1 : 0);
            Check.Same(nameof(D08o_Handcoded), check.D08o_Linq(), check.D08o_Handcoded());
            Check.Same(nameof(D08p_Handcoded), check.D08p_Linq(), check.D08p_Handcoded());
            Check.Same(nameof(D08q_Handcoded), check.D08q_Linq(), check.D08q_Handcoded());
            Check.Same(nameof(D08r_Handcoded), check.D08r_Linq(), check.D08r_Handcoded());
            Check.Same(nameof(D11a_Handcoded), check.D11a_Linq(), check.D11a_Handcoded());
            Check.Same(nameof(D11b_Handcoded), check.D11b_Linq(), check.D11b_Handcoded());
            Check.Same(nameof(D11c_Handcoded), check.D11c_Linq(), check.D11c_Handcoded());
            Check.Same(nameof(D11d_Handcoded), check.D11d_Linq(), check.D11d_Handcoded());
            Check.Same(nameof(D11e_Handcoded), check.D11e_Linq(), check.D11e_Handcoded());
            Check.Same(nameof(D11f_Handcoded), check.D11f_Linq(), check.D11f_Handcoded());
            Check.Same(nameof(D11g_Handcoded), check.D11g_Linq() ? 1 : 0, check.D11g_Handcoded() ? 1 : 0);
            Check.Same(nameof(D11h_Handcoded), check.D11h_Linq(), check.D11h_Handcoded());
            Check.Same(nameof(D11i_Handcoded), check.D11i_Linq(), check.D11i_Handcoded());
            Check.Same(nameof(D11j_Handcoded), check.D11j_Linq(), check.D11j_Handcoded());
            Check.Same(nameof(D11k_Handcoded), check.D11k_Linq(), check.D11k_Handcoded());
            Check.Same(nameof(D11l_Handcoded), check.D11l_Linq(), check.D11l_Handcoded());
            Check.Same(nameof(D11m_Handcoded), check.D11m_Linq(), check.D11m_Handcoded());
            Check.Same(nameof(D11n_Handcoded), check.D11n_Linq() ? 1 : 0, check.D11n_Handcoded() ? 1 : 0);
            Check.Same(nameof(D11o_Handcoded), check.D11o_Linq(), check.D11o_Handcoded());
            Check.Same(nameof(D11p_Handcoded), check.D11p_Linq(), check.D11p_Handcoded());
            Check.Same(nameof(D12a_Handcoded), check.D12a_Linq(), check.D12a_Handcoded());
            Check.Same(nameof(D12b_Handcoded), check.D12b_Linq(), check.D12b_Handcoded());
            Check.Same(nameof(D15a_Handcoded), check.D15a_Linq(), check.D15a_Handcoded());
            Check.Same(nameof(D15b_Handcoded), check.D15b_Linq(), check.D15b_Handcoded());
            Check.Same(nameof(D15c_Handcoded), check.D15c_Linq(), check.D15c_Handcoded());
        }
    }
}
