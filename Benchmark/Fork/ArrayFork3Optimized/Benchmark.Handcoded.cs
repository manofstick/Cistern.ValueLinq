using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Fork
{
    partial class ArrayFork3Optimized
    {
        [Benchmark]
        public (double, double, double) Handcoded()
        {
            var min = double.MaxValue;
            var max = double.MinValue;
            var last = double.NaN;

            foreach(var x in Data)
            {
                if (x > max)
                    max = x;
                if (x < min)
                    min = x;
                last = x;
            }

            return (min, max, last);
        }
    }
}
