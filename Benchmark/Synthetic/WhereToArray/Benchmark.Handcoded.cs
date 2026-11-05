using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class WhereToArray
    {
        [Benchmark(Baseline = true)]
        public int[] Handcoded()
        {
            var count = 0;
            foreach (var x in _data)
            {
                if (x % 2 == 0)
                    ++count;
            }

            var result = new int[count];
            var idx = 0;
            foreach (var x in _data)
            {
                if (x % 2 == 0)
                    result[idx++] = x;
            }
            return result;
        }
    }
}
