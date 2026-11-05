using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyChainCount
    {
        [Benchmark(Baseline = true), BenchmarkCategory("Where")]
        public int Handcoded_Where()
        {
            var count = 0;
            foreach (var row in _rows)
            {
                foreach (var x in row)
                {
                    if ((x & 1) == 0)
                        ++count;
                }
            }
            return count;
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Select")]
        public int Handcoded_Select()
        {
            var count = 0;
            foreach (var row in _rows)
                count += row.Length;
            return count;
        }
    }
}
