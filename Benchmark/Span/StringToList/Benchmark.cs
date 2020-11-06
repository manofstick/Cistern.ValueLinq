using BenchmarkDotNet.Attributes;
using System;
using System.Text;

#nullable enable

namespace Cistern.Benchmarks.Span
{
    [MemoryDiagnoser]
    public partial class StringToList
    {
        string _text = "";

        [Params(0, 1, 10, 100)]
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            var c = 'a';
            var sb = new StringBuilder();
            while (sb.Length < Length)
            {
                sb.Append(c);
                if (c == 'z')
                    c = 'a';
                else
                    ++c;
            }
            _text = sb.ToString();
        }


        internal static void SanityCheck()
        {
            var check = new StringToList();

            check.Length = 100;
            check.SetupData();

            var baseline = check.Linq();

#if LINQAF
            var linqaf = check.LinqAF();
            if (!System.Linq.Enumerable.SequenceEqual(baseline, linqaf)) throw new Exception();
#endif

            var cisternvaluelinq = check.CisternValueLinq();
            if (!System.Linq.Enumerable.SequenceEqual(baseline, cisternvaluelinq)) throw new Exception();

            var cisternvaluelinq_viaspan = check.CisternValueLinq_ViaSpan();
            if (!System.Linq.Enumerable.SequenceEqual(baseline, cisternvaluelinq_viaspan)) throw new Exception();

#if CISTERNLINQ
            var cisternlinq = check.CisternLinq();
            if (!System.Linq.Enumerable.SequenceEqual(cisternlinq, baseline)) throw new Exception();
#endif
        }
    }
}
