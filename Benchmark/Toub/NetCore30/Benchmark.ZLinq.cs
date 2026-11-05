using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class NetCore30
    {
        [Benchmark] public int N01_ZLinq() => _zeros.AsValueEnumerable().Select(i => i).TakeLast(10).Sum();

        [Benchmark] public int[] N02_ZLinq() => ValueEnumerable.Range(0, 100).Select(i => i * 2).ToArray();

        [Benchmark] public int[] N03_ZLinq() => ValueEnumerable.Empty<int>().Take(10).Select(i => i).ToArray();
    }
}
