using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.7: Append and Prepend, where System.Linq counts, copies, indexes and reads the ends of the source
    // without enumerating the whole of it. One method per entry (a letter per operation where an entry covers several)
    // and library, named by the entry's ID.
    [MemoryDiagnoser]
    public partial class B7
    {
        const int Length = 10_000;

        int[] _array;
        List<int> _list;
        HashSet<int> _set;
        int _x;
        int _y;
        int _z;
        int _present;

        public B7()
        {
            _array = Data.Values(Length);
            _list = new List<int>(_array);
            _set = new HashSet<int>(_array);

            // outside the values' range, so they are not present in the source
            _x = Data.Range + 1;
            _y = Data.Range + 2;
            _z = Data.Range + 3;

            // near the end of the list
            _present = _array[Length - 10];
        }

        internal static void SanityCheck()
        {
            var check = new B7();

            Check.Same(nameof(B56a_Linq), check.B56a_Linq(), check.B56a_CisternValueLinq());
            Check.Same(nameof(B56b_Linq), check.B56b_Linq(), check.B56b_CisternValueLinq());
            Check.Same(nameof(B56c_Linq), check.B56c_Linq(), check.B56c_CisternValueLinq());
            Check.Same(nameof(B57a_Linq), check.B57a_Linq(), check.B57a_CisternValueLinq());
            Check.Same(nameof(B57b_Linq), check.B57b_Linq(), check.B57b_CisternValueLinq());
            Check.Same(nameof(B57c_Linq), check.B57c_Linq(), check.B57c_CisternValueLinq());
            Check.Same(nameof(B57d_Linq), check.B57d_Linq(), check.B57d_CisternValueLinq());
            Check.Same(nameof(B58a_Linq), check.B58a_Linq(), check.B58a_CisternValueLinq());
            Check.Same(nameof(B58b_Linq), check.B58b_Linq(), check.B58b_CisternValueLinq());
            Check.Same(nameof(B59a_Linq), check.B59a_Linq(), check.B59a_CisternValueLinq());
            Check.Same(nameof(B59b_Linq), check.B59b_Linq(), check.B59b_CisternValueLinq());
            Check.Same(nameof(B59c_Linq), check.B59c_Linq(), check.B59c_CisternValueLinq());
            Check.Same(nameof(B60a_Linq), check.B60a_Linq() ? 1 : 0, check.B60a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B60b_Linq), check.B60b_Linq() ? 1 : 0, check.B60b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B60c_Linq), check.B60c_Linq() ? 1 : 0, check.B60c_CisternValueLinq() ? 1 : 0);

            Check.Same(nameof(B56a_ZLinq), check.B56a_Linq(), check.B56a_ZLinq());
            Check.Same(nameof(B56b_ZLinq), check.B56b_Linq(), check.B56b_ZLinq());
            Check.Same(nameof(B56c_ZLinq), check.B56c_Linq(), check.B56c_ZLinq());
            Check.Same(nameof(B57a_ZLinq), check.B57a_Linq(), check.B57a_ZLinq());
            Check.Same(nameof(B57b_ZLinq), check.B57b_Linq(), check.B57b_ZLinq());
            Check.Same(nameof(B57c_ZLinq), check.B57c_Linq(), check.B57c_ZLinq());
            Check.Same(nameof(B57d_ZLinq), check.B57d_Linq(), check.B57d_ZLinq());
            Check.Same(nameof(B58a_ZLinq), check.B58a_Linq(), check.B58a_ZLinq());
            Check.Same(nameof(B58b_ZLinq), check.B58b_Linq(), check.B58b_ZLinq());
            Check.Same(nameof(B59a_ZLinq), check.B59a_Linq(), check.B59a_ZLinq());
            Check.Same(nameof(B59b_ZLinq), check.B59b_Linq(), check.B59b_ZLinq());
            Check.Same(nameof(B59c_ZLinq), check.B59c_Linq(), check.B59c_ZLinq());
            Check.Same(nameof(B60a_ZLinq), check.B60a_Linq() ? 1 : 0, check.B60a_ZLinq() ? 1 : 0);
            Check.Same(nameof(B60b_ZLinq), check.B60b_Linq() ? 1 : 0, check.B60b_ZLinq() ? 1 : 0);
            Check.Same(nameof(B60c_ZLinq), check.B60c_Linq() ? 1 : 0, check.B60c_ZLinq() ? 1 : 0);

            Check.Same(nameof(B56a_Handcoded), check.B56a_Linq(), check.B56a_Handcoded());
            Check.Same(nameof(B56b_Handcoded), check.B56b_Linq(), check.B56b_Handcoded());
            Check.Same(nameof(B56c_Handcoded), check.B56c_Linq(), check.B56c_Handcoded());
            Check.Same(nameof(B57a_Handcoded), check.B57a_Linq(), check.B57a_Handcoded());
            Check.Same(nameof(B57b_Handcoded), check.B57b_Linq(), check.B57b_Handcoded());
            Check.Same(nameof(B57c_Handcoded), check.B57c_Linq(), check.B57c_Handcoded());
            Check.Same(nameof(B57d_Handcoded), check.B57d_Linq(), check.B57d_Handcoded());
            Check.Same(nameof(B58a_Handcoded), check.B58a_Linq(), check.B58a_Handcoded());
            Check.Same(nameof(B58b_Handcoded), check.B58b_Linq(), check.B58b_Handcoded());
            Check.Same(nameof(B59a_Handcoded), check.B59a_Linq(), check.B59a_Handcoded());
            Check.Same(nameof(B59b_Handcoded), check.B59b_Linq(), check.B59b_Handcoded());
            Check.Same(nameof(B59c_Handcoded), check.B59c_Linq(), check.B59c_Handcoded());
            Check.Same(nameof(B60a_Handcoded), check.B60a_Linq() ? 1 : 0, check.B60a_Handcoded() ? 1 : 0);
            Check.Same(nameof(B60b_Handcoded), check.B60b_Linq() ? 1 : 0, check.B60b_Handcoded() ? 1 : 0);
            Check.Same(nameof(B60c_Handcoded), check.B60c_Linq() ? 1 : 0, check.B60c_Handcoded() ? 1 : 0);

            // B58a and B58b read none of the iterator in System.Linq; Check and Data have no read counter, so only the
            // results are compared
        }
    }
}
