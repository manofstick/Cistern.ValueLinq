using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net5
    {
        [Benchmark]
        public int N01_ZLinq()
        {
            int sum = 0;
            foreach (int i in _array.AsValueEnumerable().OrderBy(i => i))
                sum += i;
            return sum;
        }

        [Benchmark] public int N02_ZLinq() => _data.AsValueEnumerable().SkipLast(5).Sum();
    }
}
