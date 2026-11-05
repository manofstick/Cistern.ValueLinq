using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class SelectManyChainSum
    {
        private int Handcoded()
        {
            var sum = 0;
            foreach (var row in _rows)
            {
                foreach (var x in row)
                    sum += x;
            }
            return sum;
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Plain")]
        public int Handcoded_Plain() => Handcoded();

        [Benchmark(Baseline = true), BenchmarkCategory("ResultSelector")]
        public int Handcoded_ResultSelector() => Handcoded();

        [Benchmark(Baseline = true), BenchmarkCategory("Wrapped")]
        public int Handcoded_Wrapped() => Handcoded();

        [Benchmark(Baseline = true), BenchmarkCategory("Returned")]
        public int Handcoded_Returned() => Handcoded();

        [Benchmark(Baseline = true), BenchmarkCategory("ReturnedResultSelector")]
        public int Handcoded_ReturnedResultSelector() => Handcoded();
    }
}
