using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class NetCore20
    {
        [Benchmark] public int N02_ZLinq() => ValueEnumerable.Range(0, 10_000_000).Reverse().OrderBy(i => i).Skip(4).First();

        [Benchmark] public List<int> N03_ZLinq() => _zeroToTenMillion.AsValueEnumerable().Select(i => i).ToList();

        [Benchmark] public int[] N04_ZLinq() => ValueEnumerable.Range(0, 100_000_000).ToArray();
    }
}
