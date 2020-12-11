using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.DoubleDoubleDouble
{
    [MemoryDiagnoser]
    public partial class OrderByThenByThenByToArray
    {
        List<(double x, double y, double z)> _doubledoubledoubles;

#if truex
        [Params(0, 1, 10, 100, 1000, 1000000)]
#else
        [Params(100)]
#endif
        public int Length { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            _doubledoubledoubles =
                Enumerable
                .Range(0, Length)
                .Select(x => ((double)(x % (Length/5)), (double)(x % (Length / 7)), (double)x))
                .ToList();

            // shuffle
            var r = new Random(42);
            for (var i=0; i < _doubledoubledoubles.Count; ++i)
            {
                var z = r.Next(i, _doubledoubledoubles.Count);
                var t = _doubledoubledoubles[i];
                _doubledoubledoubles[i] = _doubledoubledoubles[z];
                _doubledoubledoubles[z] = t;
            }

            _doubledoubledoubles =
                _doubledoubledoubles
                .OrderByDescending(x => x)
                .ToList();
        }

        internal static void SanityCheck()
        {
            var check = new OrderByThenByThenByToArray();

            check.Length = 100;
            check.SetupData();

            var baseline = check.Linq();

            var cisternvaluelinq = check.CisternValueLinq();
            if (!Enumerable.SequenceEqual(baseline, cisternvaluelinq)) throw new Exception();

#if CISTERNLINQx
            var cisternlinq = check.CisternLinq();
            if (!Enumerable.SequenceEqual(baseline, cisternlinq)) throw new Exception();
#endif

#if LINQAFx
            var linqaf = check.LinqAF();
            if (!Enumerable.SequenceEqual(baseline, linqaf)) throw new Exception();
#endif
        }
    }
}
