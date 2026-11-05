using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section A: pipeline construction, operator-pair fusion and source elision. One method per entry and
    // library, named by the entry's ID.
    [MemoryDiagnoser]
    public partial class A
    {
        const int Length = 10_000;

        int[] _array;
        List<int> _list;
        IEnumerable<int> _iterator;
        object[] _objects;
        List<int>[] _parts;
        int[] _empty;

        Func<int, bool> _predicate;
        Func<int, bool> _predicate2;
        Func<int, int> _selector;
        Func<int, int> _selector2;
        Func<int, int> _key;
        Func<int, int> _key2;
        Func<string, int> _stringLength;

        public A()
        {
            _array = Data.Values(Length);
            _list = _array.ToList();
            _iterator = Data.Iterator(_array);
            _objects = _array.Select(x => x < Data.Range / 2 ? (object)x.ToString() : x).ToArray();
            _parts = Enumerable.Range(0, 8).Select(i => Data.Values(Length / 8, Data.Seed + i).ToList()).ToArray();
            _empty = [];

            _predicate = Data.Passing(Selectivity.Fifty);
            // the low bit of a random value, independent of _predicate's threshold
            _predicate2 = x => (x & 1) == 0;
            _selector = x => x * 3;
            _selector2 = x => x + 1;
            // ties of about a hundred elements on the first key, broken by the second
            _key = x => x / 10;
            _key2 = x => x % 10;
            _stringLength = s => s.Length;
        }

        internal static void SanityCheck()
        {
            var check = new A();

            Check.Same(nameof(A01_Linq), check.A01_Linq(), check.A01_CisternValueLinq());
            Check.Same(nameof(A02_Linq), check.A02_Linq(), check.A02_CisternValueLinq());
            Check.Same(nameof(A03_Linq), check.A03_Linq(), check.A03_CisternValueLinq());
            Check.Same(nameof(A04_Linq), check.A04_Linq(), check.A04_CisternValueLinq());
            Check.Same(nameof(A05_Linq), check.A05_Linq(), check.A05_CisternValueLinq());
            Check.Same(nameof(A06_Linq), check.A06_Linq(), check.A06_CisternValueLinq());
            Check.Same(nameof(A07_Linq), check.A07_Linq(), check.A07_CisternValueLinq());
            Check.Same(nameof(A08_Linq), check.A08_Linq(), check.A08_CisternValueLinq());
            Check.Same(nameof(A09_Linq), check.A09_Linq(), check.A09_CisternValueLinq());
            Check.Same(nameof(A10_Linq), check.A10_Linq(), check.A10_CisternValueLinq());
            Check.Same(nameof(A11_Linq), check.A11_Linq(), check.A11_CisternValueLinq());
            Check.Same(nameof(A12_Linq), check.A12_Linq(), check.A12_CisternValueLinq());
            Check.Same(nameof(A13_Linq), check.A13_Linq(), check.A13_CisternValueLinq());
            Check.Same(nameof(A14_Linq), check.A14_Linq(), check.A14_CisternValueLinq());
            Check.Same(nameof(A15_Linq), check.A15_Linq(), check.A15_CisternValueLinq());
            Check.Same(nameof(A16_Linq), check.A16_Linq(), check.A16_CisternValueLinq());
            Check.Same(nameof(A17_Linq), check.A17_Linq(), check.A17_CisternValueLinq());
            // a shuffle's elements differ run to run, so only its length is compared
            Check.Same(nameof(A18_Linq), check.A18_Linq().Length, check.A18_CisternValueLinq().Length);
            Check.Same(nameof(A19_Linq), check.A19_Linq(), check.A19_CisternValueLinq());
            Check.Same(nameof(A20_Linq), check.A20_Linq(), check.A20_CisternValueLinq());
            Check.Same(nameof(A21_Linq), check.A21_Linq(), check.A21_CisternValueLinq());
            Check.Same(nameof(A22_Linq), check.A22_Linq(), check.A22_CisternValueLinq());
            Check.Same(nameof(A23_Linq), check.A23_Linq(), check.A23_CisternValueLinq());
            Check.Same(nameof(A24_Linq), check.A24_Linq(), check.A24_CisternValueLinq());
            Check.Same(nameof(A25_Linq), check.A25_Linq(), check.A25_CisternValueLinq());

            Check.Same(nameof(A01_ZLinq), check.A01_Linq(), check.A01_ZLinq());
            Check.Same(nameof(A02_ZLinq), check.A02_Linq(), check.A02_ZLinq());
            Check.Same(nameof(A03_ZLinq), check.A03_Linq(), check.A03_ZLinq());
            Check.Same(nameof(A04_ZLinq), check.A04_Linq(), check.A04_ZLinq());
            Check.Same(nameof(A05_ZLinq), check.A05_Linq(), check.A05_ZLinq());
            Check.Same(nameof(A06_ZLinq), check.A06_Linq(), check.A06_ZLinq());
            Check.Same(nameof(A07_ZLinq), check.A07_Linq(), check.A07_ZLinq());
            Check.Same(nameof(A08_ZLinq), check.A08_Linq(), check.A08_ZLinq());
            Check.Same(nameof(A09_ZLinq), check.A09_Linq(), check.A09_ZLinq());
            Check.Same(nameof(A10_ZLinq), check.A10_Linq(), check.A10_ZLinq());
            Check.Same(nameof(A11_ZLinq), check.A11_Linq(), check.A11_ZLinq());
            Check.Same(nameof(A12_ZLinq), check.A12_Linq(), check.A12_ZLinq());
            Check.Same(nameof(A13_ZLinq), check.A13_Linq(), check.A13_ZLinq());
            Check.Same(nameof(A14_ZLinq), check.A14_Linq(), check.A14_ZLinq());
            Check.Same(nameof(A15_ZLinq), check.A15_Linq(), check.A15_ZLinq());
            Check.Same(nameof(A16_ZLinq), check.A16_Linq(), check.A16_ZLinq());
            Check.Same(nameof(A17_ZLinq), check.A17_Linq(), check.A17_ZLinq());
            Check.Same(nameof(A18_ZLinq), check.A18_Linq().Length, check.A18_ZLinq().Length);
            Check.Same(nameof(A19_ZLinq), check.A19_Linq(), check.A19_ZLinq());
            Check.Same(nameof(A20_ZLinq), check.A20_Linq(), check.A20_ZLinq());
            Check.Same(nameof(A21_ZLinq), check.A21_Linq(), check.A21_ZLinq());
            Check.Same(nameof(A22_ZLinq), check.A22_Linq(), check.A22_ZLinq());
            Check.Same(nameof(A23_ZLinq), check.A23_Linq(), check.A23_ZLinq());
            Check.Same(nameof(A24_ZLinq), check.A24_Linq(), check.A24_ZLinq());
            Check.Same(nameof(A25_ZLinq), check.A25_Linq(), check.A25_ZLinq());

            Check.Same(nameof(A01_Handcoded), check.A01_Linq(), check.A01_Handcoded());
            Check.Same(nameof(A02_Handcoded), check.A02_Linq(), check.A02_Handcoded());
            Check.Same(nameof(A03_Handcoded), check.A03_Linq(), check.A03_Handcoded());
            Check.Same(nameof(A04_Handcoded), check.A04_Linq(), check.A04_Handcoded());
            Check.Same(nameof(A05_Handcoded), check.A05_Linq(), check.A05_Handcoded());
            Check.Same(nameof(A06_Handcoded), check.A06_Linq(), check.A06_Handcoded());
            Check.Same(nameof(A07_Handcoded), check.A07_Linq(), check.A07_Handcoded());
            Check.Same(nameof(A08_Handcoded), check.A08_Linq(), check.A08_Handcoded());
            Check.Same(nameof(A09_Handcoded), check.A09_Linq(), check.A09_Handcoded());
            Check.Same(nameof(A10_Handcoded), check.A10_Linq(), check.A10_Handcoded());
            Check.Same(nameof(A11_Handcoded), check.A11_Linq(), check.A11_Handcoded());
            Check.Same(nameof(A12_Handcoded), check.A12_Linq(), check.A12_Handcoded());
            Check.Same(nameof(A13_Handcoded), check.A13_Linq(), check.A13_Handcoded());
            Check.Same(nameof(A14_Handcoded), check.A14_Linq(), check.A14_Handcoded());
            Check.Same(nameof(A15_Handcoded), check.A15_Linq(), check.A15_Handcoded());
            Check.Same(nameof(A16_Handcoded), check.A16_Linq(), check.A16_Handcoded());
            Check.Same(nameof(A17_Handcoded), check.A17_Linq(), check.A17_Handcoded());
            Check.Same(nameof(A18_Handcoded), check.A18_Linq().Length, check.A18_Handcoded().Length);
            Check.Same(nameof(A19_Handcoded), check.A19_Linq(), check.A19_Handcoded());
            Check.Same(nameof(A20_Handcoded), check.A20_Linq(), check.A20_Handcoded());
            Check.Same(nameof(A21_Handcoded), check.A21_Linq(), check.A21_Handcoded());
            Check.Same(nameof(A22_Handcoded), check.A22_Linq(), check.A22_Handcoded());
            Check.Same(nameof(A23_Handcoded), check.A23_Linq(), check.A23_Handcoded());
            Check.Same(nameof(A24_Handcoded), check.A24_Linq(), check.A24_Handcoded());
            Check.Same(nameof(A25_Handcoded), check.A25_Linq(), check.A25_Handcoded());

            // the entries whose saving is a selector not run: Cistern should call it no more often than System.Linq
            Action<Func<int, int>> useSelector = selector => check._selector = selector;
            Check.NoMoreCalls(nameof(A06_Linq), check._selector, useSelector, () => check.A06_Linq(), () => check.A06_CisternValueLinq());
            Check.NoMoreCalls(nameof(A08_Linq), check._selector, useSelector, () => check.A08_Linq(), () => check.A08_CisternValueLinq());
            Check.NoMoreCalls(nameof(A09_Linq), check._selector, useSelector, () => check.A09_Linq(), () => check.A09_CisternValueLinq());
            Check.NoMoreCalls(nameof(A15_Linq), check._selector, useSelector, () => check.A15_Linq(), () => check.A15_CisternValueLinq());
        }
    }
}
