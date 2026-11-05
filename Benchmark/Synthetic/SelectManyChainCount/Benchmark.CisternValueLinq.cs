using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyChainCount
    {
        private static (IEnumerable<int>[], IEnumerable<int>[]) BuildCisternValueLinq(int[][] rows)
        {
            var where = new IEnumerable<int>[rows.Length];
            var select = new IEnumerable<int>[rows.Length];
            for (var r = 0; r < rows.Length; ++r)
            {
                where[r] = rows[r].Where(x => (x & 1) == 0);
                select[r] = rows[r].Select(x => x);
            }
            return (where, select);
        }

        [Benchmark, BenchmarkCategory("Where")]
        public int CisternValueLinq_Where() =>
            _cisternValueLinqWhere
            .SelectMany(row => row)
            .Count();

        [Benchmark, BenchmarkCategory("Select")]
        public int CisternValueLinq_Select() =>
            _cisternValueLinqSelect
            .SelectMany(row => row)
            .Count();
    }
}
