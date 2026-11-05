using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    // Count over a SelectMany whose inner sequences are each library's own chain over an array, held as
    // IEnumerable<int>: a Where, which has to be run to be counted, and a Select, whose selector may have side effects
    // so it is run too. Each workload is its own category with its own hand-coded baseline.
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class SelectManyChainCount
    {
        int[][] _rows;
        IEnumerable<int>[] _cisternValueLinqWhere;
        IEnumerable<int>[] _cisternValueLinqSelect;
        IEnumerable<int>[] _linqWhere;
        IEnumerable<int>[] _linqSelect;

        [Params(100, 10000)]
        public int Rows { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            _rows = Create(Rows);
            (_cisternValueLinqWhere, _cisternValueLinqSelect) = BuildCisternValueLinq(_rows);
            (_linqWhere, _linqSelect) = BuildLinq(_rows);
        }

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
            var check = new SelectManyChainCount();

            check.Rows = 1000;
            check.SetupData();

            var where = check.Handcoded_Where();
            if (where != check.CisternValueLinq_Where()) throw new Exception();
            if (where != check.Linq_Where()) throw new Exception();

            var select = check.Handcoded_Select();
            if (select != check.CisternValueLinq_Select()) throw new Exception();
            if (select != check.Linq_Select()) throw new Exception();
        }
    }
}
