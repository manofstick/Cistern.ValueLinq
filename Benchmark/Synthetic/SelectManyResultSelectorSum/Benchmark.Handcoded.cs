using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyResultSelectorSum
    {
        [Benchmark(Baseline = true)]
        public int Handcoded()
        {
            var sum = 0;
            foreach (var row in _rows)
            {
                foreach (var x in row)
                    sum += x + row.Length;
            }
            return sum;
        }
    }
}
