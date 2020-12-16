using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Fork
{
    partial class ArrayWhereSelectFork3
    {
        [Benchmark]
        public (long, int, double) Handcoded()
        {
            var last = 0L;
            var count = 0;
            var sum = 0.0;

            foreach(var asInt in Data)
            {
                if ((asInt & 1) == 0)
                {
                    var asLong = (long)asInt;

                    last = asLong;
                    count++;
                    sum += asLong;
                }
            }

            var average =
                sum / count;

            return (last, count, average);
        }
    }
}
