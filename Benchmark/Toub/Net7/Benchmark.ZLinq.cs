using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net7
    {
        [Benchmark] public int N01a_ZLinq() => _source4.AsValueEnumerable().Min();
        [Benchmark] public int N01b_ZLinq() => _source1024.AsValueEnumerable().Min();

        [Benchmark] public int N02a_ZLinq() => _source4.AsValueEnumerable().Max();
        [Benchmark] public int N02b_ZLinq() => _source1024.AsValueEnumerable().Max();

        [Benchmark] public float N03a_ZLinq() => _floats.AsValueEnumerable().Sum();
        [Benchmark] public float N03b_ZLinq() => _floats.AsValueEnumerable().Average();
        [Benchmark] public float N03c_ZLinq() => _floats.AsValueEnumerable().Min();
        [Benchmark] public float N03d_ZLinq() => _floats.AsValueEnumerable().Max();

        [Benchmark]
        public int N04a_ZLinq()
        {
            int sum = 0;
            foreach (int i in _reversed1024.AsValueEnumerable().OrderBy(x => x))
                sum += i;
            return sum;
        }

        [Benchmark]
        public int N04b_ZLinq()
        {
            int sum = 0;
            foreach (int i in _reversed1024.AsValueEnumerable().Order())
                sum += i;
            return sum;
        }
    }
}
