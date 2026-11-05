using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyResultSelectorWhereSum
    {
        [Benchmark(Baseline = true)]
        public int Handcoded()
        {
            var sum = 0;
            foreach (var row in _rows)
            {
                foreach (var x in row)
                {
                    var result = x + row.Length;
                    if ((result & 1) == 0)
                        sum += result;
                }
            }
            return sum;
        }
    }
}
