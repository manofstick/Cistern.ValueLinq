using BenchmarkDotNet.Attributes;
using Cistern.Benchmarks.Parity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    // The LINQ benchmarks of the .NET 5 performance post, at the post's sizes and data: sorted enumeration of a seeded
    // random int[1000] through OrderBy, and SkipLast(5).Sum() over a List<int> of 100 held as IEnumerable<int>. One method
    // per entry and library, named by the entry's ID. The post's sorting benchmark discards the elements; here they are
    // summed, so the sanity check has a result to compare.
    [MemoryDiagnoser]
    public partial class Net5
    {
        int[] _array;
        IEnumerable<int> _data;

        public Net5()
        {
            var r = new Random(42);
            _array = Enumerable.Range(0, 1_000).Select(_ => r.Next()).ToArray();
            _data = Enumerable.Range(0, 100).ToList();

            BuildCisternValueLinq();
        }

        partial void BuildCisternValueLinq();

        internal static void SanityCheck()
        {
            var check = new Net5();

            Check.Same(nameof(N01_Linq), check.N01_Linq(), check.N01_CisternValueLinq());
            Check.Same(nameof(N02_Linq), check.N02_Linq(), check.N02_CisternValueLinq());
            Check.Same(nameof(N02_CisternValueLinqTyped), check.N02_Linq(), check.N02_CisternValueLinqTyped());

            Check.Same(nameof(N01_ZLinq), check.N01_Linq(), check.N01_ZLinq());
            Check.Same(nameof(N02_ZLinq), check.N02_Linq(), check.N02_ZLinq());

            Check.Same(nameof(N01_Handcoded), check.N01_Linq(), check.N01_Handcoded());
            Check.Same(nameof(N02_Handcoded), check.N02_Linq(), check.N02_Handcoded());
        }
    }
}
