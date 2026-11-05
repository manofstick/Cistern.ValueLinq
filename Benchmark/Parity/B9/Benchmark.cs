using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // Audit section B.9: Shuffle, where System.Linq reads one element, indexes, counts and takes a prefix of a shuffled
    // source without shuffling the whole of it. One method per entry (a letter per operation where an entry covers
    // several) and library, named by the entry's ID. Results are random, so the sanity check asks of them what holds
    // whatever the order.
    [MemoryDiagnoser]
    public partial class B9
    {
        const int Length = 10_000;

        int[] _array;
        List<int> _list;
        int _present;
        int _absent;

        public B9()
        {
            _array = Data.Values(Length);
            _list = new List<int>(_array);

            // near the end of the data
            _present = _array[Length - 10];

            // outside the values' range, so it is not in the data
            _absent = Data.Range + 1;
        }

        internal static void SanityCheck()
        {
            var check = new B9();

            check.ElementOfData(nameof(B66a_Linq), check.B66a_Linq());
            check.ElementOfData(nameof(B66a_CisternValueLinq), check.B66a_CisternValueLinq());
            check.ElementOfData(nameof(B66b_Linq), check.B66b_Linq());
            check.ElementOfData(nameof(B66b_CisternValueLinq), check.B66b_CisternValueLinq());
            check.ElementOfData(nameof(B66c_Linq), check.B66c_Linq());
            check.ElementOfData(nameof(B66c_CisternValueLinq), check.B66c_CisternValueLinq());
            check.ElementOfData(nameof(B66d_Linq), check.B66d_Linq());
            check.ElementOfData(nameof(B66d_CisternValueLinq), check.B66d_CisternValueLinq());

            check.PartOfData(nameof(B67a_Linq), check.B67a_Linq(), 10);
            check.PartOfData(nameof(B67a_CisternValueLinq), check.B67a_CisternValueLinq(), 10);
            check.PartOfData(nameof(B67b_Linq), check.B67b_Linq(), 10);
            check.PartOfData(nameof(B67b_CisternValueLinq), check.B67b_CisternValueLinq(), 10);

            // B67c has nothing to compare but that it runs
            check.B67c_Linq();
            check.B67c_CisternValueLinq();

            Check.Same(nameof(B68a_Linq), check.B68a_Linq(), check.B68a_CisternValueLinq());
            Check.Same(nameof(B68b_Linq), check.B68b_Linq(), check.B68b_CisternValueLinq());
            Check.Same(nameof(B68c_Linq), check.B68c_Linq(), check.B68c_CisternValueLinq());
            Check.Same(nameof(B69a_Linq), check.B69a_Linq() ? 1 : 0, check.B69a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(B70a_Linq), check.B70a_Linq() ? 1 : 0, check.B70a_CisternValueLinq() ? 1 : 0);

            if (check.B69a_Linq() is false)
                throw new Exception($"{nameof(B69a_Linq)}: the present value was not found");
            if (check.B70a_Linq())
                throw new Exception($"{nameof(B70a_Linq)}: the absent value was found");

            check.ShuffleOfData(nameof(B71a_Linq), check.B71a_Linq());
            check.ShuffleOfData(nameof(B71a_CisternValueLinq), check.B71a_CisternValueLinq());
            check.ShuffleOfData(nameof(B71b_Linq), check.B71b_Linq());
            check.ShuffleOfData(nameof(B71b_CisternValueLinq), check.B71b_CisternValueLinq());

            check.ElementOfData(nameof(B66a_ZLinq), check.B66a_ZLinq());
            check.ElementOfData(nameof(B66b_ZLinq), check.B66b_ZLinq());
            check.ElementOfData(nameof(B66c_ZLinq), check.B66c_ZLinq());
            check.ElementOfData(nameof(B66d_ZLinq), check.B66d_ZLinq());
            check.PartOfData(nameof(B67a_ZLinq), check.B67a_ZLinq(), 10);
            check.PartOfData(nameof(B67b_ZLinq), check.B67b_ZLinq(), 10);
            check.B67c_ZLinq();
            Check.Same(nameof(B68a_ZLinq), check.B68a_Linq(), check.B68a_ZLinq());
            Check.Same(nameof(B68b_ZLinq), check.B68b_Linq(), check.B68b_ZLinq());
            Check.Same(nameof(B68c_ZLinq), check.B68c_Linq(), check.B68c_ZLinq());
            Check.Same(nameof(B69a_ZLinq), check.B69a_Linq() ? 1 : 0, check.B69a_ZLinq() ? 1 : 0);
            Check.Same(nameof(B70a_ZLinq), check.B70a_Linq() ? 1 : 0, check.B70a_ZLinq() ? 1 : 0);
            check.ShuffleOfData(nameof(B71a_ZLinq), check.B71a_ZLinq());
            check.ShuffleOfData(nameof(B71b_ZLinq), check.B71b_ZLinq());

            check.ElementOfData(nameof(B66a_Handcoded), check.B66a_Handcoded());
            check.ElementOfData(nameof(B66b_Handcoded), check.B66b_Handcoded());
            check.ElementOfData(nameof(B66c_Handcoded), check.B66c_Handcoded());
            check.ElementOfData(nameof(B66d_Handcoded), check.B66d_Handcoded());
            check.PartOfData(nameof(B67a_Handcoded), check.B67a_Handcoded(), 10);
            check.PartOfData(nameof(B67b_Handcoded), check.B67b_Handcoded(), 10);
            check.B67c_Handcoded();
            Check.Same(nameof(B68a_Handcoded), check.B68a_Linq(), check.B68a_Handcoded());
            Check.Same(nameof(B68b_Handcoded), check.B68b_Linq(), check.B68b_Handcoded());
            Check.Same(nameof(B68c_Handcoded), check.B68c_Linq(), check.B68c_Handcoded());
            Check.Same(nameof(B69a_Handcoded), check.B69a_Linq() ? 1 : 0, check.B69a_Handcoded() ? 1 : 0);
            Check.Same(nameof(B70a_Handcoded), check.B70a_Linq() ? 1 : 0, check.B70a_Handcoded() ? 1 : 0);
            check.ShuffleOfData(nameof(B71a_Handcoded), check.B71a_Handcoded());
            check.ShuffleOfData(nameof(B71b_Handcoded), check.B71b_Handcoded());
        }

        void ElementOfData(string entry, int element)
        {
            if (Array.IndexOf(_array, element) < 0)
                throw new Exception($"{entry}: {element} is not in the data");
        }

        // the given length, and no value more often than the data holds it
        void PartOfData(string entry, IEnumerable<int> part, int length)
        {
            var taken = part.ToList();
            if (taken.Count != length)
                throw new Exception($"{entry}: {taken.Count} elements, expected {length}");

            var available = _array.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            foreach (var x in taken)
            {
                if (!available.TryGetValue(x, out var remaining) || remaining == 0)
                    throw new Exception($"{entry}: {x} is taken more often than the data holds it");
                available[x] = remaining - 1;
            }
        }

        // the data, in some order
        void ShuffleOfData(string entry, IEnumerable<int> shuffled)
        {
            if (!shuffled.OrderBy(x => x).SequenceEqual(_array.OrderBy(x => x)))
                throw new Exception($"{entry}: sorted, the result is not the sorted data");
        }
    }
}
