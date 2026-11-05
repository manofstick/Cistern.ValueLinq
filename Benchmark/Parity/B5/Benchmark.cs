using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.5: the OrderBy family, where System.Linq orders only as much as the question needs (First, Last,
    // ElementAt, Skip/Take, Count) or skips the sort. One method per entry (a letter per operation where an entry covers
    // several) and library, named by the entry's ID.
    [MemoryDiagnoser]
    public partial class B5
    {
        const int Length = 10_000;

        int[] _values;
        List<int> _list;
        int[] _single;
        string[] _strings;
        int _present;

        Func<int, int> _key;
        Func<int, int> _tiedKey;
        Func<string, string> _stringKey;
        Func<int, bool> _predicate;

        public B5()
        {
            _values = Data.Values(Length);
            _list = new List<int>(_values);
            _single = new[] { _values[0] };
            _present = _values[Length / 2];

            var random = new Random(Data.Seed);
            _strings = new string[Length];
            for (var i = 0; i < _strings.Length; ++i)
                _strings[i] = random.Next().ToString("x8");

            // _tiedKey leaves about 100 elements to each key, so the ThenBy key decides most comparisons
            _key = x => x ^ 0x155;
            _tiedKey = x => x % 100;
            _stringKey = s => s;
            _predicate = Data.Passing(Selectivity.Fifty);
        }

        internal static void SanityCheck()
        {
            var check = new B5();

            Check.Same(nameof(B34a_Linq), check.B34a_Linq(), check.B34a_CisternValueLinq());
            Check.Same(nameof(B34b_Linq), check.B34b_Linq(), check.B34b_CisternValueLinq());
            Check.Same(nameof(B34c_Linq), check.B34c_Linq(), check.B34c_CisternValueLinq());
            Check.Same(nameof(B34d_Linq), check.B34d_Linq(), check.B34d_CisternValueLinq());
            Check.Same(nameof(B35a_Linq), check.B35a_Linq(), check.B35a_CisternValueLinq());
            Check.Same(nameof(B35b_Linq), check.B35b_Linq(), check.B35b_CisternValueLinq());
            Check.Same(nameof(B36a_Linq), check.B36a_Linq(), check.B36a_CisternValueLinq());
            Check.Same(nameof(B36b_Linq), check.B36b_Linq(), check.B36b_CisternValueLinq());
            Check.Same(nameof(B37_Linq), check.B37_Linq(), check.B37_CisternValueLinq());
            Check.Same(nameof(B38_Linq), check.B38_Linq(), check.B38_CisternValueLinq());
            Check.Same(nameof(B39a_Linq), check.B39a_Linq(), check.B39a_CisternValueLinq());
            Check.Same(nameof(B39b_Linq), check.B39b_Linq(), check.B39b_CisternValueLinq());
            Check.Same(nameof(B39c_Linq), check.B39c_Linq(), check.B39c_CisternValueLinq());
            Check.Same(nameof(B39d_Linq), check.B39d_Linq(), check.B39d_CisternValueLinq());
            Check.Same(nameof(B39e_Linq), check.B39e_Linq(), check.B39e_CisternValueLinq());
            Check.Same(nameof(B39f_Linq), check.B39f_Linq(), check.B39f_CisternValueLinq());
            Check.Same(nameof(B40a_Linq), check.B40a_Linq(), check.B40a_CisternValueLinq());
            Check.Same(nameof(B40b_Linq), check.B40b_Linq(), check.B40b_CisternValueLinq());
            Check.Same(nameof(B41_Linq), check.B41_Linq(), check.B41_CisternValueLinq());
            Check.Same(nameof(B42a_Linq), check.B42a_Linq(), check.B42a_CisternValueLinq());
            Check.Same(nameof(B42b_Linq), check.B42b_Linq(), check.B42b_CisternValueLinq());
            Check.Same(nameof(B42c_Linq), check.B42c_Linq(), check.B42c_CisternValueLinq());
            Check.Same(nameof(B42d_Linq), check.B42d_Linq(), check.B42d_CisternValueLinq());
            Check.Same(nameof(B43a_Linq), check.B43a_Linq(), check.B43a_CisternValueLinq());
            Check.Same(nameof(B43b_Linq), check.B43b_Linq(), check.B43b_CisternValueLinq());
            Check.Same(nameof(B44a_Linq), check.B44a_Linq(), check.B44a_CisternValueLinq());
            Check.Same(nameof(B44b_Linq), check.B44b_Linq(), check.B44b_CisternValueLinq());
            Check.Same(nameof(B45a_Linq), check.B45a_Linq(), check.B45a_CisternValueLinq());
            Check.Same(nameof(B45b_Linq), check.B45b_Linq(), check.B45b_CisternValueLinq());
            Check.Same(nameof(B45c_Linq), check.B45c_Linq() ? 1 : 0, check.B45c_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B46_Linq), check.B46_Linq() ? 1 : 0, check.B46_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B47a_Linq), check.B47a_Linq(), check.B47a_CisternValueLinq());
            Check.Same(nameof(B47b_Linq), check.B47b_Linq(), check.B47b_CisternValueLinq());
            Check.Same(nameof(B48a_Linq), check.B48a_Linq(), check.B48a_CisternValueLinq());
            Check.Same(nameof(B48b_Linq), check.B48b_Linq(), check.B48b_CisternValueLinq());

            Check.Same(nameof(B34a_ZLinq), check.B34a_Linq(), check.B34a_ZLinq());
            Check.Same(nameof(B34b_ZLinq), check.B34b_Linq(), check.B34b_ZLinq());
            Check.Same(nameof(B34c_ZLinq), check.B34c_Linq(), check.B34c_ZLinq());
            Check.Same(nameof(B34d_ZLinq), check.B34d_Linq(), check.B34d_ZLinq());
            Check.Same(nameof(B35a_ZLinq), check.B35a_Linq(), check.B35a_ZLinq());
            Check.Same(nameof(B35b_ZLinq), check.B35b_Linq(), check.B35b_ZLinq());
            Check.Same(nameof(B36a_ZLinq), check.B36a_Linq(), check.B36a_ZLinq());
            Check.Same(nameof(B36b_ZLinq), check.B36b_Linq(), check.B36b_ZLinq());
            Check.Same(nameof(B37_ZLinq), check.B37_Linq(), check.B37_ZLinq());
            Check.Same(nameof(B38_ZLinq), check.B38_Linq(), check.B38_ZLinq());
            Check.Same(nameof(B39a_ZLinq), check.B39a_Linq(), check.B39a_ZLinq());
            Check.Same(nameof(B39b_ZLinq), check.B39b_Linq(), check.B39b_ZLinq());
            Check.Same(nameof(B39c_ZLinq), check.B39c_Linq(), check.B39c_ZLinq());
            Check.Same(nameof(B39d_ZLinq), check.B39d_Linq(), check.B39d_ZLinq());
            Check.Same(nameof(B39e_ZLinq), check.B39e_Linq(), check.B39e_ZLinq());
            Check.Same(nameof(B39f_ZLinq), check.B39f_Linq(), check.B39f_ZLinq());
            Check.Same(nameof(B40a_ZLinq), check.B40a_Linq(), check.B40a_ZLinq());
            Check.Same(nameof(B40b_ZLinq), check.B40b_Linq(), check.B40b_ZLinq());
            Check.Same(nameof(B41_ZLinq), check.B41_Linq(), check.B41_ZLinq());
            Check.Same(nameof(B42a_ZLinq), check.B42a_Linq(), check.B42a_ZLinq());
            Check.Same(nameof(B42b_ZLinq), check.B42b_Linq(), check.B42b_ZLinq());
            Check.Same(nameof(B42c_ZLinq), check.B42c_Linq(), check.B42c_ZLinq());
            Check.Same(nameof(B42d_ZLinq), check.B42d_Linq(), check.B42d_ZLinq());
            Check.Same(nameof(B43a_ZLinq), check.B43a_Linq(), check.B43a_ZLinq());
            Check.Same(nameof(B43b_ZLinq), check.B43b_Linq(), check.B43b_ZLinq());
            Check.Same(nameof(B44a_ZLinq), check.B44a_Linq(), check.B44a_ZLinq());
            Check.Same(nameof(B44b_ZLinq), check.B44b_Linq(), check.B44b_ZLinq());
            Check.Same(nameof(B45a_ZLinq), check.B45a_Linq(), check.B45a_ZLinq());
            Check.Same(nameof(B45b_ZLinq), check.B45b_Linq(), check.B45b_ZLinq());
            Check.Same(nameof(B45c_ZLinq), check.B45c_Linq() ? 1 : 0, check.B45c_ZLinq() ? 1 : 0);
            Check.Same(nameof(B46_ZLinq), check.B46_Linq() ? 1 : 0, check.B46_ZLinq() ? 1 : 0);
            Check.Same(nameof(B47a_ZLinq), check.B47a_Linq(), check.B47a_ZLinq());
            Check.Same(nameof(B47b_ZLinq), check.B47b_Linq(), check.B47b_ZLinq());
            Check.Same(nameof(B48a_ZLinq), check.B48a_Linq(), check.B48a_ZLinq());
            Check.Same(nameof(B48b_ZLinq), check.B48b_Linq(), check.B48b_ZLinq());

            Check.Same(nameof(B34a_Handcoded), check.B34a_Linq(), check.B34a_Handcoded());
            Check.Same(nameof(B34b_Handcoded), check.B34b_Linq(), check.B34b_Handcoded());
            Check.Same(nameof(B34c_Handcoded), check.B34c_Linq(), check.B34c_Handcoded());
            Check.Same(nameof(B34d_Handcoded), check.B34d_Linq(), check.B34d_Handcoded());
            Check.Same(nameof(B35a_Handcoded), check.B35a_Linq(), check.B35a_Handcoded());
            Check.Same(nameof(B35b_Handcoded), check.B35b_Linq(), check.B35b_Handcoded());
            Check.Same(nameof(B36a_Handcoded), check.B36a_Linq(), check.B36a_Handcoded());
            Check.Same(nameof(B36b_Handcoded), check.B36b_Linq(), check.B36b_Handcoded());
            Check.Same(nameof(B37_Handcoded), check.B37_Linq(), check.B37_Handcoded());
            Check.Same(nameof(B38_Handcoded), check.B38_Linq(), check.B38_Handcoded());
            Check.Same(nameof(B39a_Handcoded), check.B39a_Linq(), check.B39a_Handcoded());
            Check.Same(nameof(B39b_Handcoded), check.B39b_Linq(), check.B39b_Handcoded());
            Check.Same(nameof(B39c_Handcoded), check.B39c_Linq(), check.B39c_Handcoded());
            Check.Same(nameof(B39d_Handcoded), check.B39d_Linq(), check.B39d_Handcoded());
            Check.Same(nameof(B39e_Handcoded), check.B39e_Linq(), check.B39e_Handcoded());
            Check.Same(nameof(B39f_Handcoded), check.B39f_Linq(), check.B39f_Handcoded());
            Check.Same(nameof(B40a_Handcoded), check.B40a_Linq(), check.B40a_Handcoded());
            Check.Same(nameof(B40b_Handcoded), check.B40b_Linq(), check.B40b_Handcoded());
            Check.Same(nameof(B41_Handcoded), check.B41_Linq(), check.B41_Handcoded());
            Check.Same(nameof(B42a_Handcoded), check.B42a_Linq(), check.B42a_Handcoded());
            Check.Same(nameof(B42b_Handcoded), check.B42b_Linq(), check.B42b_Handcoded());
            Check.Same(nameof(B42c_Handcoded), check.B42c_Linq(), check.B42c_Handcoded());
            Check.Same(nameof(B42d_Handcoded), check.B42d_Linq(), check.B42d_Handcoded());
            Check.Same(nameof(B43a_Handcoded), check.B43a_Linq(), check.B43a_Handcoded());
            Check.Same(nameof(B43b_Handcoded), check.B43b_Linq(), check.B43b_Handcoded());
            Check.Same(nameof(B44a_Handcoded), check.B44a_Linq(), check.B44a_Handcoded());
            Check.Same(nameof(B44b_Handcoded), check.B44b_Linq(), check.B44b_Handcoded());
            Check.Same(nameof(B45a_Handcoded), check.B45a_Linq(), check.B45a_Handcoded());
            Check.Same(nameof(B45b_Handcoded), check.B45b_Linq(), check.B45b_Handcoded());
            Check.Same(nameof(B45c_Handcoded), check.B45c_Linq() ? 1 : 0, check.B45c_Handcoded() ? 1 : 0);
            Check.Same(nameof(B46_Handcoded), check.B46_Linq() ? 1 : 0, check.B46_Handcoded() ? 1 : 0);
            Check.Same(nameof(B47a_Handcoded), check.B47a_Linq(), check.B47a_Handcoded());
            Check.Same(nameof(B47b_Handcoded), check.B47b_Linq(), check.B47b_Handcoded());
            Check.Same(nameof(B48a_Handcoded), check.B48a_Linq(), check.B48a_Handcoded());
            Check.Same(nameof(B48b_Handcoded), check.B48b_Linq(), check.B48b_Handcoded());

            // the entries that save key-selector calls: Cistern should call them no more often than System.Linq
            Action<Func<int, int>> useKey = key => check._key = key;
            Check.NoMoreCalls(nameof(B34a_Linq), check._key, useKey, () => check.B34a_Linq(), () => check.B34a_CisternValueLinq());
            Check.NoMoreCalls(nameof(B34b_Linq), check._key, useKey, () => check.B34b_Linq(), () => check.B34b_CisternValueLinq());
            Check.NoMoreCalls(nameof(B34c_Linq), check._key, useKey, () => check.B34c_Linq(), () => check.B34c_CisternValueLinq());
            Check.NoMoreCalls(nameof(B34d_Linq), check._key, useKey, () => check.B34d_Linq(), () => check.B34d_CisternValueLinq());
            Check.NoMoreCalls(nameof(B35a_Linq), check._key, useKey, () => check.B35a_Linq(), () => check.B35a_CisternValueLinq());
            Check.NoMoreCalls(nameof(B35b_Linq), check._key, useKey, () => check.B35b_Linq(), () => check.B35b_CisternValueLinq());
            Check.NoMoreCalls(nameof(B45a_Linq), check._key, useKey, () => check.B45a_Linq(), () => check.B45a_CisternValueLinq());
            Check.NoMoreCalls(nameof(B45b_Linq), check._key, useKey, () => check.B45b_Linq(), () => check.B45b_CisternValueLinq());
            Check.NoMoreCalls(nameof(B45c_Linq), check._key, useKey, () => check.B45c_Linq(), () => check.B45c_CisternValueLinq());
            Check.NoMoreCalls(nameof(B47a_Linq), check._key, useKey, () => check.B47a_Linq(), () => check.B47a_CisternValueLinq());
            Check.NoMoreCalls(nameof(B47b_Linq), check._key, useKey, () => check.B47b_Linq(), () => check.B47b_CisternValueLinq());
            // B36 saves calls of the ThenBy key, run only when the primary keys tie
            Check.NoMoreCalls(nameof(B36a_Linq), check._key, useKey, () => check.B36a_Linq(), () => check.B36a_CisternValueLinq());
            Check.NoMoreCalls(nameof(B36b_Linq), check._key, useKey, () => check.B36b_Linq(), () => check.B36b_CisternValueLinq());
        }
    }
}
