using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Fork
{
    partial class RangeSelectWhereFork3
    {
        [Benchmark]
        public (long, int, double) Handcoded()
        {
            var last = 0L;
            var count = 0;
            var sum = 0.0;

            for (var i=0; i < Length; ++i)
            {
                var value = (long)i;
                if ((value & 1) == 0)
                {
                    last = value;
                    count++;
                    sum += value;
                }
            }

            var average =
                sum / count;

            return (last, count, average);
        }
    }
}
