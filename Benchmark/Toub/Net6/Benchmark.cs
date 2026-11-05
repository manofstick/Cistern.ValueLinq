using BenchmarkDotNet.Attributes;
using Cistern.Benchmarks.Parity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    // The LINQ benchmarks of the .NET 6 performance post, at the post's sizes and data: Distinct().Count() over strings,
    // SequenceEqual of two arrays held as IEnumerable<int>, Max and Min over TimeSpans, a three-way Zip through two
    // two-way Zips and through the three-source overload, and Skip().Take() against Take(Range) over an iterator. One
    // method per entry (a letter per form where an entry covers several) and library, named by the entry's ID. Every
    // Range is the library's own; a chain held as IEnumerable<T> is built by each library into its own field, except
    // ZLinq's, a ref struct with no IEnumerable<T> form, built in the method. The post's enumerating benchmarks discard
    // the elements; here they are summed, so the sanity check has a result to compare.
    [MemoryDiagnoser]
    public partial class Net6
    {
        IEnumerable<string> _data;
        IEnumerable<int> _e1;
        IEnumerable<int> _e2;
        TimeSpan[] _values;
        IEnumerable<int> _iterator;

        public Net6()
        {
            _data = Enumerable.Range(0, 100_000).Select(i => i.ToString()).ToArray();
            _e1 = Enumerable.Range(0, 1_000_000).ToArray();
            _e2 = Enumerable.Range(0, 1_000_000).ToArray();
            _values = Enumerable.Range(0, 1_000_000).Select(i => TimeSpan.FromMilliseconds(i)).ToArray();
            _iterator = Range(10_000);

            BuildLinq();
            BuildCisternValueLinq();
        }

        partial void BuildLinq();
        partial void BuildCisternValueLinq();

        // N05's source, and N04's for the hand-coded zip, which no library builds
        private static IEnumerable<int> Range(int count)
        {
            for (int i = 0; i < count; i++) yield return i;
        }

        internal static void SanityCheck()
        {
            var check = new Net6();

            Check.Same(nameof(N01_Linq), check.N01_Linq(), check.N01_CisternValueLinq());
            Check.Same(nameof(N02_Linq), check.N02_Linq() ? 1 : 0, check.N02_CisternValueLinq() ? 1 : 0);
            Check.Same(nameof(N03a_Linq), check.N03a_Linq(), check.N03a_CisternValueLinq());
            Check.Same(nameof(N03b_Linq), check.N03b_Linq(), check.N03b_CisternValueLinq());
            Check.Same(nameof(N04a_Linq), check.N04a_Linq(), check.N04a_CisternValueLinq());
            Check.Same(nameof(N04b_Linq), check.N04b_Linq(), check.N04b_CisternValueLinq());
            Check.Same(nameof(N04a_CisternValueLinqInline), check.N04a_Linq(), check.N04a_CisternValueLinqInline());
            Check.Same(nameof(N04b_CisternValueLinqInline), check.N04b_Linq(), check.N04b_CisternValueLinqInline());
            Check.Same(nameof(N05a_Linq), check.N05a_Linq(), check.N05a_CisternValueLinq());
            Check.Same(nameof(N05b_Linq), check.N05b_Linq(), check.N05b_CisternValueLinq());

            Check.Same(nameof(N01_CisternValueLinqTyped), check.N01_Linq(), check.N01_CisternValueLinqTyped());
            Check.Same(nameof(N02_CisternValueLinqTyped), check.N02_Linq() ? 1 : 0, check.N02_CisternValueLinqTyped() ? 1 : 0);

            Check.Same(nameof(N01_ZLinq), check.N01_Linq(), check.N01_ZLinq());
            Check.Same(nameof(N02_ZLinq), check.N02_Linq() ? 1 : 0, check.N02_ZLinq() ? 1 : 0);
            Check.Same(nameof(N03a_ZLinq), check.N03a_Linq(), check.N03a_ZLinq());
            Check.Same(nameof(N03b_ZLinq), check.N03b_Linq(), check.N03b_ZLinq());
            Check.Same(nameof(N04a_ZLinq), check.N04a_Linq(), check.N04a_ZLinq());
            Check.Same(nameof(N04b_ZLinq), check.N04b_Linq(), check.N04b_ZLinq());
            Check.Same(nameof(N05a_ZLinq), check.N05a_Linq(), check.N05a_ZLinq());
            Check.Same(nameof(N05b_ZLinq), check.N05b_Linq(), check.N05b_ZLinq());

            Check.Same(nameof(N01_Handcoded), check.N01_Linq(), check.N01_Handcoded());
            Check.Same(nameof(N02_Handcoded), check.N02_Linq() ? 1 : 0, check.N02_Handcoded() ? 1 : 0);
            Check.Same(nameof(N03a_Handcoded), check.N03a_Linq(), check.N03a_Handcoded());
            Check.Same(nameof(N03b_Handcoded), check.N03b_Linq(), check.N03b_Handcoded());
            Check.Same(nameof(N04a_Handcoded), check.N04a_Linq(), check.N04a_Handcoded());
            Check.Same(nameof(N04b_Handcoded), check.N04b_Linq(), check.N04b_Handcoded());
            Check.Same(nameof(N05a_Handcoded), check.N05a_Linq(), check.N05a_Handcoded());
            Check.Same(nameof(N05b_Handcoded), check.N05b_Linq(), check.N05b_Handcoded());
        }
    }
}
