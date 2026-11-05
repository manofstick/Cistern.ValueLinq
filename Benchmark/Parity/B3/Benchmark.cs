using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.3: Skip and Take windows answering a downstream request with less work. One method per entry and
    // library, named by the entry's ID.
    [MemoryDiagnoser]
    public partial class B3
    {
        const int Length = 10_000;

        int[] _array;
        List<int> _list;
        IEnumerable<int> _iterator;
        HashSet<int> _hashSet;

        Func<int, bool> _predicate;

        public B3()
        {
            _array = Data.Values(Length);
            _list = _array.ToList();
            _iterator = Data.Iterator(_array);
            // a collection that is not a list, of Length distinct values
            _hashSet = Enumerable.Range(0, Length).ToHashSet();

            _predicate = Data.Passing(Selectivity.Fifty);
        }

        internal static void SanityCheck()
        {
            var check = new B3();

            Check.Same(nameof(B15_Linq), check.B15_Linq(), check.B15_CisternValueLinq());
            Check.Same(nameof(B16_Linq), check.B16_Linq(), check.B16_CisternValueLinq());
            Check.Same(nameof(B17_Linq), check.B17_Linq(), check.B17_CisternValueLinq());
            Check.Same(nameof(B18_Linq), check.B18_Linq() ? 1 : 0, check.B18_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B19_Linq), check.B19_Linq(), check.B19_CisternValueLinq());
            Check.Same(nameof(B20_Linq), check.B20_Linq(), check.B20_CisternValueLinq());
            Check.Same(nameof(B21_Linq), check.B21_Linq(), check.B21_CisternValueLinq());
            Check.Same(nameof(B22_Linq), check.B22_Linq(), check.B22_CisternValueLinq());
            Check.Same(nameof(B23_Linq), check.B23_Linq(), check.B23_CisternValueLinq());
            Check.Same(nameof(B24_Linq), check.B24_Linq(), check.B24_CisternValueLinq());
            Check.Same(nameof(B25_Linq), check.B25_Linq(), check.B25_CisternValueLinq());

            Check.Same(nameof(B15_ZLinq), check.B15_Linq(), check.B15_ZLinq());
            Check.Same(nameof(B16_ZLinq), check.B16_Linq(), check.B16_ZLinq());
            Check.Same(nameof(B17_ZLinq), check.B17_Linq(), check.B17_ZLinq());
            Check.Same(nameof(B18_ZLinq), check.B18_Linq() ? 1 : 0, check.B18_ZLinq() ? 1 : 0);
            Check.Same(nameof(B19_ZLinq), check.B19_Linq(), check.B19_ZLinq());
            Check.Same(nameof(B20_ZLinq), check.B20_Linq(), check.B20_ZLinq());
            Check.Same(nameof(B21_ZLinq), check.B21_Linq(), check.B21_ZLinq());
            Check.Same(nameof(B22_ZLinq), check.B22_Linq(), check.B22_ZLinq());
            Check.Same(nameof(B23_ZLinq), check.B23_Linq(), check.B23_ZLinq());
            Check.Same(nameof(B24_ZLinq), check.B24_Linq(), check.B24_ZLinq());
            Check.Same(nameof(B25_ZLinq), check.B25_Linq(), check.B25_ZLinq());

            Check.Same(nameof(B15_Handcoded), check.B15_Linq(), check.B15_Handcoded());
            Check.Same(nameof(B16_Handcoded), check.B16_Linq(), check.B16_Handcoded());
            Check.Same(nameof(B17_Handcoded), check.B17_Linq(), check.B17_Handcoded());
            Check.Same(nameof(B18_Handcoded), check.B18_Linq() ? 1 : 0, check.B18_Handcoded() ? 1 : 0);
            Check.Same(nameof(B19_Handcoded), check.B19_Linq(), check.B19_Handcoded());
            Check.Same(nameof(B20_Handcoded), check.B20_Linq(), check.B20_Handcoded());
            Check.Same(nameof(B21_Handcoded), check.B21_Linq(), check.B21_Handcoded());
            Check.Same(nameof(B22_Handcoded), check.B22_Linq(), check.B22_Handcoded());
            Check.Same(nameof(B23_Handcoded), check.B23_Linq(), check.B23_Handcoded());
            Check.Same(nameof(B24_Handcoded), check.B24_Linq(), check.B24_Handcoded());
            Check.Same(nameof(B25_Handcoded), check.B25_Linq(), check.B25_Handcoded());

            // B22 saves predicate calls: Cistern should make no more than System.Linq
            Action<Func<int, bool>> usePredicate = predicate => check._predicate = predicate;
            Check.NoMoreCalls(nameof(B22_Linq), check._predicate, usePredicate, () => check.B22_Linq(), () => check.B22_CisternValueLinq());
        }
    }
}
