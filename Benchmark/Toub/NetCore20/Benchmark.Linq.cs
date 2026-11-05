using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    partial class NetCore20
    {
        IEnumerable<int> _linqConcatenated;
        IEnumerable<int> _linqTenMillionToZero;
        IEnumerable<int> _linqHundredMillion;

        partial void BuildLinq()
        {
            IEnumerable<int> zeroToTen = Enumerable.Range(0, 10);
            IEnumerable<int> result = zeroToTen;
            for (int i = 0; i < 10_000; i++)
                result = result.Concat(zeroToTen);
            _linqConcatenated = result;

            _linqTenMillionToZero = Enumerable.Range(0, 10_000_000).Reverse();
            _linqHundredMillion = Enumerable.Range(0, 100_000_000);
        }

        [Benchmark]
        public int N01_Linq()
        {
            int sum = 0;
            foreach (int i in _linqConcatenated)
                sum += i;
            return sum;
        }

        [Benchmark] public int N02_Linq() => _linqTenMillionToZero.OrderBy(i => i).Skip(4).First();

        [Benchmark] public List<int> N03_Linq() => _zeroToTenMillion.Select(i => i).ToList();

        [Benchmark] public int[] N04_Linq() => _linqHundredMillion.ToArray();
    }
}
