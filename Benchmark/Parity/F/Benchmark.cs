using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section F: System.Linq's materialisation and buffering: ToArray and ToList over iterators, OfType, Skip and Take,
    // Concat, Append and Prepend, Range, collections and sets, OrderBy, GroupBy, ToLookup, joins, DefaultIfEmpty and a deferred
    // query held as IEnumerable<T>. One method per entry (a letter per source or operation where an entry covers several) and
    // library, named by the entry's ID, so a filter `*F.F01*` matches only F01. F10 is measured by B44, B65 and B71.
    [MemoryDiagnoser]
    public partial class F
    {
        const int Length = 10_000;

        int[] _ints;
        List<int> _intList;
        IEnumerable<int> _iterator;
        IEnumerable<int> _tenIterator;
        IEnumerable<int> _collection;
        IEnumerable<int> _set;
        IEnumerable<int> _emptyCollection;
        object[] _mixed;
        int[] _inner;
        Func<int, bool> _half;
        Func<int, int> _double;
        Func<int, int> _identity;
        Func<int, int> _key;
        Func<int, int, int> _add;
        Func<int, IEnumerable<int>, int> _keyOfGroup;

        public F()
        {
            _ints = Data.Values(Length);
            _intList = new List<int>(_ints);
            _iterator = Data.Iterator(_ints);
            _tenIterator = Data.Iterator(_ints[..10]);
            _collection = new Collection<int>(new List<int>(_ints));
            _set = new HashSet<int>(_ints);
            _emptyCollection = new Collection<int>(new List<int>());

            _mixed = new object[_ints.Length];
            for (var i = 0; i < _ints.Length; ++i)
                _mixed[i] = i % 2 == 0 ? _ints[i].ToString() : _ints[i];

            _inner = Data.Values(1000, Data.Seed + 1);

            _half = Data.Passing(Selectivity.Fifty);
            _double = x => x * 2;
            _identity = x => x;
            _key = x => x % 100;
            _add = (a, b) => a + b;
            _keyOfGroup = (k, g) => k;
        }

        internal static void SanityCheck()
        {
            var check = new F();

            Check.Same(nameof(F01a_Linq), check.F01a_Linq(), check.F01a_CisternValueLinq());
            Check.Same(nameof(F01b_Linq), check.F01b_Linq(), check.F01b_CisternValueLinq());
            Check.Same(nameof(F01c_Linq), check.F01c_Linq(), check.F01c_CisternValueLinq());
            Check.Same(nameof(F01d_Linq), check.F01d_Linq(), check.F01d_CisternValueLinq());
            Check.Same(nameof(F02a_Linq), check.F02a_Linq(), check.F02a_CisternValueLinq());
            Check.Same(nameof(F02b_Linq), check.F02b_Linq(), check.F02b_CisternValueLinq());
            Check.Same(nameof(F02c_Linq), check.F02c_Linq(), check.F02c_CisternValueLinq());
            Check.Same(nameof(F02d_Linq), check.F02d_Linq(), check.F02d_CisternValueLinq());
            Check.Same(nameof(F02e_Linq), check.F02e_Linq(), check.F02e_CisternValueLinq());
            Check.Same(nameof(F02f_Linq), check.F02f_Linq(), check.F02f_CisternValueLinq());
            Check.Same(nameof(F02g_Linq), check.F02g_Linq(), check.F02g_CisternValueLinq());
            Check.Same(nameof(F03a_Linq), check.F03a_Linq(), check.F03a_CisternValueLinq());
            Check.Same(nameof(F03b_Linq), check.F03b_Linq(), check.F03b_CisternValueLinq());
            Check.Same(nameof(F04a_Linq), check.F04a_Linq(), check.F04a_CisternValueLinq());
            Check.Same(nameof(F04b_Linq), check.F04b_Linq(), check.F04b_CisternValueLinq());
            Check.Same(nameof(F05a_Linq), check.F05a_Linq(), check.F05a_CisternValueLinq());
            Check.Same(nameof(F05b_Linq), check.F05b_Linq(), check.F05b_CisternValueLinq());
            Check.Same(nameof(F05c_Linq), check.F05c_Linq(), check.F05c_CisternValueLinq());
            Check.Same(nameof(F06a_Linq), check.F06a_Linq(), check.F06a_CisternValueLinq());
            Check.Same(nameof(F06b_Linq), check.F06b_Linq(), check.F06b_CisternValueLinq());
            Check.Same(nameof(F06c_Linq), check.F06c_Linq(), check.F06c_CisternValueLinq());
            Check.Same(nameof(F07a_Linq), check.F07a_Linq(), check.F07a_CisternValueLinq());
            Check.Same(nameof(F07b_Linq), check.F07b_Linq(), check.F07b_CisternValueLinq());
            Check.Same(nameof(F08a_Linq), check.F08a_Linq(), check.F08a_CisternValueLinq());
            Check.Same(nameof(F08b_Linq), check.F08b_Linq(), check.F08b_CisternValueLinq());
            Check.Same(nameof(F09_Linq), check.F09_Linq(), check.F09_CisternValueLinq());
            Check.Same(nameof(F11a_Linq), check.F11a_Linq(), check.F11a_CisternValueLinq());
            Check.Same(nameof(F11b_Linq), check.F11b_Linq(), check.F11b_CisternValueLinq());
            Check.Same(nameof(F12a_Linq), check.F12a_Linq(), check.F12a_CisternValueLinq());
            Check.Same(nameof(F12b_Linq), check.F12b_Linq(), check.F12b_CisternValueLinq());

            Check.Same(nameof(F01a_ZLinq), check.F01a_Linq(), check.F01a_ZLinq());
            Check.Same(nameof(F01b_ZLinq), check.F01b_Linq(), check.F01b_ZLinq());
            Check.Same(nameof(F01c_ZLinq), check.F01c_Linq(), check.F01c_ZLinq());
            Check.Same(nameof(F01d_ZLinq), check.F01d_Linq(), check.F01d_ZLinq());
            Check.Same(nameof(F02a_ZLinq), check.F02a_Linq(), check.F02a_ZLinq());
            Check.Same(nameof(F02b_ZLinq), check.F02b_Linq(), check.F02b_ZLinq());
            Check.Same(nameof(F02c_ZLinq), check.F02c_Linq(), check.F02c_ZLinq());
            Check.Same(nameof(F02d_ZLinq), check.F02d_Linq(), check.F02d_ZLinq());
            Check.Same(nameof(F02e_ZLinq), check.F02e_Linq(), check.F02e_ZLinq());
            Check.Same(nameof(F02f_ZLinq), check.F02f_Linq(), check.F02f_ZLinq());
            Check.Same(nameof(F02g_ZLinq), check.F02g_Linq(), check.F02g_ZLinq());
            Check.Same(nameof(F03a_ZLinq), check.F03a_Linq(), check.F03a_ZLinq());
            Check.Same(nameof(F03b_ZLinq), check.F03b_Linq(), check.F03b_ZLinq());
            Check.Same(nameof(F04a_ZLinq), check.F04a_Linq(), check.F04a_ZLinq());
            Check.Same(nameof(F04b_ZLinq), check.F04b_Linq(), check.F04b_ZLinq());
            Check.Same(nameof(F05a_ZLinq), check.F05a_Linq(), check.F05a_ZLinq());
            Check.Same(nameof(F05b_ZLinq), check.F05b_Linq(), check.F05b_ZLinq());
            Check.Same(nameof(F05c_ZLinq), check.F05c_Linq(), check.F05c_ZLinq());
            Check.Same(nameof(F06a_ZLinq), check.F06a_Linq(), check.F06a_ZLinq());
            Check.Same(nameof(F06b_ZLinq), check.F06b_Linq(), check.F06b_ZLinq());
            Check.Same(nameof(F06c_ZLinq), check.F06c_Linq(), check.F06c_ZLinq());
            Check.Same(nameof(F07a_ZLinq), check.F07a_Linq(), check.F07a_ZLinq());
            Check.Same(nameof(F07b_ZLinq), check.F07b_Linq(), check.F07b_ZLinq());
            Check.Same(nameof(F08a_ZLinq), check.F08a_Linq(), check.F08a_ZLinq());
            Check.Same(nameof(F08b_ZLinq), check.F08b_Linq(), check.F08b_ZLinq());
            Check.Same(nameof(F09_ZLinq), check.F09_Linq(), check.F09_ZLinq());
            Check.Same(nameof(F11a_ZLinq), check.F11a_Linq(), check.F11a_ZLinq());
            Check.Same(nameof(F11b_ZLinq), check.F11b_Linq(), check.F11b_ZLinq());
            Check.Same(nameof(F12a_ZLinq), check.F12a_Linq(), check.F12a_ZLinq());
            Check.Same(nameof(F12b_ZLinq), check.F12b_Linq(), check.F12b_ZLinq());

            Check.Same(nameof(F01a_Handcoded), check.F01a_Linq(), check.F01a_Handcoded());
            Check.Same(nameof(F01b_Handcoded), check.F01b_Linq(), check.F01b_Handcoded());
            Check.Same(nameof(F01c_Handcoded), check.F01c_Linq(), check.F01c_Handcoded());
            Check.Same(nameof(F01d_Handcoded), check.F01d_Linq(), check.F01d_Handcoded());
            Check.Same(nameof(F02a_Handcoded), check.F02a_Linq(), check.F02a_Handcoded());
            Check.Same(nameof(F02b_Handcoded), check.F02b_Linq(), check.F02b_Handcoded());
            Check.Same(nameof(F02c_Handcoded), check.F02c_Linq(), check.F02c_Handcoded());
            Check.Same(nameof(F02d_Handcoded), check.F02d_Linq(), check.F02d_Handcoded());
            Check.Same(nameof(F02e_Handcoded), check.F02e_Linq(), check.F02e_Handcoded());
            Check.Same(nameof(F02f_Handcoded), check.F02f_Linq(), check.F02f_Handcoded());
            Check.Same(nameof(F02g_Handcoded), check.F02g_Linq(), check.F02g_Handcoded());
            Check.Same(nameof(F03a_Handcoded), check.F03a_Linq(), check.F03a_Handcoded());
            Check.Same(nameof(F03b_Handcoded), check.F03b_Linq(), check.F03b_Handcoded());
            Check.Same(nameof(F04a_Handcoded), check.F04a_Linq(), check.F04a_Handcoded());
            Check.Same(nameof(F04b_Handcoded), check.F04b_Linq(), check.F04b_Handcoded());
            Check.Same(nameof(F05a_Handcoded), check.F05a_Linq(), check.F05a_Handcoded());
            Check.Same(nameof(F05b_Handcoded), check.F05b_Linq(), check.F05b_Handcoded());
            Check.Same(nameof(F05c_Handcoded), check.F05c_Linq(), check.F05c_Handcoded());
            Check.Same(nameof(F06a_Handcoded), check.F06a_Linq(), check.F06a_Handcoded());
            Check.Same(nameof(F06b_Handcoded), check.F06b_Linq(), check.F06b_Handcoded());
            Check.Same(nameof(F06c_Handcoded), check.F06c_Linq(), check.F06c_Handcoded());
            Check.Same(nameof(F07a_Handcoded), check.F07a_Linq(), check.F07a_Handcoded());
            Check.Same(nameof(F07b_Handcoded), check.F07b_Linq(), check.F07b_Handcoded());
            Check.Same(nameof(F08a_Handcoded), check.F08a_Linq(), check.F08a_Handcoded());
            Check.Same(nameof(F08b_Handcoded), check.F08b_Linq(), check.F08b_Handcoded());
            Check.Same(nameof(F09_Handcoded), check.F09_Linq(), check.F09_Handcoded());
            Check.Same(nameof(F11a_Handcoded), check.F11a_Linq(), check.F11a_Handcoded());
            Check.Same(nameof(F11b_Handcoded), check.F11b_Linq(), check.F11b_Handcoded());
            Check.Same(nameof(F12a_Handcoded), check.F12a_Linq(), check.F12a_Handcoded());
            Check.Same(nameof(F12b_Handcoded), check.F12b_Linq(), check.F12b_Handcoded());
        }
    }
}
