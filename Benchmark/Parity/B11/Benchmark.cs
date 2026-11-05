using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.11: DefaultIfEmpty, Cast, SelectMany, GroupBy and the lookup, and the Join family, where
    // System.Linq answers first, last, element-at, count, contain and copy requests without buffering or running user
    // code. One method per entry (a letter per operation where an entry covers several) and library, named by the
    // entry's ID. The grouping entries read a grouping and a lookup each library built once, over about ten groups of a
    // thousand; the Join entries use an empty side, where System.Linq never calls the key selector for the other, and
    // a FullJoin whose every grouping matches, where it skips the pass over the unmatched.
    [MemoryDiagnoser]
    public partial class B11
    {
        const int Length = 10_000;

        int[] _array;
        List<int> _list;
        HashSet<int> _set;
        List<int> _empty;
        int[] _emptyArray;
        System.Collections.ArrayList _arrayList;
        int[][] _jagged;
        Func<int, int> _keySelector;
        Func<int, IEnumerable<int>, int> _resultSelector;
        int _present;
        int _presentInJagged;
        int _inGroup;

        public B11()
        {
            _array = Data.Values(Length);
            _list = new List<int>(_array);
            _set = new HashSet<int>(_array);
            _empty = new List<int>();
            _emptyArray = new int[0];
            _arrayList = new System.Collections.ArrayList(_array);

            _jagged = new int[100][];
            for (var i = 0; i < _jagged.Length; ++i)
                _jagged[i] = _array[(i * 100)..(i * 100 + 100)];

            _keySelector = x => x % 100;
            _resultSelector = (key, group) => key;

            // near the end of the data
            _present = _array[Length - 10];
            _presentInJagged = _array[Length - 10];

            _inGroup = _array.Last(x => x % 10 == 3);

            InitLinq();
            InitCistern();
            InitZLinq();
        }

        internal static void SanityCheck()
        {
            var check = new B11();

            Check.Same(nameof(B78a_Linq), check.B78a_Linq(), check.B78a_CisternValueLinq());
            Check.Same(nameof(B78b_Linq), check.B78b_Linq(), check.B78b_CisternValueLinq());
            Check.Same(nameof(B78c_Linq), check.B78c_Linq(), check.B78c_CisternValueLinq());
            Check.Same(nameof(B78d_Linq), check.B78d_Linq(), check.B78d_CisternValueLinq());
            Check.Same(nameof(B78e_Linq), check.B78e_Linq(), check.B78e_CisternValueLinq());
            Check.Same(nameof(B78f_Linq), check.B78f_Linq(), check.B78f_CisternValueLinq());
            Check.Same(nameof(B79a_Linq), check.B79a_Linq() ? 1 : 0, check.B79a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B79b_Linq), check.B79b_Linq() ? 1 : 0, check.B79b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B80a_Linq), check.B80a_Linq(), check.B80a_CisternValueLinq());
            Check.Same(nameof(B80b_Linq), check.B80b_Linq(), check.B80b_CisternValueLinq());
            Check.Same(nameof(B80c_Linq), check.B80c_Linq(), check.B80c_CisternValueLinq());
            Check.Same(nameof(B80d_Linq), check.B80d_Linq(), check.B80d_CisternValueLinq());
            Check.Same(nameof(B81a_Linq), check.B81a_Linq(), check.B81a_CisternValueLinq());
            Check.Same(nameof(B81b_Linq), check.B81b_Linq() ? 1 : 0, check.B81b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B81c_Linq), check.B81c_Linq(), check.B81c_CisternValueLinq());
            Check.Same(nameof(B81d_Linq), check.B81d_Linq(), check.B81d_CisternValueLinq());
            Check.Same(nameof(B82a_Linq), check.B82a_Linq(), check.B82a_CisternValueLinq());
            Check.Same(nameof(B82b_Linq), check.B82b_Linq(), check.B82b_CisternValueLinq());
            Check.Same(nameof(B82c_Linq), check.B82c_Linq(), check.B82c_CisternValueLinq());
            Check.Same(nameof(B82d_Linq), check.B82d_Linq(), check.B82d_CisternValueLinq());
            Check.Same(nameof(B82e_Linq), check.B82e_Linq(), check.B82e_CisternValueLinq());
            Check.Same(nameof(B82f_Linq), check.B82f_Linq() ? 1 : 0, check.B82f_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B82g_Linq), check.B82g_Linq(), check.B82g_CisternValueLinq());
            Check.Same(nameof(B82h_Linq), check.B82h_Linq(), check.B82h_CisternValueLinq());
            Check.Same(nameof(B82i_Linq), check.B82i_Linq(), check.B82i_CisternValueLinq());
            Check.Same(nameof(B82j_Linq), check.B82j_Linq(), check.B82j_CisternValueLinq());
            Check.Same(nameof(B82k_Linq), check.B82k_Linq(), check.B82k_CisternValueLinq());
            Check.Same(nameof(B82l_Linq), check.B82l_Linq(), check.B82l_CisternValueLinq());
            Check.Same(nameof(B82m_Linq), check.B82m_Linq(), check.B82m_CisternValueLinq());
            Check.Same(nameof(B82n_Linq), check.B82n_Linq(), check.B82n_CisternValueLinq());
            Check.Same(nameof(B82o_Linq), check.B82o_Linq(), check.B82o_CisternValueLinq());
            Check.Same(nameof(B82p_Linq), check.B82p_Linq(), check.B82p_CisternValueLinq());
            Check.Same(nameof(B82q_Linq), check.B82q_Linq(), check.B82q_CisternValueLinq());
            Check.Same(nameof(B82r_Linq), check.B82r_Linq(), check.B82r_CisternValueLinq());

            Check.Same(nameof(B78a_ZLinq), check.B78a_Linq(), check.B78a_ZLinq());
            Check.Same(nameof(B78b_ZLinq), check.B78b_Linq(), check.B78b_ZLinq());
            Check.Same(nameof(B78c_ZLinq), check.B78c_Linq(), check.B78c_ZLinq());
            Check.Same(nameof(B78d_ZLinq), check.B78d_Linq(), check.B78d_ZLinq());
            Check.Same(nameof(B78e_ZLinq), check.B78e_Linq(), check.B78e_ZLinq());
            Check.Same(nameof(B78f_ZLinq), check.B78f_Linq(), check.B78f_ZLinq());
            Check.Same(nameof(B79a_ZLinq), check.B79a_Linq() ? 1 : 0, check.B79a_ZLinq() ? 1 : 0);
            Check.Same(nameof(B79b_ZLinq), check.B79b_Linq() ? 1 : 0, check.B79b_ZLinq() ? 1 : 0);
            Check.Same(nameof(B80a_ZLinq), check.B80a_Linq(), check.B80a_ZLinq());
            Check.Same(nameof(B80b_ZLinq), check.B80b_Linq(), check.B80b_ZLinq());
            Check.Same(nameof(B80c_ZLinq), check.B80c_Linq(), check.B80c_ZLinq());
            Check.Same(nameof(B80d_ZLinq), check.B80d_Linq(), check.B80d_ZLinq());
            Check.Same(nameof(B81a_ZLinq), check.B81a_Linq(), check.B81a_ZLinq());
            Check.Same(nameof(B81b_ZLinq), check.B81b_Linq() ? 1 : 0, check.B81b_ZLinq() ? 1 : 0);
            Check.Same(nameof(B81c_ZLinq), check.B81c_Linq(), check.B81c_ZLinq());
            Check.Same(nameof(B81d_ZLinq), check.B81d_Linq(), check.B81d_ZLinq());
            Check.Same(nameof(B82a_ZLinq), check.B82a_Linq(), check.B82a_ZLinq());
            Check.Same(nameof(B82b_ZLinq), check.B82b_Linq(), check.B82b_ZLinq());
            Check.Same(nameof(B82c_ZLinq), check.B82c_Linq(), check.B82c_ZLinq());
            Check.Same(nameof(B82d_ZLinq), check.B82d_Linq(), check.B82d_ZLinq());
            Check.Same(nameof(B82e_ZLinq), check.B82e_Linq(), check.B82e_ZLinq());
            Check.Same(nameof(B82f_ZLinq), check.B82f_Linq() ? 1 : 0, check.B82f_ZLinq() ? 1 : 0);
            Check.Same(nameof(B82g_ZLinq), check.B82g_Linq(), check.B82g_ZLinq());
            Check.Same(nameof(B82h_ZLinq), check.B82h_Linq(), check.B82h_ZLinq());
            Check.Same(nameof(B82i_ZLinq), check.B82i_Linq(), check.B82i_ZLinq());
            Check.Same(nameof(B82j_ZLinq), check.B82j_Linq(), check.B82j_ZLinq());
            Check.Same(nameof(B82k_ZLinq), check.B82k_Linq(), check.B82k_ZLinq());
            Check.Same(nameof(B82l_ZLinq), check.B82l_Linq(), check.B82l_ZLinq());
            Check.Same(nameof(B82m_ZLinq), check.B82m_Linq(), check.B82m_ZLinq());
            Check.Same(nameof(B82n_ZLinq), check.B82n_Linq(), check.B82n_ZLinq());
            Check.Same(nameof(B82o_ZLinq), check.B82o_Linq(), check.B82o_ZLinq());
            Check.Same(nameof(B82p_ZLinq), check.B82p_Linq(), check.B82p_ZLinq());

            Check.Same(nameof(B78a_Handcoded), check.B78a_Linq(), check.B78a_Handcoded());
            Check.Same(nameof(B78b_Handcoded), check.B78b_Linq(), check.B78b_Handcoded());
            Check.Same(nameof(B78c_Handcoded), check.B78c_Linq(), check.B78c_Handcoded());
            Check.Same(nameof(B78d_Handcoded), check.B78d_Linq(), check.B78d_Handcoded());
            Check.Same(nameof(B78e_Handcoded), check.B78e_Linq(), check.B78e_Handcoded());
            Check.Same(nameof(B78f_Handcoded), check.B78f_Linq(), check.B78f_Handcoded());
            Check.Same(nameof(B79a_Handcoded), check.B79a_Linq() ? 1 : 0, check.B79a_Handcoded() ? 1 : 0);
            Check.Same(nameof(B79b_Handcoded), check.B79b_Linq() ? 1 : 0, check.B79b_Handcoded() ? 1 : 0);
            Check.Same(nameof(B80a_Handcoded), check.B80a_Linq(), check.B80a_Handcoded());
            Check.Same(nameof(B80b_Handcoded), check.B80b_Linq(), check.B80b_Handcoded());
            Check.Same(nameof(B80c_Handcoded), check.B80c_Linq(), check.B80c_Handcoded());
            Check.Same(nameof(B80d_Handcoded), check.B80d_Linq(), check.B80d_Handcoded());
            Check.Same(nameof(B81a_Handcoded), check.B81a_Linq(), check.B81a_Handcoded());
            Check.Same(nameof(B81b_Handcoded), check.B81b_Linq() ? 1 : 0, check.B81b_Handcoded() ? 1 : 0);
            Check.Same(nameof(B81c_Handcoded), check.B81c_Linq(), check.B81c_Handcoded());
            Check.Same(nameof(B81d_Handcoded), check.B81d_Linq(), check.B81d_Handcoded());
            Check.Same(nameof(B82a_Handcoded), check.B82a_Linq(), check.B82a_Handcoded());
            Check.Same(nameof(B82b_Handcoded), check.B82b_Linq(), check.B82b_Handcoded());
            Check.Same(nameof(B82c_Handcoded), check.B82c_Linq(), check.B82c_Handcoded());
            Check.Same(nameof(B82d_Handcoded), check.B82d_Linq(), check.B82d_Handcoded());
            Check.Same(nameof(B82e_Handcoded), check.B82e_Linq(), check.B82e_Handcoded());
            Check.Same(nameof(B82f_Handcoded), check.B82f_Linq() ? 1 : 0, check.B82f_Handcoded() ? 1 : 0);
            Check.Same(nameof(B82g_Handcoded), check.B82g_Linq(), check.B82g_Handcoded());
            Check.Same(nameof(B82h_Handcoded), check.B82h_Linq(), check.B82h_Handcoded());
            Check.Same(nameof(B82i_Handcoded), check.B82i_Linq(), check.B82i_Handcoded());
            Check.Same(nameof(B82j_Handcoded), check.B82j_Linq(), check.B82j_Handcoded());
            Check.Same(nameof(B82k_Handcoded), check.B82k_Linq(), check.B82k_Handcoded());
            Check.Same(nameof(B82l_Handcoded), check.B82l_Linq(), check.B82l_Handcoded());
            Check.Same(nameof(B82m_Handcoded), check.B82m_Linq(), check.B82m_Handcoded());
            Check.Same(nameof(B82n_Handcoded), check.B82n_Linq(), check.B82n_Handcoded());
            Check.Same(nameof(B82o_Handcoded), check.B82o_Linq(), check.B82o_Handcoded());
            Check.Same(nameof(B82p_Handcoded), check.B82p_Linq(), check.B82p_Handcoded());
            Check.Same(nameof(B82q_Handcoded), check.B82q_Linq(), check.B82q_Handcoded());
            Check.Same(nameof(B82r_Handcoded), check.B82r_Linq(), check.B82r_Handcoded());

            // the entries whose saving is a key selector not run: Cistern should call it no more often than System.Linq
            Action<Func<int, int>> useKey = k => check._keySelector = k;
            Check.NoMoreCalls(nameof(B82l_Linq), check._keySelector, useKey, () => check.B82l_Linq(), () => check.B82l_CisternValueLinq());
            Check.NoMoreCalls(nameof(B82m_Linq), check._keySelector, useKey, () => check.B82m_Linq(), () => check.B82m_CisternValueLinq());
            Check.NoMoreCalls(nameof(B82n_Linq), check._keySelector, useKey, () => check.B82n_Linq(), () => check.B82n_CisternValueLinq());
            Check.NoMoreCalls(nameof(B82o_Linq), check._keySelector, useKey, () => check.B82o_Linq(), () => check.B82o_CisternValueLinq());
            Check.NoMoreCalls(nameof(B82p_Linq), check._keySelector, useKey, () => check.B82p_Linq(), () => check.B82p_CisternValueLinq());
            Check.NoMoreCalls(nameof(B82q_Linq), check._keySelector, useKey, () => check.B82q_Linq(), () => check.B82q_CisternValueLinq());

            // the same for B82b's result selector, which takes two arguments
            var resultSelector = check._resultSelector;
            var calls = 0;
            check._resultSelector = (key, group) => { ++calls; return resultSelector(key, group); };
            check.B82b_Linq();
            var linqCalls = calls;
            calls = 0;
            check.B82b_CisternValueLinq();
            check._resultSelector = resultSelector;

            if (calls > linqCalls)
                Console.WriteLine($"PARITY {nameof(B82b_Linq)}: System.Linq calls the user code {linqCalls} times, Cistern {calls}");
        }
    }
}
