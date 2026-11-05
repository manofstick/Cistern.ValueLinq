using BenchmarkDotNet.Attributes;
using Cistern.Benchmarks.Parity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    // The LINQ benchmarks of the .NET 7 performance post, at the post's sizes and data: Min and Max over an int[] of 4
    // and 1,024 held as IEnumerable<int>, Sum, Average, Min and Max over a float[] of 10,000 held as IEnumerable<float>,
    // and sorted enumeration of a reversed int[1024] through OrderBy(x => x) and Order(). One method per entry (a letter
    // per form where an entry covers several) and library, named by the entry's ID. The post's sorting benchmarks discard
    // the elements; here they are summed, so the sanity check has a result to compare.
    [MemoryDiagnoser]
    public partial class Net7
    {
        IEnumerable<int> _source4;
        IEnumerable<int> _source1024;
        IEnumerable<float> _floats;
        int[] _reversed1024;

        public Net7()
        {
            _source4 = Enumerable.Range(1, 4).ToArray();
            _source1024 = Enumerable.Range(1, 1024).ToArray();
            _floats = CreateRandom();
            _reversed1024 = Enumerable.Range(1, 1024).Reverse().ToArray();

            BuildCisternValueLinq();
        }

        partial void BuildCisternValueLinq();

        private static float[] CreateRandom()
        {
            var r = new Random(42);
            var results = new float[10_000];
            for (var i = 0; i < results.Length; ++i)
                results[i] = (float)r.NextDouble();
            return results;
        }

        internal static void SanityCheck()
        {
            var check = new Net7();

            Check.Same(nameof(N01a_Linq), check.N01a_Linq(), check.N01a_CisternValueLinq());
            Check.Same(nameof(N01b_Linq), check.N01b_Linq(), check.N01b_CisternValueLinq());
            Check.Same(nameof(N02a_Linq), check.N02a_Linq(), check.N02a_CisternValueLinq());
            Check.Same(nameof(N02b_Linq), check.N02b_Linq(), check.N02b_CisternValueLinq());
            Check.Same(nameof(N03a_Linq), check.N03a_Linq(), check.N03a_CisternValueLinq());
            Check.Same(nameof(N03b_Linq), check.N03b_Linq(), check.N03b_CisternValueLinq());
            Check.Same(nameof(N03c_Linq), check.N03c_Linq(), check.N03c_CisternValueLinq());
            Check.Same(nameof(N03d_Linq), check.N03d_Linq(), check.N03d_CisternValueLinq());
            Check.Same(nameof(N04a_Linq), check.N04a_Linq(), check.N04a_CisternValueLinq());
            Check.Same(nameof(N04b_Linq), check.N04b_Linq(), check.N04b_CisternValueLinq());

            Check.Same(nameof(N01a_CisternValueLinqTyped), check.N01a_Linq(), check.N01a_CisternValueLinqTyped());
            Check.Same(nameof(N01b_CisternValueLinqTyped), check.N01b_Linq(), check.N01b_CisternValueLinqTyped());
            Check.Same(nameof(N02a_CisternValueLinqTyped), check.N02a_Linq(), check.N02a_CisternValueLinqTyped());
            Check.Same(nameof(N02b_CisternValueLinqTyped), check.N02b_Linq(), check.N02b_CisternValueLinqTyped());
            Check.Same(nameof(N03a_CisternValueLinqTyped), check.N03a_Linq(), check.N03a_CisternValueLinqTyped());
            Check.Same(nameof(N03b_CisternValueLinqTyped), check.N03b_Linq(), check.N03b_CisternValueLinqTyped());
            Check.Same(nameof(N03c_CisternValueLinqTyped), check.N03c_Linq(), check.N03c_CisternValueLinqTyped());
            Check.Same(nameof(N03d_CisternValueLinqTyped), check.N03d_Linq(), check.N03d_CisternValueLinqTyped());

            Check.Same(nameof(N01a_ZLinq), check.N01a_Linq(), check.N01a_ZLinq());
            Check.Same(nameof(N01b_ZLinq), check.N01b_Linq(), check.N01b_ZLinq());
            Check.Same(nameof(N02a_ZLinq), check.N02a_Linq(), check.N02a_ZLinq());
            Check.Same(nameof(N02b_ZLinq), check.N02b_Linq(), check.N02b_ZLinq());
            Check.Same(nameof(N03a_ZLinq), check.N03a_Linq(), check.N03a_ZLinq());
            Check.Same(nameof(N03b_ZLinq), check.N03b_Linq(), check.N03b_ZLinq());
            Check.Same(nameof(N03c_ZLinq), check.N03c_Linq(), check.N03c_ZLinq());
            Check.Same(nameof(N03d_ZLinq), check.N03d_Linq(), check.N03d_ZLinq());
            Check.Same(nameof(N04a_ZLinq), check.N04a_Linq(), check.N04a_ZLinq());
            Check.Same(nameof(N04b_ZLinq), check.N04b_Linq(), check.N04b_ZLinq());

            Check.Same(nameof(N01a_Handcoded), check.N01a_Linq(), check.N01a_Handcoded());
            Check.Same(nameof(N01b_Handcoded), check.N01b_Linq(), check.N01b_Handcoded());
            Check.Same(nameof(N02a_Handcoded), check.N02a_Linq(), check.N02a_Handcoded());
            Check.Same(nameof(N02b_Handcoded), check.N02b_Linq(), check.N02b_Handcoded());
            Check.Same(nameof(N03a_Handcoded), check.N03a_Linq(), check.N03a_Handcoded());
            Check.Same(nameof(N03b_Handcoded), check.N03b_Linq(), check.N03b_Handcoded());
            Check.Same(nameof(N03c_Handcoded), check.N03c_Linq(), check.N03c_Handcoded());
            Check.Same(nameof(N03d_Handcoded), check.N03d_Linq(), check.N03d_Handcoded());
            Check.Same(nameof(N04a_Handcoded), check.N04a_Linq(), check.N04a_Handcoded());
            Check.Same(nameof(N04b_Handcoded), check.N04b_Linq(), check.N04b_Handcoded());
        }
    }
}
