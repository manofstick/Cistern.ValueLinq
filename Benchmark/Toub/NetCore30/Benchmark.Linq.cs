using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    partial class NetCore30
    {
        IEnumerable<int> _linqEnumerable;

        partial void BuildLinq()
        {
            _linqEnumerable = _zeros.Select(i => i);
        }

        [Benchmark] public int N01_Linq() => _linqEnumerable.TakeLast(10).Sum();

        [Benchmark] public int[] N02_Linq() => Enumerable.Range(0, 100).Select(i => i * 2).ToArray();

        [Benchmark] public int[] N03_Linq() => Enumerable.Empty<int>().Take(10).Select(i => i).ToArray();
    }
}
