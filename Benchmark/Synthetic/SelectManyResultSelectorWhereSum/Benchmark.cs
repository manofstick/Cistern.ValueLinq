using BenchmarkDotNet.Attributes;
using System;

namespace Cistern.Benchmarks.Synthetic
{
    // The result-selector SelectMany over inner arrays with a Where between it and the Sum, so its downstream is a
    // forwarder rather than an aggregator: the case the aggregator marker leaves on the by-ref path.
    [MemoryDiagnoser]
    public partial class SelectManyResultSelectorWhereSum
    {
        int[][] _rows;

        [Params(100, 10000)]
        public int Rows { get; set; } = 0;

        [GlobalSetup]
        public void SetupData() => _rows = Create(Rows);

        // row r has (r * 37) % 200 elements, so 10,000 rows is about a million elements
        private static int[][] Create(int rows)
        {
            var data = new int[rows][];
            for (var r = 0; r < rows; ++r)
            {
                var row = new int[(r * 37) % 200];
                for (var j = 0; j < row.Length; ++j)
                    row[j] = (r * 1000 + j) % 1000;
                data[r] = row;
            }
            return data;
        }

        internal static void SanityCheck()
        {
            var check = new SelectManyResultSelectorWhereSum();

            check.Rows = 1000;
            check.SetupData();

            var baseline = check.Handcoded();

            var cisternvaluelinq = check.CisternValueLinq();
            if (baseline != cisternvaluelinq) throw new Exception();

            var linq = check.Linq();
            if (baseline != linq) throw new Exception();
        }
    }
}
