using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    // Inner sequences that are each library's own chain over an array, held as IEnumerable<int>, so SelectMany meets
    // every inner sequence across an IEnumerable<T> boundary: with and without a result selector, and the SelectMany
    // handed out through a wrapper that only forwards GetEnumerator before it is summed. The Returned categories build
    // each inner chain in the selector instead, so its static type is the chain's own. Each workload is its own
    // category with its own hand-coded baseline.
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class SelectManyChainSum
    {
        int[][] _rows;
        IEnumerable<int>[] _cisternValueLinq;
        IEnumerable<int> _cisternValueLinqWrapped;
        IEnumerable<int>[] _linq;
        IEnumerable<int> _linqWrapped;

        [Params(100, 10000)]
        public int Rows { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            _rows = Create(Rows);
            _cisternValueLinq = BuildCisternValueLinq(_rows);
            _cisternValueLinqWrapped = BuildCisternValueLinqWrapped(_cisternValueLinq);
            _linq = BuildLinq(_rows);
            _linqWrapped = BuildLinqWrapped(_linq);
        }

        private sealed class Forwarding(IEnumerable<int> source)
            : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator() => source.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
            var check = new SelectManyChainSum();

            check.Rows = 1000;
            check.SetupData();

            var baseline = check.Handcoded_Plain();
            if (baseline != check.Handcoded_ResultSelector()) throw new Exception();
            if (baseline != check.Handcoded_Wrapped()) throw new Exception();
            if (baseline != check.Handcoded_Returned()) throw new Exception();
            if (baseline != check.Handcoded_ReturnedResultSelector()) throw new Exception();

            if (baseline != check.CisternValueLinq_Plain()) throw new Exception();
            if (baseline != check.CisternValueLinq_ResultSelector()) throw new Exception();
            if (baseline != check.CisternValueLinq_Wrapped()) throw new Exception();
            if (baseline != check.CisternValueLinq_Returned()) throw new Exception();
            if (baseline != check.CisternValueLinq_ReturnedResultSelector()) throw new Exception();

            if (baseline != check.Linq_Plain()) throw new Exception();
            if (baseline != check.Linq_ResultSelector()) throw new Exception();
            if (baseline != check.Linq_Wrapped()) throw new Exception();
            if (baseline != check.Linq_Returned()) throw new Exception();
            if (baseline != check.Linq_ReturnedResultSelector()) throw new Exception();
        }
    }
}
