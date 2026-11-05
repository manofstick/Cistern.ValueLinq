using BenchmarkDotNet.Attributes;
using System;

namespace Cistern.Benchmarks.Synthetic
{
    // exercises the batch-from-local-copy path: ValueLinq/Docs/BatchFromLocalCopy.md, "Benchmarks"
    [MemoryDiagnoser]
    public partial class SelectManyWhereSum
    {
        int[][] _rows;

        [Params(1, 100, 10000)]
        public int Rows { get; set; } = 0;

        // the average row length; varying it at a fixed row count separates the per-row cost from the per-element cost
        [Params(10, 100, 1000)]
        public int RowLength { get; set; } = 100;

        [GlobalSetup]
        public void SetupData() => _rows = Create(Rows, RowLength);

        // row r has (r * 37) % (2 * rowLength) elements, so 10,000 rows of length 100 is about a million elements
        private static int[][] Create(int rows, int rowLength)
        {
            var data = new int[rows][];
            for (var r = 0; r < rows; ++r)
            {
                var row = new int[(r * 37) % (2 * rowLength)];
                for (var j = 0; j < row.Length; ++j)
                    row[j] = (r * 1000 + j) % 1000;
                data[r] = row;
            }
            return data;
        }

        internal static void SanityCheck()
        {
            var check = new SelectManyWhereSum();

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
