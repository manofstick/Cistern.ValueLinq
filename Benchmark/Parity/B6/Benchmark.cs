using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.6: Concat, where System.Linq copies, counts, indexes and reads the ends of the parts without
    // enumerating the whole of them. One method per entry (a letter per operation where an entry covers several) and
    // library, named by the entry's ID.
    [MemoryDiagnoser]
    public partial class B6
    {
        const int Length = 5_000;

        int[] _a;
        int[] _b;
        int[] _c;
        List<int> _listA;
        List<int> _listB;
        HashSet<int> _setA;
        HashSet<int> _setB;
        int _index;
        int _presentB;
        int _presentC;

        public B6()
        {
            // each part draws from a range of its own, so a value is present in one part only
            _a = Data.Values(Length);
            _b = Shifted(Data.Values(Length, Data.Seed + 1), Data.Range);
            _c = Shifted(Data.Values(Length, Data.Seed + 2), 2 * Data.Range);
            _listA = new List<int>(_a);
            _listB = new List<int>(_b);
            _setA = new HashSet<int>(_a);
            _setB = new HashSet<int>(_b);

            // in the second part
            _index = Length + Length / 2;
            _presentB = _b[Length / 2];
            _presentC = _c[Length / 2];
        }

        static int[] Shifted(int[] values, int by)
        {
            for (var i = 0; i < values.Length; ++i)
                values[i] += by;
            return values;
        }

        internal static void SanityCheck()
        {
            var check = new B6();

            Check.Same(nameof(B49a_Linq), check.B49a_Linq(), check.B49a_CisternValueLinq());
            Check.Same(nameof(B49b_Linq), check.B49b_Linq(), check.B49b_CisternValueLinq());
            Check.Same(nameof(B49c_Linq), check.B49c_Linq(), check.B49c_CisternValueLinq());
            Check.Same(nameof(B50a_Linq), check.B50a_Linq(), check.B50a_CisternValueLinq());
            Check.Same(nameof(B50b_Linq), check.B50b_Linq(), check.B50b_CisternValueLinq());
            Check.Same(nameof(B51a_Linq), check.B51a_Linq(), check.B51a_CisternValueLinq());
            Check.Same(nameof(B51b_Linq), check.B51b_Linq(), check.B51b_CisternValueLinq());
            Check.Same(nameof(B51c_Linq), check.B51c_Linq(), check.B51c_CisternValueLinq());
            Check.Same(nameof(B52a_Linq), check.B52a_Linq(), check.B52a_CisternValueLinq());
            Check.Same(nameof(B52b_Linq), check.B52b_Linq(), check.B52b_CisternValueLinq());
            Check.Same(nameof(B53a_Linq), check.B53a_Linq(), check.B53a_CisternValueLinq());
            Check.Same(nameof(B53b_Linq), check.B53b_Linq(), check.B53b_CisternValueLinq());
            Check.Same(nameof(B53c_Linq), check.B53c_Linq(), check.B53c_CisternValueLinq());
            Check.Same(nameof(B54a_Linq), check.B54a_Linq() ? 1 : 0, check.B54a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B54b_Linq), check.B54b_Linq() ? 1 : 0, check.B54b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B54c_Linq), check.B54c_Linq() ? 1 : 0, check.B54c_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B55a_Linq), check.B55a_Linq(), check.B55a_CisternValueLinq());
            Check.Same(nameof(B55b_Linq), check.B55b_Linq(), check.B55b_CisternValueLinq());

            Check.Same(nameof(B49a_ZLinq), check.B49a_Linq(), check.B49a_ZLinq());
            Check.Same(nameof(B49b_ZLinq), check.B49b_Linq(), check.B49b_ZLinq());
            Check.Same(nameof(B49c_ZLinq), check.B49c_Linq(), check.B49c_ZLinq());
            Check.Same(nameof(B50a_ZLinq), check.B50a_Linq(), check.B50a_ZLinq());
            Check.Same(nameof(B50b_ZLinq), check.B50b_Linq(), check.B50b_ZLinq());
            Check.Same(nameof(B51a_ZLinq), check.B51a_Linq(), check.B51a_ZLinq());
            Check.Same(nameof(B51b_ZLinq), check.B51b_Linq(), check.B51b_ZLinq());
            Check.Same(nameof(B51c_ZLinq), check.B51c_Linq(), check.B51c_ZLinq());
            Check.Same(nameof(B52a_ZLinq), check.B52a_Linq(), check.B52a_ZLinq());
            Check.Same(nameof(B52b_ZLinq), check.B52b_Linq(), check.B52b_ZLinq());
            Check.Same(nameof(B53a_ZLinq), check.B53a_Linq(), check.B53a_ZLinq());
            Check.Same(nameof(B53b_ZLinq), check.B53b_Linq(), check.B53b_ZLinq());
            Check.Same(nameof(B53c_ZLinq), check.B53c_Linq(), check.B53c_ZLinq());
            Check.Same(nameof(B54a_ZLinq), check.B54a_Linq() ? 1 : 0, check.B54a_ZLinq() ? 1 : 0);
            Check.Same(nameof(B54b_ZLinq), check.B54b_Linq() ? 1 : 0, check.B54b_ZLinq() ? 1 : 0);
            Check.Same(nameof(B54c_ZLinq), check.B54c_Linq() ? 1 : 0, check.B54c_ZLinq() ? 1 : 0);
            Check.Same(nameof(B55a_ZLinq), check.B55a_Linq(), check.B55a_ZLinq());
            Check.Same(nameof(B55b_ZLinq), check.B55b_Linq(), check.B55b_ZLinq());

            Check.Same(nameof(B49a_Handcoded), check.B49a_Linq(), check.B49a_Handcoded());
            Check.Same(nameof(B49b_Handcoded), check.B49b_Linq(), check.B49b_Handcoded());
            Check.Same(nameof(B49c_Handcoded), check.B49c_Linq(), check.B49c_Handcoded());
            Check.Same(nameof(B50a_Handcoded), check.B50a_Linq(), check.B50a_Handcoded());
            Check.Same(nameof(B50b_Handcoded), check.B50b_Linq(), check.B50b_Handcoded());
            Check.Same(nameof(B51a_Handcoded), check.B51a_Linq(), check.B51a_Handcoded());
            Check.Same(nameof(B51b_Handcoded), check.B51b_Linq(), check.B51b_Handcoded());
            Check.Same(nameof(B51c_Handcoded), check.B51c_Linq(), check.B51c_Handcoded());
            Check.Same(nameof(B52a_Handcoded), check.B52a_Linq(), check.B52a_Handcoded());
            Check.Same(nameof(B52b_Handcoded), check.B52b_Linq(), check.B52b_Handcoded());
            Check.Same(nameof(B53a_Handcoded), check.B53a_Linq(), check.B53a_Handcoded());
            Check.Same(nameof(B53b_Handcoded), check.B53b_Linq(), check.B53b_Handcoded());
            Check.Same(nameof(B53c_Handcoded), check.B53c_Linq(), check.B53c_Handcoded());
            Check.Same(nameof(B54a_Handcoded), check.B54a_Linq() ? 1 : 0, check.B54a_Handcoded() ? 1 : 0);
            Check.Same(nameof(B54b_Handcoded), check.B54b_Linq() ? 1 : 0, check.B54b_Handcoded() ? 1 : 0);
            Check.Same(nameof(B54c_Handcoded), check.B54c_Linq() ? 1 : 0, check.B54c_Handcoded() ? 1 : 0);
            Check.Same(nameof(B55a_Handcoded), check.B55a_Linq(), check.B55a_Handcoded());
            Check.Same(nameof(B55b_Handcoded), check.B55b_Linq(), check.B55b_Handcoded());

            // B52b and B53b read less of the iterator in System.Linq (the first part is enumerated up to the index in
            // B52b, not at all for Last in B53b); Check and Data have no read counter, so only the results are compared
        }
    }
}
