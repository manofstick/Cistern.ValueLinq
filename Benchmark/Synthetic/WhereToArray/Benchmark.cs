using BenchmarkDotNet.Attributes;
using System;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    [MemoryDiagnoser]
    public partial class WhereToArray
    {
        int[] _data;

        // half the elements survive the filter: 10 → 5 (within the stack buffer), 100 → 50 (fills most of it),
        // 1000 → 500 (overflows it), 100000 → 50000
        [Params(10, 100, 1000, 100000)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData() => _data = Create(Length);

        private static int[] Create(int length)
        {
            var data = new int[length];
            for (var i = 0; i < length; ++i)
                data[i] = i;
            return data;
        }

        internal static void SanityCheck()
        {
            var check = new WhereToArray();

            foreach (var length in new[] { 0, 1, 10, 100, 1000 })
            {
                check.Length = length;
                check.SetupData();

                var baseline = check.Handcoded();

                var cisternvaluelinq = check.CisternValueLinq();
                if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq)) throw new Exception();

                var linq = check.Linq();
                if (!Enumerable.SequenceEqual(baseline, linq)) throw new Exception();

                var zlinq = check.ZLinq();
                if (!Enumerable.SequenceEqual(baseline, zlinq)) throw new Exception();
            }
        }
    }
}
