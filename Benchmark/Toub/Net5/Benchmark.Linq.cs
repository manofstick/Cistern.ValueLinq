using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net5
    {
        [Benchmark]
        public int N01_Linq()
        {
            int sum = 0;
            foreach (int i in _array.OrderBy(i => i))
                sum += i;
            return sum;
        }

        [Benchmark] public int N02_Linq() => _data.SkipLast(5).Sum();
    }
}
