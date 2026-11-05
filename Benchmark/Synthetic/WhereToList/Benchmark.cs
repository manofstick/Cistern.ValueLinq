using BenchmarkDotNet.Attributes;
using System;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    [MemoryDiagnoser]
    public partial class WhereToList
    {
        int[] _data;

        // ToList takes the stack path only when the source's maximum length is within the stack buffer,
        // so 10 and 64 exercise it and 100 and 100000 take the List.Add path
        [Params(10, 64, 100, 100000)]
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
            var check = new WhereToList();

            foreach (var length in new[] { 0, 1, 10, 64, 100, 1000 })
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
