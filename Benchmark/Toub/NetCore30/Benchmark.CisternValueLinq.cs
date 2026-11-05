using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class NetCore30
    {
        IEnumerable<int> _cisternEnumerable;

        partial void BuildCisternValueLinq()
        {
            _cisternEnumerable = _zeros.Select(i => i);
        }

        [Benchmark] public int N01_CisternValueLinq() => _cisternEnumerable.TakeLast(10).Sum();

        // built in the method, as ZLinq's is
        [Benchmark] public int N01_CisternValueLinqInline() => _zeros.Select(i => i).TakeLast(10).Sum();

        [Benchmark] public int[] N02_CisternValueLinq() => Enumerable.Range(0, 100).Select(i => i * 2).ToArray();

        [Benchmark] public int[] N03_CisternValueLinq() => Enumerable.Empty<int>().Take(10).Select(i => i).ToArray();
    }
}
