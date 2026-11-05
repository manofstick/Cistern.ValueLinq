using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net8
    {
        [Benchmark] public List<int> N01a_ZLinq() => _source1024.AsValueEnumerable().Select(i => i * 2).ToList();
        [Benchmark] public List<byte> N01b_ZLinq() => ValueEnumerable.Repeat((byte)'a', 1024).ToList();
        [Benchmark] public List<int> N01c_ZLinq() => ValueEnumerable.Range(0, 1024).Select(i => i * 2).ToList();

        [Benchmark] public List<int> N02_ZLinq() => ValueEnumerable.Range(0, 16_384).ToList();

        // a ZLinq sequence has no IEnumerable<T> form to hand AddRange, so its own CopyTo operator fills the list
        [Benchmark]
        public List<byte> N03_ZLinq()
        {
            _bytesList.Clear();
            ValueEnumerable
            .Repeat((byte)'a', 1024)
            .CopyTo(_bytesList);
            return _bytesList;
        }

        [Benchmark] public byte N04_ZLinq() => _bytes4096.AsValueEnumerable().Max();

        [Benchmark] public int N05_ZLinq() => _source1024.AsValueEnumerable().Sum();

        [Benchmark]
        public int N06_ZLinq()
        {
            int sum = 0;
            foreach (int i in ValueEnumerable.Range(0, 1000).Reverse().Order())
                sum += i;
            return sum;
        }

        [Benchmark]
        public int N07_ZLinq()
        {
            int sum = 0;
            foreach (int i in _reversed1000000.AsValueEnumerable().OrderBy(i => i * 2))
                sum += i;
            return sum;
        }

        [Benchmark]
        public bool N08_ZLinq() =>
            _str.AsValueEnumerable().Any() &&
            _list3.AsValueEnumerable().Any() &&
            _array3.AsValueEnumerable().Any();
    }
}
