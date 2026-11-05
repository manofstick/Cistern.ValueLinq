using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class ConcatWrapped
    {
        [Benchmark(Baseline = true), BenchmarkCategory("Sum")]
        public int Handcoded_Sum()
        {
            var sum = 0;
            foreach (var part in _parts)
            {
                foreach (var x in part)
                    sum += x;
            }
            return sum;
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Count")]
        public int Handcoded_Count()
        {
            var count = 0;
            foreach (var part in _parts)
                count += part.Length;
            return count;
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Foreach")]
        public int Handcoded_Foreach()
        {
            var sum = 0;
            foreach (var part in _parts)
            {
                foreach (var x in part)
                    sum += x;
            }
            return sum;
        }
    }
}
