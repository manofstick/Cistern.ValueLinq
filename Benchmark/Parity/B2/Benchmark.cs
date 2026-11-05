using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.2: Where and Where-Select answering a downstream request with less work. One method per entry and
    // library, named by the entry's ID, each at three selectivities.
    [MemoryDiagnoser]
    public partial class B2
    {
        const int Length = 10_000;

        int[] _array = [];
        IEnumerable<int> _iterator = [];

        Func<int, bool> _predicate = _ => true;
        Func<int, int> _selector = x => x;

        [Params(Selectivity.Ten, Selectivity.Fifty, Selectivity.Ninety)]
        public Selectivity Selectivity { get; set; } = Selectivity.Fifty;

        [GlobalSetup]
        public void SetupData()
        {
            _array = Data.Values(Length);
            _iterator = Data.Iterator(_array);

            _predicate = Data.Passing(Selectivity);
            _selector = x => x * 3;
        }

        internal static void SanityCheck()
        {
            var check = new B2();
            check.SetupData();

            Check.Same(nameof(B10_Linq), check.B10_Linq(), check.B10_CisternValueLinq());
            Check.Same(nameof(B11_Linq), check.B11_Linq(), check.B11_CisternValueLinq());
            Check.Same(nameof(B12_Linq), check.B12_Linq(), check.B12_CisternValueLinq());
            Check.Same(nameof(B13_Linq), check.B13_Linq(), check.B13_CisternValueLinq());
            Check.Same(nameof(B14_Linq), check.B14_Linq(), check.B14_CisternValueLinq());

            Check.Same(nameof(B10_ZLinq), check.B10_Linq(), check.B10_ZLinq());
            Check.Same(nameof(B11_ZLinq), check.B11_Linq(), check.B11_ZLinq());
            Check.Same(nameof(B12_ZLinq), check.B12_Linq(), check.B12_ZLinq());
            Check.Same(nameof(B13_ZLinq), check.B13_Linq(), check.B13_ZLinq());
            Check.Same(nameof(B14_ZLinq), check.B14_Linq(), check.B14_ZLinq());

            Check.Same(nameof(B10_Handcoded), check.B10_Linq(), check.B10_Handcoded());
            Check.Same(nameof(B11_Handcoded), check.B11_Linq(), check.B11_Handcoded());
            Check.Same(nameof(B12_Handcoded), check.B12_Linq(), check.B12_Handcoded());
            Check.Same(nameof(B13_Handcoded), check.B13_Linq(), check.B13_Handcoded());
            Check.Same(nameof(B14_Handcoded), check.B14_Linq(), check.B14_Handcoded());

            // B10 saves predicate calls, B11 and B12 selector calls: Cistern should make no more than System.Linq
            Action<Func<int, bool>> usePredicate = predicate => check._predicate = predicate;
            Action<Func<int, int>> useSelector = selector => check._selector = selector;
            Check.NoMoreCalls(nameof(B10_Linq), check._predicate, usePredicate, () => check.B10_Linq(), () => check.B10_CisternValueLinq());
            Check.NoMoreCalls(nameof(B11_Linq), check._selector, useSelector, () => check.B11_Linq(), () => check.B11_CisternValueLinq());
            Check.NoMoreCalls(nameof(B12_Linq), check._selector, useSelector, () => check.B12_Linq(), () => check.B12_CisternValueLinq());
        }
    }
}
