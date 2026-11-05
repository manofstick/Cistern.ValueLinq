using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.1: Select answering a downstream request with less work. One method per entry and library, named
    // by the entry's ID; B2 documents a deliberate non-optimization and has none.
    [MemoryDiagnoser]
    public partial class B1
    {
        const int Length = 10_000;

        int[] _array;
        List<int> _list;
        IEnumerable<int> _iterator;

        Func<int, int> _selector;

        public B1()
        {
            _array = Data.Values(Length);
            _list = _array.ToList();
            _iterator = Data.Iterator(_array);

            _selector = x => x * 3;
        }

        internal static void SanityCheck()
        {
            var check = new B1();

            Check.Same(nameof(B01_Linq), check.B01_Linq(), check.B01_CisternValueLinq());
            Check.Same(nameof(B03_Linq), check.B03_Linq(), check.B03_CisternValueLinq());
            Check.Same(nameof(B04_Linq), check.B04_Linq(), check.B04_CisternValueLinq());
            Check.Same(nameof(B05_Linq), check.B05_Linq(), check.B05_CisternValueLinq());
            Check.Same(nameof(B06_Linq), check.B06_Linq(), check.B06_CisternValueLinq());
            Check.Same(nameof(B07_Linq), check.B07_Linq() ? 1 : 0, check.B07_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B08_Linq), check.B08_Linq(), check.B08_CisternValueLinq());
            Check.Same(nameof(B09_Linq), check.B09_Linq(), check.B09_CisternValueLinq());

            Check.Same(nameof(B01_ZLinq), check.B01_Linq(), check.B01_ZLinq());
            Check.Same(nameof(B03_ZLinq), check.B03_Linq(), check.B03_ZLinq());
            Check.Same(nameof(B04_ZLinq), check.B04_Linq(), check.B04_ZLinq());
            Check.Same(nameof(B05_ZLinq), check.B05_Linq(), check.B05_ZLinq());
            Check.Same(nameof(B06_ZLinq), check.B06_Linq(), check.B06_ZLinq());
            Check.Same(nameof(B07_ZLinq), check.B07_Linq() ? 1 : 0, check.B07_ZLinq() ? 1 : 0);
            Check.Same(nameof(B08_ZLinq), check.B08_Linq(), check.B08_ZLinq());
            Check.Same(nameof(B09_ZLinq), check.B09_Linq(), check.B09_ZLinq());

            Check.Same(nameof(B01_Handcoded), check.B01_Linq(), check.B01_Handcoded());
            Check.Same(nameof(B03_Handcoded), check.B03_Linq(), check.B03_Handcoded());
            Check.Same(nameof(B04_Handcoded), check.B04_Linq(), check.B04_Handcoded());
            Check.Same(nameof(B05_Handcoded), check.B05_Linq(), check.B05_Handcoded());
            Check.Same(nameof(B06_Handcoded), check.B06_Linq(), check.B06_Handcoded());
            Check.Same(nameof(B07_Handcoded), check.B07_Linq() ? 1 : 0, check.B07_Handcoded() ? 1 : 0);
            Check.Same(nameof(B08_Handcoded), check.B08_Linq(), check.B08_Handcoded());
            Check.Same(nameof(B09_Handcoded), check.B09_Linq(), check.B09_Handcoded());

            // every entry but B9 saves selector calls: Cistern should call it no more often than System.Linq
            Action<Func<int, int>> useSelector = selector => check._selector = selector;
            Check.NoMoreCalls(nameof(B01_Linq), check._selector, useSelector, () => check.B01_Linq(), () => check.B01_CisternValueLinq());
            Check.NoMoreCalls(nameof(B03_Linq), check._selector, useSelector, () => check.B03_Linq(), () => check.B03_CisternValueLinq());
            Check.NoMoreCalls(nameof(B04_Linq), check._selector, useSelector, () => check.B04_Linq(), () => check.B04_CisternValueLinq());
            Check.NoMoreCalls(nameof(B05_Linq), check._selector, useSelector, () => check.B05_Linq(), () => check.B05_CisternValueLinq());
            Check.NoMoreCalls(nameof(B06_Linq), check._selector, useSelector, () => check.B06_Linq(), () => check.B06_CisternValueLinq());
            Check.NoMoreCalls(nameof(B07_Linq), check._selector, useSelector, () => check.B07_Linq(), () => check.B07_CisternValueLinq());
            Check.NoMoreCalls(nameof(B08_Linq), check._selector, useSelector, () => check.B08_Linq(), () => check.B08_CisternValueLinq());
        }
    }
}
