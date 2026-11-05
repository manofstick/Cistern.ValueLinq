using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    // a chain of known static depth over an IEnumerable<int> source, handed out as IEnumerable<int>: each GetEnumerator
    // rebuilds the chain over the array the source holds, so with few elements the time is that rebuild's
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class DeepChainEnumerator
    {
        int[] _array = [];
        IEnumerable<int> _depth4 = [];
        IEnumerable<int> _depth16 = [];
        IEnumerable<int> _depth32 = [];
        IEnumerable<int> _concat = [];

        [Params(0, 1, 10)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            _array = new int[Length];
            for (var i = 0; i < Length; ++i)
                _array[i] = (i * 31) % 1000;

            IEnumerable<int> source = _array;
            _depth4 = BuildCisternValueLinq_Depth4(source);
            _depth16 = BuildCisternValueLinq_Depth16(source);
            _depth32 = BuildCisternValueLinq_Depth32(source);
            _concat = BuildCisternValueLinq_Concat(source);
        }

        internal static void SanityCheck()
        {
            var check = new DeepChainEnumerator();

            foreach (var length in new[] { 0, 1, 10, 1000 })
            {
                check.Length = length;
                check.SetupData();

                if (check.Handcoded_Depth4() != check.CisternValueLinq_Depth4()) throw new Exception();
                if (check.Handcoded_Depth16() != check.CisternValueLinq_Depth16()) throw new Exception();
                if (check.Handcoded_Depth32() != check.CisternValueLinq_Depth32()) throw new Exception();
                if (check.Handcoded_Concat() != check.CisternValueLinq_Concat()) throw new Exception();
            }
        }
    }
}
