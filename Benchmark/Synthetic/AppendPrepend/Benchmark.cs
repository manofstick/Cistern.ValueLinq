using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    // one prepend and one append around an array: each workload is its own category with its own hand-coded baseline
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class AppendPrepend
    {
        const int Prepended = 7;
        const int Appended = 9;

        int[] _data;

        [Params(10, 1000, 100000)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData() => _data = Create(Length);

        // small values, so a checked int Sum cannot overflow
        private static int[] Create(int length)
        {
            var data = new int[length];
            for (var i = 0; i < length; ++i)
                data[i] = i % 100;
            return data;
        }

        internal static void SanityCheck()
        {
            var check = new AppendPrepend();

            foreach (var length in new[] { 0, 1, 10, 1000 })
            {
                check.Length = length;
                check.SetupData();

                var sum = check.Handcoded_Sum();
                if (sum != check.CisternValueLinq_Sum()) throw new Exception();
                if (sum != check.Linq_Sum()) throw new Exception();
                if (sum != check.ZLinq_Sum()) throw new Exception();
                if (sum != check.Handcoded_Foreach()) throw new Exception();
                if (sum != check.CisternValueLinq_Foreach()) throw new Exception();
                if (sum != check.Linq_Foreach()) throw new Exception();
                if (sum != check.ZLinq_Foreach()) throw new Exception();

                var reverseSum = check.Handcoded_ReverseSum();
                if (reverseSum != sum) throw new Exception();
                if (reverseSum != check.CisternValueLinq_ReverseSum()) throw new Exception();
                if (reverseSum != check.Linq_ReverseSum()) throw new Exception();
                if (reverseSum != check.ZLinq_ReverseSum()) throw new Exception();

                var reversed = check.Handcoded_ReverseToArray();
                if (!Enumerable.SequenceEqual(reversed, Enumerable.Reverse(check.Handcoded_ToArray()))) throw new Exception();
                if (!Enumerable.SequenceEqual(reversed, check.CisternValueLinq_ReverseToArray())) throw new Exception();
                if (!Enumerable.SequenceEqual(reversed, check.Linq_ReverseToArray())) throw new Exception();
                if (!Enumerable.SequenceEqual(reversed, check.ZLinq_ReverseToArray())) throw new Exception();

                var array = check.Handcoded_ToArray();
                if (!Enumerable.SequenceEqual(array, check.CisternValueLinq_ToArray())) throw new Exception();
                if (!Enumerable.SequenceEqual(array, check.Linq_ToArray())) throw new Exception();
                if (!Enumerable.SequenceEqual(array, check.ZLinq_ToArray())) throw new Exception();

                var list = check.Handcoded_ToList();
                if (!Enumerable.SequenceEqual(array, list)) throw new Exception();
                if (!Enumerable.SequenceEqual(list, check.CisternValueLinq_ToList())) throw new Exception();
                if (!Enumerable.SequenceEqual(list, check.Linq_ToList())) throw new Exception();
                if (!Enumerable.SequenceEqual(list, check.ZLinq_ToList())) throw new Exception();
            }
        }
    }
}
