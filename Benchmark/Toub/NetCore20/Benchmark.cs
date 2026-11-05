using BenchmarkDotNet.Attributes;
using Cistern.Benchmarks.Parity;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    // The LINQ examples of the .NET Core 2.0 performance post, at the post's sizes: enumerating Range(0, 10) concatenated
    // 10,000 times through IEnumerable<int>, OrderBy(i => i).Skip(4).First() over Range(0, 10_000_000).Reverse(),
    // Select(i => i).ToList() over an int[10_000_000], and ToArray() of Range(0, 100_000_000). One method per entry and
    // library, named by the entry's ID. Every Range and Reverse is the library's own; a chain held as IEnumerable<T> is
    // built by each library into its own field, except ZLinq's, a ref struct with no IEnumerable<T> form, built in the
    // method, where Cistern has a copy of the same form (CisternValueLinqInline). ZLinq cannot build N01's chain, whose
    // type would change with each Concat, so it has no N01. The post's enumerating example discards the elements; here
    // they are summed, so the sanity check has a result to compare.
    [MemoryDiagnoser]
    public partial class NetCore20
    {
        IEnumerable<int> _zeroToTenMillion;

        public NetCore20()
        {
            _zeroToTenMillion = Enumerable.Range(0, 10_000_000).ToArray();

            BuildLinq();
            BuildCisternValueLinq();
            BuildHandcoded();
        }

        partial void BuildLinq();
        partial void BuildCisternValueLinq();
        partial void BuildHandcoded();

        internal static void SanityCheck()
        {
            var check = new NetCore20();

            Check.Same(nameof(N01_Linq), check.N01_Linq(), check.N01_CisternValueLinq());
            Check.Same(nameof(N02_Linq), check.N02_Linq(), check.N02_CisternValueLinq());
            Check.Same(nameof(N02_CisternValueLinqInline), check.N02_Linq(), check.N02_CisternValueLinqInline());
            Check.Same(nameof(N03_Linq), check.N03_Linq(), check.N03_CisternValueLinq());
            Check.Same(nameof(N04_Linq), check.N04_Linq(), check.N04_CisternValueLinq());
            Check.Same(nameof(N04_CisternValueLinqInline), check.N04_Linq(), check.N04_CisternValueLinqInline());
            Check.Same(nameof(N03_CisternValueLinqTyped), check.N03_Linq(), check.N03_CisternValueLinqTyped());

            Check.Same(nameof(N02_ZLinq), check.N02_Linq(), check.N02_ZLinq());
            Check.Same(nameof(N03_ZLinq), check.N03_Linq(), check.N03_ZLinq());
            Check.Same(nameof(N04_ZLinq), check.N04_Linq(), check.N04_ZLinq());

            Check.Same(nameof(N01_Handcoded), check.N01_Linq(), check.N01_Handcoded());
            Check.Same(nameof(N02_Handcoded), check.N02_Linq(), check.N02_Handcoded());
            Check.Same(nameof(N03_Handcoded), check.N03_Linq(), check.N03_Handcoded());
            Check.Same(nameof(N04_Handcoded), check.N04_Linq(), check.N04_Handcoded());
        }
    }
}
