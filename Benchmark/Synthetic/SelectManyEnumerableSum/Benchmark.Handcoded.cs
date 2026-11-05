using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyEnumerableSum
    {
        [Benchmark(Baseline = true)]
        public int Handcoded()
        {
            var sum = 0;
            foreach (var row in _rows)
            {
                foreach (var x in row)
                    sum += x;
            }
            return sum;
        }
    }
}
