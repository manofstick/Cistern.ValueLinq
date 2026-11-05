using BenchmarkDotNet.Attributes;
using Cistern.Benchmarks.Parity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    // The LINQ benchmarks of the .NET 8 performance post, at the post's sizes and data: ToList over Select, Repeat and
    // Range, AddRange of a Repeat, Max over bytes, Sum, sorted enumeration through Order and OrderBy, and Any over a
    // string, a list and an array. One method per entry (a letter per form where an entry covers several) and library,
    // named by the entry's ID. Every Range, Repeat and Reverse is the library's own; a chain held as IEnumerable<T> is
    // built by each library into its own field, except ZLinq's, a ref struct with no IEnumerable<T> form, built in the
    // method. Max's bytes are seeded rather than the post's Random.Shared, so the sanity check sees the same data.
    [MemoryDiagnoser]
    public partial class Net8
    {
        IEnumerable<int> _source1024;
        List<byte> _bytesList;
        byte[] _bytes4096;
        int[] _reversed1000000;
        string _str;
        List<int> _list3;
        int[] _array3;

        public Net8()
        {
            _source1024 = Enumerable.Range(0, 1024).ToArray();
            _bytesList = new();
            var random = new Random(42);
            _bytes4096 = new byte[4096];
            for (var i = 0; i < _bytes4096.Length; ++i)
                _bytes4096[i] = (byte)random.Next(0, 256);
            _reversed1000000 = Enumerable.Range(0, 1_000_000).Reverse().ToArray();
            _str = "hello";
            _list3 = [1, 2, 3];
            _array3 = [4, 5, 6];

            BuildLinq();
            BuildCisternValueLinq();
        }

        partial void BuildLinq();
        partial void BuildCisternValueLinq();

        // N06's source for the hand-coded sort, which no library builds
        private static IEnumerable<int> Descending(int count)
        {
            for (var i = count - 1; i >= 0; --i)
                yield return i;
        }

        internal static void SanityCheck()
        {
            var check = new Net8();

            Check.Same(nameof(N01a_Linq), check.N01a_Linq(), check.N01a_CisternValueLinq());
            Check.Same(nameof(N01b_Linq), check.N01b_Linq(), check.N01b_CisternValueLinq());
            Check.Same(nameof(N01c_Linq), check.N01c_Linq(), check.N01c_CisternValueLinq());
            Check.Same(nameof(N02_Linq), check.N02_Linq(), check.N02_CisternValueLinq());
            Check.Same(nameof(N03_Linq), check.N03_Linq().ToArray(), check.N03_CisternValueLinq());
            Check.Same(nameof(N03_CisternValueLinqCollection), check.N03_Linq().ToArray(), check.N03_CisternValueLinqCollection());
            Check.Same(nameof(N04_Linq), check.N04_Linq(), check.N04_CisternValueLinq());
            Check.Same(nameof(N05_Linq), check.N05_Linq(), check.N05_CisternValueLinq());
            Check.Same(nameof(N06_Linq), check.N06_Linq(), check.N06_CisternValueLinq());
            Check.Same(nameof(N06_CisternValueLinqInline), check.N06_Linq(), check.N06_CisternValueLinqInline());
            Check.Same(nameof(N07_Linq), check.N07_Linq(), check.N07_CisternValueLinq());
            Check.Same(nameof(N08_Linq), check.N08_Linq() ? 1 : 0, check.N08_CisternValueLinq() ? 1 : 0);

            Check.Same(nameof(N01a_CisternValueLinqTyped), check.N01a_Linq(), check.N01a_CisternValueLinqTyped());
            Check.Same(nameof(N05_CisternValueLinqTyped), check.N05_Linq(), check.N05_CisternValueLinqTyped());

            Check.Same(nameof(N01a_ZLinq), check.N01a_Linq(), check.N01a_ZLinq());
            Check.Same(nameof(N01b_ZLinq), check.N01b_Linq(), check.N01b_ZLinq());
            Check.Same(nameof(N01c_ZLinq), check.N01c_Linq(), check.N01c_ZLinq());
            Check.Same(nameof(N02_ZLinq), check.N02_Linq(), check.N02_ZLinq());
            Check.Same(nameof(N03_ZLinq), check.N03_Linq().ToArray(), check.N03_ZLinq());
            Check.Same(nameof(N04_ZLinq), check.N04_Linq(), check.N04_ZLinq());
            Check.Same(nameof(N05_ZLinq), check.N05_Linq(), check.N05_ZLinq());
            Check.Same(nameof(N06_ZLinq), check.N06_Linq(), check.N06_ZLinq());
            Check.Same(nameof(N07_ZLinq), check.N07_Linq(), check.N07_ZLinq());
            Check.Same(nameof(N08_ZLinq), check.N08_Linq() ? 1 : 0, check.N08_ZLinq() ? 1 : 0);

            Check.Same(nameof(N01a_Handcoded), check.N01a_Linq(), check.N01a_Handcoded());
            Check.Same(nameof(N01b_Handcoded), check.N01b_Linq(), check.N01b_Handcoded());
            Check.Same(nameof(N01c_Handcoded), check.N01c_Linq(), check.N01c_Handcoded());
            Check.Same(nameof(N02_Handcoded), check.N02_Linq(), check.N02_Handcoded());
            Check.Same(nameof(N03_Handcoded), check.N03_Linq().ToArray(), check.N03_Handcoded());
            Check.Same(nameof(N04_Handcoded), check.N04_Linq(), check.N04_Handcoded());
            Check.Same(nameof(N05_Handcoded), check.N05_Linq(), check.N05_Handcoded());
            Check.Same(nameof(N06_Handcoded), check.N06_Linq(), check.N06_Handcoded());
            Check.Same(nameof(N07_Handcoded), check.N07_Linq(), check.N07_Handcoded());
            Check.Same(nameof(N08_Handcoded), check.N08_Linq() ? 1 : 0, check.N08_Handcoded() ? 1 : 0);
        }
    }
}
