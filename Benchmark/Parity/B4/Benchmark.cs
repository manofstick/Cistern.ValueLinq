using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.4: Range and Repeat answering Count, Contains, ElementAt, First, Last and the copies by arithmetic
    // or a fill, and Range under a Select saving selector calls. One method per entry (a letter per operation where an
    // entry covers several) and library, named by the entry's ID.
    [MemoryDiagnoser]
    public partial class B4
    {
        const int Length = 10_000;

        int _start;
        int _element;
        int _absent;
        string _nullString;

        Func<int, int> _selector;

        public B4()
        {
            _start = 5;
            _element = 7;
            _absent = -1;
            _nullString = null;

            _selector = x => x * 3;
        }

        internal static void SanityCheck()
        {
            var check = new B4();

            Check.Same(nameof(B26a_Linq), check.B26a_Linq(), check.B26a_CisternValueLinq());
            Check.Same(nameof(B26b_Linq), check.B26b_Linq() ? 1 : 0, check.B26b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B26c_Linq), check.B26c_Linq(), check.B26c_CisternValueLinq());
            Check.Same(nameof(B26d_Linq), check.B26d_Linq(), check.B26d_CisternValueLinq());
            Check.Same(nameof(B26e_Linq), check.B26e_Linq(), check.B26e_CisternValueLinq());
            Check.Same(nameof(B27a_Linq), check.B27a_Linq(), check.B27a_CisternValueLinq());
            Check.Same(nameof(B27b_Linq), check.B27b_Linq(), check.B27b_CisternValueLinq());
            Check.Same(nameof(B28_Linq), check.B28_Linq(), check.B28_CisternValueLinq());
            Check.Same(nameof(B29a_Linq), check.B29a_Linq(), check.B29a_CisternValueLinq());
            Check.Same(nameof(B29b_Linq), check.B29b_Linq() ? 1 : 0, check.B29b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B29c_Linq), check.B29c_Linq(), check.B29c_CisternValueLinq());
            Check.Same(nameof(B29d_Linq), check.B29d_Linq(), check.B29d_CisternValueLinq());
            Check.Same(nameof(B29e_Linq), check.B29e_Linq(), check.B29e_CisternValueLinq());
            Check.Same(nameof(B30a_Linq), check.B30a_Linq(), check.B30a_CisternValueLinq());
            Check.Same(nameof(B30b_Linq), check.B30b_Linq(), check.B30b_CisternValueLinq());
            Check.Same(nameof(B30c_Linq), check.B30c_Linq(), check.B30c_CisternValueLinq());
            Check.Same(nameof(B30d_Linq), check.B30d_Linq(), check.B30d_CisternValueLinq());
            Check.Same(nameof(B31_Linq), check.B31_Linq(), check.B31_CisternValueLinq());
            Check.Same(nameof(B32a_Linq), check.B32a_Linq(), check.B32a_CisternValueLinq());
            Check.Same(nameof(B32b_Linq), check.B32b_Linq(), check.B32b_CisternValueLinq());
            Check.Same(nameof(B32c_Linq), check.B32c_Linq(), check.B32c_CisternValueLinq());
            Check.Same(nameof(B32d_Linq), check.B32d_Linq(), check.B32d_CisternValueLinq());

            Check.Same(nameof(B26a_ZLinq), check.B26a_Linq(), check.B26a_ZLinq());
            Check.Same(nameof(B26b_ZLinq), check.B26b_Linq() ? 1 : 0, check.B26b_ZLinq() ? 1 : 0);
            Check.Same(nameof(B26c_ZLinq), check.B26c_Linq(), check.B26c_ZLinq());
            Check.Same(nameof(B26d_ZLinq), check.B26d_Linq(), check.B26d_ZLinq());
            Check.Same(nameof(B26e_ZLinq), check.B26e_Linq(), check.B26e_ZLinq());
            Check.Same(nameof(B27a_ZLinq), check.B27a_Linq(), check.B27a_ZLinq());
            Check.Same(nameof(B27b_ZLinq), check.B27b_Linq(), check.B27b_ZLinq());
            Check.Same(nameof(B29a_ZLinq), check.B29a_Linq(), check.B29a_ZLinq());
            Check.Same(nameof(B29b_ZLinq), check.B29b_Linq() ? 1 : 0, check.B29b_ZLinq() ? 1 : 0);
            Check.Same(nameof(B29c_ZLinq), check.B29c_Linq(), check.B29c_ZLinq());
            Check.Same(nameof(B29d_ZLinq), check.B29d_Linq(), check.B29d_ZLinq());
            Check.Same(nameof(B29e_ZLinq), check.B29e_Linq(), check.B29e_ZLinq());
            Check.Same(nameof(B30a_ZLinq), check.B30a_Linq(), check.B30a_ZLinq());
            Check.Same(nameof(B30b_ZLinq), check.B30b_Linq(), check.B30b_ZLinq());
            Check.Same(nameof(B30c_ZLinq), check.B30c_Linq(), check.B30c_ZLinq());
            Check.Same(nameof(B30d_ZLinq), check.B30d_Linq(), check.B30d_ZLinq());
            Check.Same(nameof(B32a_ZLinq), check.B32a_Linq(), check.B32a_ZLinq());
            Check.Same(nameof(B32b_ZLinq), check.B32b_Linq(), check.B32b_ZLinq());
            Check.Same(nameof(B32c_ZLinq), check.B32c_Linq(), check.B32c_ZLinq());
            Check.Same(nameof(B32d_ZLinq), check.B32d_Linq(), check.B32d_ZLinq());

            Check.Same(nameof(B26a_Handcoded), check.B26a_Linq(), check.B26a_Handcoded());
            Check.Same(nameof(B26b_Handcoded), check.B26b_Linq() ? 1 : 0, check.B26b_Handcoded() ? 1 : 0);
            Check.Same(nameof(B26c_Handcoded), check.B26c_Linq(), check.B26c_Handcoded());
            Check.Same(nameof(B26d_Handcoded), check.B26d_Linq(), check.B26d_Handcoded());
            Check.Same(nameof(B26e_Handcoded), check.B26e_Linq(), check.B26e_Handcoded());
            Check.Same(nameof(B27a_Handcoded), check.B27a_Linq(), check.B27a_Handcoded());
            Check.Same(nameof(B27b_Handcoded), check.B27b_Linq(), check.B27b_Handcoded());
            Check.Same(nameof(B28_Handcoded), check.B28_Linq(), check.B28_Handcoded());
            Check.Same(nameof(B29a_Handcoded), check.B29a_Linq(), check.B29a_Handcoded());
            Check.Same(nameof(B29b_Handcoded), check.B29b_Linq() ? 1 : 0, check.B29b_Handcoded() ? 1 : 0);
            Check.Same(nameof(B29c_Handcoded), check.B29c_Linq(), check.B29c_Handcoded());
            Check.Same(nameof(B29d_Handcoded), check.B29d_Linq(), check.B29d_Handcoded());
            Check.Same(nameof(B29e_Handcoded), check.B29e_Linq(), check.B29e_Handcoded());
            Check.Same(nameof(B30a_Handcoded), check.B30a_Linq(), check.B30a_Handcoded());
            Check.Same(nameof(B30b_Handcoded), check.B30b_Linq(), check.B30b_Handcoded());
            Check.Same(nameof(B30c_Handcoded), check.B30c_Linq(), check.B30c_Handcoded());
            Check.Same(nameof(B30d_Handcoded), check.B30d_Linq(), check.B30d_Handcoded());
            Check.Same(nameof(B31_Handcoded), check.B31_Linq(), check.B31_Handcoded());
            Check.Same(nameof(B32a_Handcoded), check.B32a_Linq(), check.B32a_Handcoded());
            Check.Same(nameof(B32b_Handcoded), check.B32b_Linq(), check.B32b_Handcoded());
            Check.Same(nameof(B32c_Handcoded), check.B32c_Linq(), check.B32c_Handcoded());
            Check.Same(nameof(B32d_Handcoded), check.B32d_Linq(), check.B32d_Handcoded());

            // B32 saves selector calls: Cistern should call it no more often than System.Linq
            Action<Func<int, int>> useSelector = selector => check._selector = selector;
            Check.NoMoreCalls(nameof(B32a_Linq), check._selector, useSelector, () => check.B32a_Linq(), () => check.B32a_CisternValueLinq());
            Check.NoMoreCalls(nameof(B32b_Linq), check._selector, useSelector, () => check.B32b_Linq(), () => check.B32b_CisternValueLinq());
            Check.NoMoreCalls(nameof(B32c_Linq), check._selector, useSelector, () => check.B32c_Linq(), () => check.B32c_CisternValueLinq());
            Check.NoMoreCalls(nameof(B32d_Linq), check._selector, useSelector, () => check.B32d_Linq(), () => check.B32d_CisternValueLinq());
        }
    }
}
