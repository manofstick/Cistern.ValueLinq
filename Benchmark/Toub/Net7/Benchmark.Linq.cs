using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net7
    {
        [Benchmark] public int N01a_Linq() => _source4.Min();
        [Benchmark] public int N01b_Linq() => _source1024.Min();

        [Benchmark] public int N02a_Linq() => _source4.Max();
        [Benchmark] public int N02b_Linq() => _source1024.Max();

        [Benchmark] public float N03a_Linq() => _floats.Sum();
        [Benchmark] public float N03b_Linq() => _floats.Average();
        [Benchmark] public float N03c_Linq() => _floats.Min();
        [Benchmark] public float N03d_Linq() => _floats.Max();

        [Benchmark]
        public int N04a_Linq()
        {
            int sum = 0;
            foreach (int i in _reversed1024.OrderBy(x => x))
                sum += i;
            return sum;
        }

        [Benchmark]
        public int N04b_Linq()
        {
            int sum = 0;
            foreach (int i in _reversed1024.Order())
                sum += i;
            return sum;
        }
    }
}
