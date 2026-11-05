using BenchmarkDotNet.Attributes;
using Cistern.Benchmarks.Parity;

namespace Cistern.Benchmarks.Toub
{
    // The LINQ benchmarks of the .NET Core 3.0 performance post, at the post's sizes: TakeLast(10).Sum() over a Select
    // of an int[1000] held as IEnumerable<int>, Range(0, 100).Select(i => i * 2).ToArray(), and
    // Empty<int>().Take(10).Select(i => i).ToArray(). One method per entry and library, named by the entry's ID. A chain
    // held as IEnumerable<T> is built by each library into its own field, except ZLinq's, a ref struct with no
    // IEnumerable<T> form, built in the method, where Cistern has a copy of the same form (CisternValueLinqInline).
    [MemoryDiagnoser]
    public partial class NetCore30
    {
        int[] _zeros = new int[1000];

        public NetCore30()
        {
            BuildLinq();
            BuildCisternValueLinq();
            BuildHandcoded();
        }

        partial void BuildLinq();
        partial void BuildCisternValueLinq();
        partial void BuildHandcoded();

        internal static void SanityCheck()
        {
            var check = new NetCore30();

            Check.Same(nameof(N01_Linq), check.N01_Linq(), check.N01_CisternValueLinq());
            Check.Same(nameof(N01_CisternValueLinqInline), check.N01_Linq(), check.N01_CisternValueLinqInline());
            Check.Same(nameof(N02_Linq), check.N02_Linq(), check.N02_CisternValueLinq());
            Check.Same(nameof(N03_Linq), check.N03_Linq(), check.N03_CisternValueLinq());

            Check.Same(nameof(N01_ZLinq), check.N01_Linq(), check.N01_ZLinq());
            Check.Same(nameof(N02_ZLinq), check.N02_Linq(), check.N02_ZLinq());
            Check.Same(nameof(N03_ZLinq), check.N03_Linq(), check.N03_ZLinq());

            Check.Same(nameof(N01_Handcoded), check.N01_Linq(), check.N01_Handcoded());
            Check.Same(nameof(N02_Handcoded), check.N02_Linq(), check.N02_Handcoded());
            Check.Same(nameof(N03_Handcoded), check.N03_Linq(), check.N03_Handcoded());
        }
    }
}
