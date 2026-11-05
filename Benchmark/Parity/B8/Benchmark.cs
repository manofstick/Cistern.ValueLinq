using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.8: Reverse, where System.Linq reads the ends of a list or ordering, indexes, counts and copies
    // a reversed source without buffering it. One method per entry (a letter per operation where an entry covers
    // several) and library, named by the entry's ID. List<T>.Reverse() is an instance method returning void that wins
    // over the extension, so the list sources are reversed through the static call of each library's Enumerable.
    [MemoryDiagnoser]
    public partial class B8
    {
        const int Length = 10_000;

        int[] _array;
        List<int> _list;
        HashSet<int> _set;
        int _present;

        Func<int, int> _key;

        public B8()
        {
            _array = Data.Values(Length);
            _list = new List<int>(_array);
            _set = new HashSet<int>(_array);

            // near the end of the data
            _present = _array[Length - 10];

            _key = x => x ^ 0x155;
        }

        internal static void SanityCheck()
        {
            var check = new B8();

            Check.Same(nameof(B61a_Linq), check.B61a_Linq(), check.B61a_CisternValueLinq());
            Check.Same(nameof(B61b_Linq), check.B61b_Linq(), check.B61b_CisternValueLinq());
            Check.Same(nameof(B61c_Linq), check.B61c_Linq(), check.B61c_CisternValueLinq());
            Check.Same(nameof(B62a_Linq), check.B62a_Linq(), check.B62a_CisternValueLinq());
            Check.Same(nameof(B62b_Linq), check.B62b_Linq(), check.B62b_CisternValueLinq());
            Check.Same(nameof(B62c_Linq), check.B62c_Linq(), check.B62c_CisternValueLinq());
            Check.Same(nameof(B63a_Linq), check.B63a_Linq(), check.B63a_CisternValueLinq());
            Check.Same(nameof(B63b_Linq), check.B63b_Linq(), check.B63b_CisternValueLinq());
            Check.Same(nameof(B64a_Linq), check.B64a_Linq(), check.B64a_CisternValueLinq());
            Check.Same(nameof(B64b_Linq), check.B64b_Linq(), check.B64b_CisternValueLinq());
            Check.Same(nameof(B64c_Linq), check.B64c_Linq(), check.B64c_CisternValueLinq());
            Check.Same(nameof(B64d_Linq), check.B64d_Linq() ? 1 : 0, check.B64d_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B64e_Linq), check.B64e_Linq() ? 1 : 0, check.B64e_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B65a_Linq), check.B65a_Linq(), check.B65a_CisternValueLinq());
            Check.Same(nameof(B65b_Linq), check.B65b_Linq(), check.B65b_CisternValueLinq());
            Check.Same(nameof(B65c_Linq), check.B65c_Linq(), check.B65c_CisternValueLinq());

            Check.Same(nameof(B61a_ZLinq), check.B61a_Linq(), check.B61a_ZLinq());
            Check.Same(nameof(B61b_ZLinq), check.B61b_Linq(), check.B61b_ZLinq());
            Check.Same(nameof(B61c_ZLinq), check.B61c_Linq(), check.B61c_ZLinq());
            Check.Same(nameof(B62a_ZLinq), check.B62a_Linq(), check.B62a_ZLinq());
            Check.Same(nameof(B62b_ZLinq), check.B62b_Linq(), check.B62b_ZLinq());
            Check.Same(nameof(B62c_ZLinq), check.B62c_Linq(), check.B62c_ZLinq());
            Check.Same(nameof(B63a_ZLinq), check.B63a_Linq(), check.B63a_ZLinq());
            Check.Same(nameof(B63b_ZLinq), check.B63b_Linq(), check.B63b_ZLinq());
            Check.Same(nameof(B64a_ZLinq), check.B64a_Linq(), check.B64a_ZLinq());
            Check.Same(nameof(B64b_ZLinq), check.B64b_Linq(), check.B64b_ZLinq());
            Check.Same(nameof(B64c_ZLinq), check.B64c_Linq(), check.B64c_ZLinq());
            Check.Same(nameof(B64d_ZLinq), check.B64d_Linq() ? 1 : 0, check.B64d_ZLinq() ? 1 : 0);
            Check.Same(nameof(B64e_ZLinq), check.B64e_Linq() ? 1 : 0, check.B64e_ZLinq() ? 1 : 0);
            Check.Same(nameof(B65a_ZLinq), check.B65a_Linq(), check.B65a_ZLinq());
            Check.Same(nameof(B65b_ZLinq), check.B65b_Linq(), check.B65b_ZLinq());
            Check.Same(nameof(B65c_ZLinq), check.B65c_Linq(), check.B65c_ZLinq());

            Check.Same(nameof(B61a_Handcoded), check.B61a_Linq(), check.B61a_Handcoded());
            Check.Same(nameof(B61b_Handcoded), check.B61b_Linq(), check.B61b_Handcoded());
            Check.Same(nameof(B61c_Handcoded), check.B61c_Linq(), check.B61c_Handcoded());
            Check.Same(nameof(B62a_Handcoded), check.B62a_Linq(), check.B62a_Handcoded());
            Check.Same(nameof(B62b_Handcoded), check.B62b_Linq(), check.B62b_Handcoded());
            Check.Same(nameof(B62c_Handcoded), check.B62c_Linq(), check.B62c_Handcoded());
            Check.Same(nameof(B63a_Handcoded), check.B63a_Linq(), check.B63a_Handcoded());
            Check.Same(nameof(B63b_Handcoded), check.B63b_Linq(), check.B63b_Handcoded());
            Check.Same(nameof(B64a_Handcoded), check.B64a_Linq(), check.B64a_Handcoded());
            Check.Same(nameof(B64b_Handcoded), check.B64b_Linq(), check.B64b_Handcoded());
            Check.Same(nameof(B64c_Handcoded), check.B64c_Linq(), check.B64c_Handcoded());
            Check.Same(nameof(B64d_Handcoded), check.B64d_Linq() ? 1 : 0, check.B64d_Handcoded() ? 1 : 0);
            Check.Same(nameof(B64e_Handcoded), check.B64e_Linq() ? 1 : 0, check.B64e_Handcoded() ? 1 : 0);
            Check.Same(nameof(B65a_Handcoded), check.B65a_Linq(), check.B65a_Handcoded());
            Check.Same(nameof(B65b_Handcoded), check.B65b_Linq(), check.B65b_Handcoded());
            Check.Same(nameof(B65c_Handcoded), check.B65c_Linq(), check.B65c_Handcoded());

            // B61a and B62a read the ordering's last element by a linear scan, calling the key once per element
            Action<Func<int, int>> useKey = key => check._key = key;
            Check.NoMoreCalls(nameof(B61a_Linq), check._key, useKey, () => check.B61a_Linq(), () => check.B61a_CisternValueLinq());
            Check.NoMoreCalls(nameof(B62a_Linq), check._key, useKey, () => check.B62a_Linq(), () => check.B62a_CisternValueLinq());
        }
    }
}
