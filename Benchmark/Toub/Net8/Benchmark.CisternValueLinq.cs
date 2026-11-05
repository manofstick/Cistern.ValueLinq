using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net8
    {
        IEnumerable<int> _cisternReversed;

        // the source as its own type, for the Typed rows
        int[] _cisternSource1024;

        partial void BuildCisternValueLinq()
        {
            _cisternReversed = Enumerable.Range(0, 1000).Reverse();

            _cisternSource1024 = (int[])_source1024;
        }

        [Benchmark] public List<int> N01a_CisternValueLinq() => _source1024.Select(i => i * 2).ToList();
        [Benchmark] public List<int> N01a_CisternValueLinqTyped() => _cisternSource1024.Select(i => i * 2).ToList();
        [Benchmark] public List<byte> N01b_CisternValueLinq() => Enumerable.Repeat((byte)'a', 1024).ToList();
        [Benchmark] public List<int> N01c_CisternValueLinq() => Enumerable.Range(0, 1024).Select(i => i * 2).ToList();

        [Benchmark] public List<int> N02_CisternValueLinq() => Enumerable.Range(0, 16_384).ToList();

        [Benchmark]
        public List<byte> N03_CisternValueLinq()
        {
            _bytesList.Clear();
            _bytesList.AddRange(Enumerable.Repeat((byte)'a', 1024));
            return _bytesList;
        }

        // handed out as an ICollection<T>, which AddRange sizes from and copies in
        [Benchmark]
        public List<byte> N03_CisternValueLinqCollection()
        {
            _bytesList.Clear();
            _bytesList.AddRange(Enumerable.Repeat((byte)'a', 1024).ToEnumerable());
            return _bytesList;
        }

        [Benchmark] public byte N04_CisternValueLinq() => _bytes4096.Max();

        [Benchmark] public int N05_CisternValueLinq() => _source1024.Sum();

        [Benchmark] public int N05_CisternValueLinqTyped() => _cisternSource1024.Sum();

        [Benchmark]
        public int N06_CisternValueLinq()
        {
            int sum = 0;
            foreach (int i in _cisternReversed.Order())
                sum += i;
            return sum;
        }

        // built in the method, as ZLinq's is
        [Benchmark]
        public int N06_CisternValueLinqInline()
        {
            int sum = 0;
            foreach (int i in Enumerable.Range(0, 1000).Reverse().Order())
                sum += i;
            return sum;
        }

        [Benchmark]
        public int N07_CisternValueLinq()
        {
            int sum = 0;
            foreach (int i in _reversed1000000.OrderBy(i => i * 2))
                sum += i;
            return sum;
        }

        [Benchmark]
        public bool N08_CisternValueLinq() =>
            _str.Any() &&
            _list3.Any() &&
            _array3.Any();
    }
}
