using BenchmarkDotNet.Attributes;
using Cistern.Benchmarks.Parity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cistern.Benchmarks.Toub
{
    // The LINQ benchmarks of the .NET 9 performance post, at the post's sizes and data: operator pairs answered through
    // an IEnumerable<T> a library handed out, Any over Where and Select, operators over an empty source, DefaultIfEmpty
    // into Average, Chunk, the predicate aggregates over a List<T>, SequenceEqual, ToArray and ToList over iterators,
    // OrderBy over one element, ToDictionary, Max, OrderBy and Order into Last, sorted enumeration, and a lookup's
    // groupings. One method per entry (a letter per operator or length where an entry covers several) and library, named
    // by the entry's ID. Where an entry measures a chain a library handed out as IEnumerable<T>, each library builds its
    // own into that field; the empty-source entries return object, so a struct chain is boxed, as the post's are not.
    [MemoryDiagnoser]
    public partial class Net9
    {
        int[] _ints1000;
        List<int> _list1000;
        string[] _empty;
        IEnumerable<int> _list1000AsEnumerable;
        IEnumerable<int> _source10000;
        IEnumerable<int> _source10000List;
        Int128[] _int128s;
        int[] _ints10000;
        List<int> _shuffled;
        IEnumerable<DayOfWeek> _days;
        IEnumerable<int> _data1000;
        string[] _lookupSource;
        int[] _ints100;
        Dictionary<int, List<string>> _handLookup;

        public Net9()
        {
            _ints1000 = Enumerable.Range(0, 1000).ToArray();
            _list1000 = Enumerable.Range(0, 1000).ToList();
            _empty = [];
            _list1000AsEnumerable = Enumerable.Range(0, 1000).ToList();
            _source10000 = Enumerable.Range(0, 10_000).ToArray();
            _source10000List = ((int[])_source10000).ToList();
            _int128s = Enumerable.Range(0, 1000).Select(x => (Int128)x).ToArray();
            _ints10000 = Enumerable.Range(0, 10_000).ToArray();
            _shuffled = new(Enumerable.Range(-8000, 8000 * 2));
            new Random(42).Shuffle(CollectionsMarshal.AsSpan(_shuffled));
            _days = new Random(42).GetItems(Enum.GetValues<DayOfWeek>(), 100);
            _data1000 = Enumerable.Range(0, 1000).ToArray();
            _lookupSource =
                (from i in Enumerable.Range(0, 10)
                 from item in Enumerable.Range(0, 8)
                 select new string((char)('a' + item), i + 1)).ToArray();

            _ints100 = Enumerable.Range(0, 100).ToArray();
            _handLookup = new Dictionary<int, List<string>>();
            foreach (var s in _lookupSource)
            {
                if (!_handLookup.TryGetValue(s.Length, out var group))
                    _handLookup[s.Length] = group = new List<string>();
                group.Add(s);
            }

            BuildLinq();
            BuildCisternValueLinq();
            BuildZLinq();
        }

        partial void BuildLinq();
        partial void BuildCisternValueLinq();
        partial void BuildZLinq();

        private static IEnumerable<int> Iterations(int count)
        {
            for (int i = 0; i < count; i++)
                yield return i;
        }

        private static IEnumerable<string> StringItems(int count)
        {
            for (int i = 0; i < count; i++)
                yield return ".NET 9";
        }

        private static IEnumerable<int> IntItems(int count)
        {
            for (int i = 0; i < count; i++)
                yield return i;
        }

        // N11's pairs for the hand-coded loop, which no library builds
        private static IEnumerable<KeyValuePair<int, int>> Pairs(int count)
        {
            for (int i = 0; i < count; i++)
                yield return new KeyValuePair<int, int>(i, i);
        }

        private static int Elements(object sequence)
        {
            var count = 0;
            foreach (var _ in (IEnumerable)sequence)
                ++count;
            return count;
        }

        internal static void SanityCheck()
        {
            var check = new Net9();

            Check.Same(nameof(N01a_Linq), check.N01a_Linq(), check.N01a_CisternValueLinq());
            Check.Same(nameof(N01b_Linq), check.N01b_Linq(), check.N01b_CisternValueLinq());
            Check.Same(nameof(N01c_Linq), check.N01c_Linq(), check.N01c_CisternValueLinq());
            Check.Same(nameof(N01d_Linq), check.N01d_Linq(), check.N01d_CisternValueLinq());
            Check.Same(nameof(N01e_Linq), check.N01e_Linq(), check.N01e_CisternValueLinq());
            Check.Same(nameof(N01f_Linq), check.N01f_Linq(), check.N01f_CisternValueLinq());
            Check.Same(nameof(N01a_CisternValueLinqEnumerable), check.N01a_Linq(), check.N01a_CisternValueLinqEnumerable());
            Check.Same(nameof(N01b_CisternValueLinqEnumerable), check.N01b_Linq(), check.N01b_CisternValueLinqEnumerable());
            Check.Same(nameof(N01c_CisternValueLinqEnumerable), check.N01c_Linq(), check.N01c_CisternValueLinqEnumerable());
            Check.Same(nameof(N01d_CisternValueLinqEnumerable), check.N01d_Linq(), check.N01d_CisternValueLinqEnumerable());
            Check.Same(nameof(N01e_CisternValueLinqEnumerable), check.N01e_Linq(), check.N01e_CisternValueLinqEnumerable());
            Check.Same(nameof(N01f_CisternValueLinqEnumerable), check.N01f_Linq(), check.N01f_CisternValueLinqEnumerable());
            Check.Same(nameof(N01a_CisternValueLinqFused), check.N01a_Linq(), check.N01a_CisternValueLinqFused());
            Check.Same(nameof(N01b_CisternValueLinqFused), check.N01b_Linq(), check.N01b_CisternValueLinqFused());
            Check.Same(nameof(N01c_CisternValueLinqFused), check.N01c_Linq(), check.N01c_CisternValueLinqFused());
            Check.Same(nameof(N01d_CisternValueLinqFused), check.N01d_Linq(), check.N01d_CisternValueLinqFused());
            Check.Same(nameof(N01e_CisternValueLinqFused), check.N01e_Linq(), check.N01e_CisternValueLinqFused());
            Check.Same(nameof(N01f_CisternValueLinqFused), check.N01f_Linq(), check.N01f_CisternValueLinqFused());
            Check.Same(nameof(N02a_Linq), check.N02a_Linq() ? 1 : 0, check.N02a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N02b_Linq), check.N02b_Linq() ? 1 : 0, check.N02b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N02a_CisternValueLinqEnumerable), check.N02a_Linq() ? 1 : 0, check.N02a_CisternValueLinqEnumerable() ? 1 : 0);
            Check.Same(nameof(N02b_CisternValueLinqEnumerable), check.N02b_Linq() ? 1 : 0, check.N02b_CisternValueLinqEnumerable() ? 1 : 0);
            Check.Same(nameof(N02a_CisternValueLinqFused), check.N02a_Linq() ? 1 : 0, check.N02a_CisternValueLinqFused() ? 1 : 0);
            Check.Same(nameof(N02b_CisternValueLinqFused), check.N02b_Linq() ? 1 : 0, check.N02b_CisternValueLinqFused() ? 1 : 0);
            Check.Same(nameof(N03a_Linq), Elements(check.N03a_Linq()), Elements(check.N03a_CisternValueLinq()));
            Check.Same(nameof(N03b_Linq), Elements(check.N03b_Linq()), Elements(check.N03b_CisternValueLinq()));
            Check.Same(nameof(N03c_Linq), Elements(check.N03c_Linq()), Elements(check.N03c_CisternValueLinq()));
            Check.Same(nameof(N03d_Linq), Elements(check.N03d_Linq()), Elements(check.N03d_CisternValueLinq()));
            Check.Same(nameof(N03e_Linq), Elements(check.N03e_Linq()), Elements(check.N03e_CisternValueLinq()));
            Check.Same(nameof(N03f_Linq), Elements(check.N03f_Linq()), Elements(check.N03f_CisternValueLinq()));
            Check.Same(nameof(N03g_Linq), Elements(check.N03g_Linq()), Elements(check.N03g_CisternValueLinq()));
            Check.Same(nameof(N03h_Linq), Elements(check.N03h_Linq()), Elements(check.N03h_CisternValueLinq()));
            Check.Same(nameof(N03i_Linq), Elements(check.N03i_Linq()), Elements(check.N03i_CisternValueLinq()));
            Check.Same(nameof(N03j_Linq), Elements(check.N03j_Linq()), Elements(check.N03j_CisternValueLinq()));
            Check.Same(nameof(N03k_Linq), Elements(check.N03k_Linq()), Elements(check.N03k_CisternValueLinq()));
            Check.Same(nameof(N04_Linq), check.N04_Linq(), check.N04_CisternValueLinq());
            Check.Same(nameof(N05_Linq), check.N05_Linq(), check.N05_CisternValueLinq());
            Check.Same(nameof(N06a_Linq), check.N06a_Linq() ? 1 : 0, check.N06a_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N06b_Linq), check.N06b_Linq() ? 1 : 0, check.N06b_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N06c_Linq), check.N06c_Linq(), check.N06c_CisternValueLinq());
            Check.Same(nameof(N06d_Linq), check.N06d_Linq(), check.N06d_CisternValueLinq());
            Check.Same(nameof(N06e_Linq), check.N06e_Linq(), check.N06e_CisternValueLinq());
            Check.Same(nameof(N07_Linq), check.N07_Linq() ? 1 : 0, check.N07_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N08a_Linq), check.N08a_Linq(), check.N08a_CisternValueLinq());
            Check.Same(nameof(N08b_Linq), check.N08b_Linq(), check.N08b_CisternValueLinq());
            Check.Same(nameof(N08c_Linq), check.N08c_Linq(), check.N08c_CisternValueLinq());
            Check.Same(nameof(N09a_Linq), check.N09a_Linq(), check.N09a_CisternValueLinq());
            Check.Same(nameof(N09b_Linq), check.N09b_Linq(), check.N09b_CisternValueLinq());
            Check.Same(nameof(N10_Linq), check.N10_Linq(), check.N10_CisternValueLinq());
            Check.Same(nameof(N11_Linq), check.N11_Linq().Count, check.N11_CisternValueLinq().Count);
            Check.Same(nameof(N11_CisternValueLinqInline), check.N11_Linq().Count, check.N11_CisternValueLinqInline().Count);
            Check.Same(nameof(N12_Linq), (long)check.N12_Linq(), (long)check.N12_CisternValueLinq());
            Check.Same(nameof(N13_Linq), check.N13_Linq(), check.N13_CisternValueLinq());
            Check.Same(nameof(N14a_Linq), check.N14a_Linq(), check.N14a_CisternValueLinq());
            Check.Same(nameof(N14b_Linq), check.N14b_Linq(), check.N14b_CisternValueLinq());
            Check.Same(nameof(N15_Linq), check.N15_Linq(), check.N15_CisternValueLinq());
            Check.Same(nameof(N16_Linq), check.N16_Linq(), check.N16_CisternValueLinq());
            Check.Same(nameof(N17_Linq), check.N17_Linq(), check.N17_CisternValueLinq());

            Check.Same(nameof(N01a_CisternValueLinqTyped), check.N01a_Linq(), check.N01a_CisternValueLinqTyped());
            Check.Same(nameof(N01b_CisternValueLinqTyped), check.N01b_Linq(), check.N01b_CisternValueLinqTyped());
            Check.Same(nameof(N01c_CisternValueLinqTyped), check.N01c_Linq(), check.N01c_CisternValueLinqTyped());
            Check.Same(nameof(N01d_CisternValueLinqTyped), check.N01d_Linq(), check.N01d_CisternValueLinqTyped());
            Check.Same(nameof(N01e_CisternValueLinqTyped), check.N01e_Linq(), check.N01e_CisternValueLinqTyped());
            Check.Same(nameof(N01f_CisternValueLinqTyped), check.N01f_Linq(), check.N01f_CisternValueLinqTyped());
            Check.Same(nameof(N02b_CisternValueLinqTyped), check.N02b_Linq() ? 1 : 0, check.N02b_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N06a_CisternValueLinqTyped), check.N06a_Linq() ? 1 : 0, check.N06a_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N06b_CisternValueLinqTyped), check.N06b_Linq() ? 1 : 0, check.N06b_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N06c_CisternValueLinqTyped), check.N06c_Linq(), check.N06c_CisternValueLinqTyped());
            Check.Same(nameof(N06d_CisternValueLinqTyped), check.N06d_Linq(), check.N06d_CisternValueLinqTyped());
            Check.Same(nameof(N06e_CisternValueLinqTyped), check.N06e_Linq(), check.N06e_CisternValueLinqTyped());
            Check.Same(nameof(N07_CisternValueLinqTyped), check.N07_Linq() ? 1 : 0, check.N07_CisternValueLinqTyped() ? 1 : 0);
            Check.Same(nameof(N15_CisternValueLinqTyped), check.N15_Linq(), check.N15_CisternValueLinqTyped());
            Check.Same(nameof(N16_CisternValueLinqTyped), check.N16_Linq(), check.N16_CisternValueLinqTyped());

            Check.Same(nameof(N04_ZLinq), check.N04_Linq(), check.N04_ZLinq());
            Check.Same(nameof(N05_ZLinq), check.N05_Linq(), check.N05_ZLinq());
            Check.Same(nameof(N06a_ZLinq), check.N06a_Linq() ? 1 : 0, check.N06a_ZLinq() ? 1 : 0);
            Check.Same(nameof(N06b_ZLinq), check.N06b_Linq() ? 1 : 0, check.N06b_ZLinq() ? 1 : 0);
            Check.Same(nameof(N06c_ZLinq), check.N06c_Linq(), check.N06c_ZLinq());
            Check.Same(nameof(N06d_ZLinq), check.N06d_Linq(), check.N06d_ZLinq());
            Check.Same(nameof(N06e_ZLinq), check.N06e_Linq(), check.N06e_ZLinq());
            Check.Same(nameof(N07_ZLinq), check.N07_Linq() ? 1 : 0, check.N07_ZLinq() ? 1 : 0);
            Check.Same(nameof(N08a_ZLinq), check.N08a_Linq(), check.N08a_ZLinq());
            Check.Same(nameof(N08b_ZLinq), check.N08b_Linq(), check.N08b_ZLinq());
            Check.Same(nameof(N08c_ZLinq), check.N08c_Linq(), check.N08c_ZLinq());
            Check.Same(nameof(N09a_ZLinq), check.N09a_Linq(), check.N09a_ZLinq());
            Check.Same(nameof(N09b_ZLinq), check.N09b_Linq(), check.N09b_ZLinq());
            Check.Same(nameof(N10_ZLinq), check.N10_Linq(), check.N10_ZLinq());
            Check.Same(nameof(N11_ZLinq), check.N11_Linq().Count, check.N11_ZLinq().Count);
            Check.Same(nameof(N12_ZLinq), (long)check.N12_Linq(), (long)check.N12_ZLinq());
            Check.Same(nameof(N13_ZLinq), check.N13_Linq(), check.N13_ZLinq());
            Check.Same(nameof(N14a_ZLinq), check.N14a_Linq(), check.N14a_ZLinq());
            Check.Same(nameof(N14b_ZLinq), check.N14b_Linq(), check.N14b_ZLinq());
            Check.Same(nameof(N15_ZLinq), check.N15_Linq(), check.N15_ZLinq());
            Check.Same(nameof(N16_ZLinq), check.N16_Linq(), check.N16_ZLinq());
            Check.Same(nameof(N17_ZLinq), check.N17_Linq(), check.N17_ZLinq());

            Check.Same(nameof(N01a_Handcoded), check.N01a_Linq(), check.N01a_Handcoded());
            Check.Same(nameof(N01b_Handcoded), check.N01b_Linq(), check.N01b_Handcoded());
            Check.Same(nameof(N01c_Handcoded), check.N01c_Linq(), check.N01c_Handcoded());
            Check.Same(nameof(N01d_Handcoded), check.N01d_Linq(), check.N01d_Handcoded());
            Check.Same(nameof(N01e_Handcoded), check.N01e_Linq(), check.N01e_Handcoded());
            Check.Same(nameof(N01f_Handcoded), check.N01f_Linq(), check.N01f_Handcoded());
            Check.Same(nameof(N02a_Handcoded), check.N02a_Linq() ? 1 : 0, check.N02a_Handcoded() ? 1 : 0);
            Check.Same(nameof(N02b_Handcoded), check.N02b_Linq() ? 1 : 0, check.N02b_Handcoded() ? 1 : 0);
            Check.Same(nameof(N04_Handcoded), check.N04_Linq(), check.N04_Handcoded());
            Check.Same(nameof(N05_Handcoded), check.N05_Linq(), check.N05_Handcoded());
            Check.Same(nameof(N06a_Handcoded), check.N06a_Linq() ? 1 : 0, check.N06a_Handcoded() ? 1 : 0);
            Check.Same(nameof(N06b_Handcoded), check.N06b_Linq() ? 1 : 0, check.N06b_Handcoded() ? 1 : 0);
            Check.Same(nameof(N06c_Handcoded), check.N06c_Linq(), check.N06c_Handcoded());
            Check.Same(nameof(N06d_Handcoded), check.N06d_Linq(), check.N06d_Handcoded());
            Check.Same(nameof(N06e_Handcoded), check.N06e_Linq(), check.N06e_Handcoded());
            Check.Same(nameof(N07_Handcoded), check.N07_Linq() ? 1 : 0, check.N07_Handcoded() ? 1 : 0);
            Check.Same(nameof(N08a_Handcoded), check.N08a_Linq(), check.N08a_Handcoded());
            Check.Same(nameof(N08b_Handcoded), check.N08b_Linq(), check.N08b_Handcoded());
            Check.Same(nameof(N08c_Handcoded), check.N08c_Linq(), check.N08c_Handcoded());
            Check.Same(nameof(N09a_Handcoded), check.N09a_Linq(), check.N09a_Handcoded());
            Check.Same(nameof(N09b_Handcoded), check.N09b_Linq(), check.N09b_Handcoded());
            Check.Same(nameof(N10_Handcoded), check.N10_Linq(), check.N10_Handcoded());
            Check.Same(nameof(N11_Handcoded), check.N11_Linq().Count, check.N11_Handcoded().Count);
            Check.Same(nameof(N12_Handcoded), (long)check.N12_Linq(), (long)check.N12_Handcoded());
            Check.Same(nameof(N13_Handcoded), check.N13_Linq(), check.N13_Handcoded());
            Check.Same(nameof(N14a_Handcoded), check.N14a_Linq(), check.N14a_Handcoded());
            Check.Same(nameof(N14b_Handcoded), check.N14b_Linq(), check.N14b_Handcoded());
            Check.Same(nameof(N15_Handcoded), check.N15_Linq(), check.N15_Handcoded());
            Check.Same(nameof(N16_Handcoded), check.N16_Linq(), check.N16_Handcoded());
            Check.Same(nameof(N17_Handcoded), check.N17_Linq(), check.N17_Handcoded());
        }
    }
}
