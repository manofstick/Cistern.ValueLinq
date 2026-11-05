using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyChainSum
    {
        private static IEnumerable<int>[] BuildCisternValueLinq(int[][] rows)
        {
            var chains = new IEnumerable<int>[rows.Length];
            for (var r = 0; r < rows.Length; ++r)
                chains[r] = rows[r].Select(x => x);
            return chains;
        }

        private static IEnumerable<int> BuildCisternValueLinqWrapped(IEnumerable<int>[] chains) =>
            new Forwarding(chains.SelectMany(row => row));

        [Benchmark, BenchmarkCategory("Plain")]
        public int CisternValueLinq_Plain() =>
            _cisternValueLinq
            .SelectMany(row => row)
            .Sum();

        [Benchmark, BenchmarkCategory("ResultSelector")]
        public int CisternValueLinq_ResultSelector() =>
            _cisternValueLinq
            .SelectMany(row => row, (row, x) => x)
            .Sum();

        [Benchmark, BenchmarkCategory("Wrapped")]
        public int CisternValueLinq_Wrapped() =>
            _cisternValueLinqWrapped
            .Sum();

        [Benchmark, BenchmarkCategory("Returned")]
        public int CisternValueLinq_Returned() =>
            _rows
            .SelectMany(row => row.Select(x => x))
            .Sum();

        [Benchmark, BenchmarkCategory("ReturnedResultSelector")]
        public int CisternValueLinq_ReturnedResultSelector() =>
            _rows
            .SelectMany(row => row.Select(x => x), (row, x) => x)
            .Sum();
    }
}
