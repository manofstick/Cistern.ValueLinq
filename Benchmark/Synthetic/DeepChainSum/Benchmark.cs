using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;

namespace Cistern.Benchmarks.Synthetic
{
    // a push over a chain of known static depth on an array: with few elements the time is that of building the
    // forwarder chain; each chain is built in setup and held in a closure, so its static type survives
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class DeepChainSum
    {
        int[] _array = [];
        Func<int> _depth1 = () => 0;
        Func<int> _depth2 = () => 0;
        Func<int> _depth4 = () => 0;
        Func<int> _depth16 = () => 0;
        Func<int> _depth32 = () => 0;
        Func<int> _concat = () => 0;

        [Params(0, 1, 10)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            _array = new int[Length];
            for (var i = 0; i < Length; ++i)
                _array[i] = (i * 31) % 1000;

            _depth1 = BuildCisternValueLinq_Depth1(_array);
            _depth2 = BuildCisternValueLinq_Depth2(_array);
            _depth4 = BuildCisternValueLinq_Depth4(_array);
            _depth16 = BuildCisternValueLinq_Depth16(_array);
            _depth32 = BuildCisternValueLinq_Depth32(_array);
            _concat = BuildCisternValueLinq_Concat(_array);
        }

        internal static void SanityCheck()
        {
            var check = new DeepChainSum();

            foreach (var length in new[] { 0, 1, 10, 1000 })
            {
                check.Length = length;
                check.SetupData();

                if (check.Handcoded_Depth1() != check.CisternValueLinq_Depth1()) throw new Exception();
                if (check.Handcoded_Depth2() != check.CisternValueLinq_Depth2()) throw new Exception();
                if (check.Handcoded_Depth4() != check.CisternValueLinq_Depth4()) throw new Exception();
                if (check.Handcoded_Depth16() != check.CisternValueLinq_Depth16()) throw new Exception();
                if (check.Handcoded_Depth32() != check.CisternValueLinq_Depth32()) throw new Exception();
                if (check.Handcoded_Concat() != check.CisternValueLinq_Concat()) throw new Exception();

                if (check.Handcoded_Depth1() != check.ZLinq_Depth1()) throw new Exception();
                if (check.Handcoded_Depth2() != check.ZLinq_Depth2()) throw new Exception();
                if (check.Handcoded_Depth4() != check.ZLinq_Depth4()) throw new Exception();
                if (check.Handcoded_Depth16() != check.ZLinq_Depth16()) throw new Exception();
                if (check.Handcoded_Depth32() != check.ZLinq_Depth32()) throw new Exception();
                if (check.Handcoded_Concat() != check.ZLinq_Concat()) throw new Exception();

                if (check.Handcoded_Depth1() != check.Linq_Depth1()) throw new Exception();
                if (check.Handcoded_Depth2() != check.Linq_Depth2()) throw new Exception();
                if (check.Handcoded_Depth4() != check.Linq_Depth4()) throw new Exception();
                if (check.Handcoded_Depth16() != check.Linq_Depth16()) throw new Exception();
                if (check.Handcoded_Depth32() != check.Linq_Depth32()) throw new Exception();
                if (check.Handcoded_Concat() != check.Linq_Concat()) throw new Exception();

                if (check.Handcoded_Depth1() != check.CisternValueLinqPerCall_Depth1()) throw new Exception();
                if (check.Handcoded_Depth2() != check.CisternValueLinqPerCall_Depth2()) throw new Exception();
                if (check.Handcoded_Depth4() != check.CisternValueLinqPerCall_Depth4()) throw new Exception();
                if (check.Handcoded_Depth16() != check.CisternValueLinqPerCall_Depth16()) throw new Exception();
                if (check.Handcoded_Depth32() != check.CisternValueLinqPerCall_Depth32()) throw new Exception();
                if (check.Handcoded_Concat() != check.CisternValueLinqPerCall_Concat()) throw new Exception();
            }
        }
    }
}
