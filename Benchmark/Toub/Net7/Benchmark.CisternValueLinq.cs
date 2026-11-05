using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net7
    {
        // the sources as their own types, for the Typed rows
        int[] _cisternSource4;
        int[] _cisternSource1024;
        float[] _cisternFloats;

        partial void BuildCisternValueLinq()
        {
            _cisternSource4 = (int[])_source4;
            _cisternSource1024 = (int[])_source1024;
            _cisternFloats = (float[])_floats;
        }

        [Benchmark] public int N01a_CisternValueLinq() => _source4.Min();
        [Benchmark] public int N01b_CisternValueLinq() => _source1024.Min();

        [Benchmark] public int N02a_CisternValueLinq() => _source4.Max();
        [Benchmark] public int N02b_CisternValueLinq() => _source1024.Max();

        [Benchmark] public float N03a_CisternValueLinq() => _floats.Sum();
        [Benchmark] public float N03b_CisternValueLinq() => _floats.Average();
        [Benchmark] public float N03c_CisternValueLinq() => _floats.Min();
        [Benchmark] public float N03d_CisternValueLinq() => _floats.Max();

        [Benchmark] public int N01a_CisternValueLinqTyped() => _cisternSource4.Min();
        [Benchmark] public int N01b_CisternValueLinqTyped() => _cisternSource1024.Min();

        [Benchmark] public int N02a_CisternValueLinqTyped() => _cisternSource4.Max();
        [Benchmark] public int N02b_CisternValueLinqTyped() => _cisternSource1024.Max();

        [Benchmark] public float N03a_CisternValueLinqTyped() => _cisternFloats.Sum();
        [Benchmark] public float N03b_CisternValueLinqTyped() => _cisternFloats.Average();
        [Benchmark] public float N03c_CisternValueLinqTyped() => _cisternFloats.Min();
        [Benchmark] public float N03d_CisternValueLinqTyped() => _cisternFloats.Max();

        [Benchmark]
        public int N04a_CisternValueLinq()
        {
            int sum = 0;
            foreach (int i in _reversed1024.OrderBy(x => x))
                sum += i;
            return sum;
        }

        [Benchmark]
        public int N04b_CisternValueLinq()
        {
            int sum = 0;
            foreach (int i in _reversed1024.Order())
                sum += i;
            return sum;
        }
    }
}
