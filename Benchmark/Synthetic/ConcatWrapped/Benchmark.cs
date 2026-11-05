using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    // a concat built one part at a time, each step applied to a wrapper that only forwards GetEnumerator, so no level
    // can see the concat below it; each workload is its own category with its own hand-coded baseline
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class ConcatWrapped
    {
        int[][] _parts;
        IEnumerable<int> _cisternValueLinq;
        IEnumerable<int> _linq;

        [Params(10, 100, 500)]
        public int Depth { get; set; } = 0;

        [Params(1, 100)]
        public int PartLength { get; set; } = 0;

        [GlobalSetup]
        public void SetupData()
        {
            _parts = Create(Depth, PartLength);
            _cisternValueLinq = BuildCisternValueLinq(_parts);
            _linq = BuildLinq(_parts);
        }

        private sealed class Forwarding(IEnumerable<int> source)
            : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator() => source.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        // small values, so a checked int Sum cannot overflow
        private static int[][] Create(int depth, int partLength)
        {
            var parts = new int[depth][];
            for (var i = 0; i < depth; ++i)
            {
                var part = new int[partLength];
                for (var j = 0; j < partLength; ++j)
                    part[j] = (i * partLength + j) % 100;
                parts[i] = part;
            }
            return parts;
        }

        internal static void SanityCheck()
        {
            var check = new ConcatWrapped();

            foreach (var (depth, partLength) in new[] { (1, 0), (1, 1), (10, 1), (100, 100) })
            {
                check.Depth = depth;
                check.PartLength = partLength;
                check.SetupData();

                var sum = check.Handcoded_Sum();
                if (sum != check.CisternValueLinq_Sum()) throw new Exception();
                if (sum != check.Linq_Sum()) throw new Exception();
                if (sum != check.Handcoded_Foreach()) throw new Exception();
                if (sum != check.CisternValueLinq_Foreach()) throw new Exception();
                if (sum != check.Linq_Foreach()) throw new Exception();

                var count = check.Handcoded_Count();
                if (count != depth * partLength) throw new Exception();
                if (count != check.CisternValueLinq_Count()) throw new Exception();
                if (count != check.Linq_Count()) throw new Exception();
            }
        }
    }
}
