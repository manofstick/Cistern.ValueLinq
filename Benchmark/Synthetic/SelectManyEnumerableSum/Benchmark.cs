using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    // Inner sequences that cannot expose a span, so SelectMany pushes each through the shared downstream chain.
    [MemoryDiagnoser]
    public partial class SelectManyEnumerableSum
    {
        Queue<int>[] _rows;

        [Params(100, 10000)]
        public int Rows { get; set; } = 0;

        [GlobalSetup]
        public void SetupData() => _rows = Create(Rows);

        // row r has (r * 37) % 200 elements, so 10,000 rows is about a million elements
        private static Queue<int>[] Create(int rows)
        {
            var data = new Queue<int>[rows];
            for (var r = 0; r < rows; ++r)
            {
                var length = (r * 37) % 200;
                var row = new Queue<int>(length);
                for (var j = 0; j < length; ++j)
                    row.Enqueue((r * 1000 + j) % 1000);
                data[r] = row;
            }
            return data;
        }

        internal static void SanityCheck()
        {
            var check = new SelectManyEnumerableSum();

            check.Rows = 1000;
            check.SetupData();

            var baseline = check.Handcoded();

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

            var linq = check.Linq();
            if (baseline != linq) throw new Exception();

            var zlinq = check.ZLinq();
            if (baseline != zlinq) throw new Exception();
        }
    }
}
