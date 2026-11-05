using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.10: Distinct and Union, where System.Linq's set operators first, any, contain, count and copy
    // without buffering the whole result. One method per entry (a letter per operation where an entry covers several)
    // and library, named by the entry's ID. The values are drawn from [0, Data.Range), so the first source holds many
    // duplicates, and the second array is shifted by Data.Range, so it shares no value with the first.
    [MemoryDiagnoser]
    public partial class B10
    {
        const int Length = 10_000;

        int[] _array;
        List<int> _list;
        HashSet<int> _set;
        int[] _second;
        HashSet<int> _secondSet;
        int _present;
        int _presentInSecond;

        public B10()
        {
            _array = Data.Values(Length);
            _list = new List<int>(_array);
            _set = new HashSet<int>(_array);

            _second = Data.Values(Length, Data.Seed + 1);
            for (var i = 0; i < _second.Length; ++i)
                _second[i] += Data.Range;
            _secondSet = new HashSet<int>(_second);

            // near the end of the data
            _present = _array[Length - 10];
            _presentInSecond = _second[Length - 10];
        }

        internal static void SanityCheck()
        {
            var check = new B10();

            Check.Same(nameof(B72a_Linq), check.B72a_Linq(), check.B72a_CisternValueLinq());
            Check.Same(nameof(B72b_Linq), check.B72b_Linq(), check.B72b_CisternValueLinq());
            Check.Same(nameof(B72c_Linq), check.B72c_Linq() ? 1 : 0, check.B72c_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B73a_Linq), check.B73a_Linq() ? 1 : 0, check.B73a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B73b_Linq), check.B73b_Linq() ? 1 : 0, check.B73b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B74a_Linq), check.B74a_Linq(), check.B74a_CisternValueLinq());
            Check.Same(nameof(B74b_Linq), check.B74b_Linq(), check.B74b_CisternValueLinq());
            Check.Same(nameof(B74c_Linq), check.B74c_Linq(), check.B74c_CisternValueLinq());
            Check.Same(nameof(B75a_Linq), check.B75a_Linq(), check.B75a_CisternValueLinq());
            Check.Same(nameof(B75b_Linq), check.B75b_Linq(), check.B75b_CisternValueLinq());
            Check.Same(nameof(B76a_Linq), check.B76a_Linq() ? 1 : 0, check.B76a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B76b_Linq), check.B76b_Linq() ? 1 : 0, check.B76b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B77a_Linq), check.B77a_Linq(), check.B77a_CisternValueLinq());
            Check.Same(nameof(B77b_Linq), check.B77b_Linq(), check.B77b_CisternValueLinq());
            Check.Same(nameof(B77c_Linq), check.B77c_Linq(), check.B77c_CisternValueLinq());

            Check.Same(nameof(B72a_ZLinq), check.B72a_Linq(), check.B72a_ZLinq());
            Check.Same(nameof(B72b_ZLinq), check.B72b_Linq(), check.B72b_ZLinq());
            Check.Same(nameof(B72c_ZLinq), check.B72c_Linq() ? 1 : 0, check.B72c_ZLinq() ? 1 : 0);
            Check.Same(nameof(B73a_ZLinq), check.B73a_Linq() ? 1 : 0, check.B73a_ZLinq() ? 1 : 0);
            Check.Same(nameof(B73b_ZLinq), check.B73b_Linq() ? 1 : 0, check.B73b_ZLinq() ? 1 : 0);
            Check.Same(nameof(B74a_ZLinq), check.B74a_Linq(), check.B74a_ZLinq());
            Check.Same(nameof(B74b_ZLinq), check.B74b_Linq(), check.B74b_ZLinq());
            Check.Same(nameof(B74c_ZLinq), check.B74c_Linq(), check.B74c_ZLinq());
            Check.Same(nameof(B75a_ZLinq), check.B75a_Linq(), check.B75a_ZLinq());
            Check.Same(nameof(B75b_ZLinq), check.B75b_Linq(), check.B75b_ZLinq());
            Check.Same(nameof(B76a_ZLinq), check.B76a_Linq() ? 1 : 0, check.B76a_ZLinq() ? 1 : 0);
            Check.Same(nameof(B76b_ZLinq), check.B76b_Linq() ? 1 : 0, check.B76b_ZLinq() ? 1 : 0);
            Check.Same(nameof(B77a_ZLinq), check.B77a_Linq(), check.B77a_ZLinq());
            Check.Same(nameof(B77b_ZLinq), check.B77b_Linq(), check.B77b_ZLinq());
            Check.Same(nameof(B77c_ZLinq), check.B77c_Linq(), check.B77c_ZLinq());

            Check.Same(nameof(B72a_Handcoded), check.B72a_Linq(), check.B72a_Handcoded());
            Check.Same(nameof(B72b_Handcoded), check.B72b_Linq(), check.B72b_Handcoded());
            Check.Same(nameof(B72c_Handcoded), check.B72c_Linq() ? 1 : 0, check.B72c_Handcoded() ? 1 : 0);
            Check.Same(nameof(B73a_Handcoded), check.B73a_Linq() ? 1 : 0, check.B73a_Handcoded() ? 1 : 0);
            Check.Same(nameof(B73b_Handcoded), check.B73b_Linq() ? 1 : 0, check.B73b_Handcoded() ? 1 : 0);
            Check.Same(nameof(B74a_Handcoded), check.B74a_Linq(), check.B74a_Handcoded());
            Check.Same(nameof(B74b_Handcoded), check.B74b_Linq(), check.B74b_Handcoded());
            Check.Same(nameof(B74c_Handcoded), check.B74c_Linq(), check.B74c_Handcoded());
            Check.Same(nameof(B75a_Handcoded), check.B75a_Linq(), check.B75a_Handcoded());
            Check.Same(nameof(B75b_Handcoded), check.B75b_Linq(), check.B75b_Handcoded());
            Check.Same(nameof(B76a_Handcoded), check.B76a_Linq() ? 1 : 0, check.B76a_Handcoded() ? 1 : 0);
            Check.Same(nameof(B76b_Handcoded), check.B76b_Linq() ? 1 : 0, check.B76b_Handcoded() ? 1 : 0);
            Check.Same(nameof(B77a_Handcoded), check.B77a_Linq(), check.B77a_Handcoded());
            Check.Same(nameof(B77b_Handcoded), check.B77b_Linq(), check.B77b_Handcoded());
            Check.Same(nameof(B77c_Handcoded), check.B77c_Linq(), check.B77c_Handcoded());
        }
    }
}
