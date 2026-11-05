using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyChainSum
    {
        private static IEnumerable<int>[] BuildLinq(int[][] rows)
        {
            var chains = new IEnumerable<int>[rows.Length];
            for (var r = 0; r < rows.Length; ++r)
                chains[r] = rows[r].Select(x => x);
            return chains;
        }

        private static IEnumerable<int> BuildLinqWrapped(IEnumerable<int>[] chains) =>
            new Forwarding(chains.SelectMany(row => row));

        [Benchmark, BenchmarkCategory("Plain")]
        public int Linq_Plain() =>
            _linq
            .SelectMany(row => row)
            .Sum();

        [Benchmark, BenchmarkCategory("ResultSelector")]
        public int Linq_ResultSelector() =>
            _linq
            .SelectMany(row => row, (row, x) => x)
            .Sum();

        [Benchmark, BenchmarkCategory("Wrapped")]
        public int Linq_Wrapped() =>
            _linqWrapped
            .Sum();

        [Benchmark, BenchmarkCategory("Returned")]
        public int Linq_Returned() =>
            _rows
            .SelectMany(row => row.Select(x => x))
            .Sum();

        [Benchmark, BenchmarkCategory("ReturnedResultSelector")]
        public int Linq_ReturnedResultSelector() =>
            _rows
            .SelectMany(row => row.Select(x => x), (row, x) => x)
            .Sum();
    }
}
