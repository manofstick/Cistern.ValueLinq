using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class NetCore20
    {
        IEnumerable<int> _cisternConcatenated;
        IEnumerable<int> _cisternTenMillionToZero;
        IEnumerable<int> _cisternHundredMillion;

        // the source as its own type, for the Typed row
        int[] _cisternZeroToTenMillion;

        partial void BuildCisternValueLinq()
        {
            _cisternZeroToTenMillion = (int[])_zeroToTenMillion;

            IEnumerable<int> zeroToTen = Enumerable.Range(0, 10);
            IEnumerable<int> result = zeroToTen;
            for (int i = 0; i < 10_000; i++)
                result = result.Concat(zeroToTen);
            _cisternConcatenated = result;

            _cisternTenMillionToZero = Enumerable.Range(0, 10_000_000).Reverse();
            _cisternHundredMillion = Enumerable.Range(0, 100_000_000);
        }

        [Benchmark]
        public int N01_CisternValueLinq()
        {
            int sum = 0;
            foreach (int i in _cisternConcatenated)
                sum += i;
            return sum;
        }

        [Benchmark] public int N02_CisternValueLinq() => _cisternTenMillionToZero.OrderBy(i => i).Skip(4).First();

        // built in the method, as ZLinq's is
        [Benchmark] public int N02_CisternValueLinqInline() => Enumerable.Range(0, 10_000_000).Reverse().OrderBy(i => i).Skip(4).First();

        [Benchmark] public List<int> N03_CisternValueLinq() => _zeroToTenMillion.Select(i => i).ToList();

        [Benchmark] public List<int> N03_CisternValueLinqTyped() => _cisternZeroToTenMillion.Select(i => i).ToList();

        [Benchmark] public int[] N04_CisternValueLinq() => _cisternHundredMillion.ToArray();

        // built in the method, as ZLinq's is
        [Benchmark] public int[] N04_CisternValueLinqInline() => Enumerable.Range(0, 100_000_000).ToArray();
    }
}
