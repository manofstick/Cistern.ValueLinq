using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net5
    {
        // the source as its own type, for the Typed row
        List<int> _cisternData;

        partial void BuildCisternValueLinq()
        {
            _cisternData = (List<int>)_data;
        }

        [Benchmark]
        public int N01_CisternValueLinq()
        {
            int sum = 0;
            foreach (int i in _array.OrderBy(i => i))
                sum += i;
            return sum;
        }

        [Benchmark] public int N02_CisternValueLinq() => _data.SkipLast(5).Sum();

        [Benchmark] public int N02_CisternValueLinqTyped() => _cisternData.SkipLast(5).Sum();
    }
}
