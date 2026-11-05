using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyChainCount
    {
        private static (IEnumerable<int>[], IEnumerable<int>[]) BuildLinq(int[][] rows)
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
        public int Linq_Where() =>
            _linqWhere
            .SelectMany(row => row)
            .Count();

        [Benchmark, BenchmarkCategory("Select")]
        public int Linq_Select() =>
            _linqSelect
            .SelectMany(row => row)
            .Count();
    }
}
