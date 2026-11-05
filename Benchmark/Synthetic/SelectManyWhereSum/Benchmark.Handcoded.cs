using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyWhereSum
    {
        [Benchmark(Baseline = true)]
        public long Handcoded()
        {
            var sum = 0L;
            foreach (var row in _rows)
            {
                foreach (var x in row)
                {
                    if (x % 3 == 0)
                        sum += (long)x * x;
                }
            }
            return sum;
        }
    }
}
