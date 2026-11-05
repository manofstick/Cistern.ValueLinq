using BenchmarkDotNet.Attributes;
using Cistern.Benchmarks.Parity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    // The LINQ benchmarks of the .NET 11 performance post (its Collections and LINQ section), at the post's sizes and
    // data: Skip's last element over an Append of a known count, a short Sum, a Max over bytes, and FullJoin beside the
    // GroupJoin, SelectMany and Concat composition it replaces. One method per entry (a letter per length or form where
    // an entry covers several) and library, named by the entry's ID.
    [MemoryDiagnoser]
    public partial class Net11
    {
        const int JoinLength = 10_000;

        int[] _source;
        int _appended;
        int[] _intData;
        byte[] _bytes16;
        byte[] _bytes64;
        List<(int Id, string Name)> _left;
        List<(int Id, decimal Amount)> _right;

        public Net11()
        {
            _source = [1, 2, 3, 4, 5];
            _appended = 6;

            Random rng = new(42);
            _intData = Enumerable.Range(0, 32).Select(_ => rng.Next(-1_000, 1_000)).ToArray();

            _bytes16 = Enumerable.Range(0, 16).Select(i => (byte)i).ToArray();
            _bytes64 = Enumerable.Range(0, 64).Select(i => (byte)i).ToArray();

            // a quarter of each side unmatched
            _left = Enumerable.Range(0, JoinLength).Select(i => (i, $"item{i}")).ToList();
            _right = Enumerable.Range(JoinLength / 4, JoinLength).Select(i => (i, (decimal)i * 1.5m)).ToList();
        }

        internal static void SanityCheck()
        {
            var check = new Net11();

            Check.Same(nameof(N01_Linq), check.N01_Linq(), check.N01_CisternValueLinq());
            Check.Same(nameof(N02_Linq), check.N02_Linq(), check.N02_CisternValueLinq());
            Check.Same(nameof(N02_CisternValueLinqFastest), check.N02_Linq(), check.N02_CisternValueLinqFastest());
            Check.Same(nameof(N03a_Linq), check.N03a_Linq(), check.N03a_CisternValueLinq());
            Check.Same(nameof(N03b_Linq), check.N03b_Linq(), check.N03b_CisternValueLinq());
            Check.Same(nameof(N04a_Linq), check.N04a_Linq(), check.N04a_CisternValueLinq());
            Check.Same(nameof(N04b_Linq), check.N04b_Linq(), check.N04b_CisternValueLinq());

            Check.Same(nameof(N01_ZLinq), check.N01_Linq(), check.N01_ZLinq());
            Check.Same(nameof(N02_ZLinq), check.N02_Linq(), check.N02_ZLinq());
            Check.Same(nameof(N03a_ZLinq), check.N03a_Linq(), check.N03a_ZLinq());
            Check.Same(nameof(N03b_ZLinq), check.N03b_Linq(), check.N03b_ZLinq());

            Check.Same(nameof(N01_Handcoded), check.N01_Linq(), check.N01_Handcoded());
            Check.Same(nameof(N02_Handcoded), check.N02_Linq(), check.N02_Handcoded());
            Check.Same(nameof(N03a_Handcoded), check.N03a_Linq(), check.N03a_Handcoded());
            Check.Same(nameof(N03b_Handcoded), check.N03b_Linq(), check.N03b_Handcoded());
            Check.Same(nameof(N04a_Handcoded), check.N04a_Linq(), check.N04a_Handcoded());
            Check.Same(nameof(N04b_Linq), check.N04a_Linq(), check.N04b_Linq());
        }
    }
}
