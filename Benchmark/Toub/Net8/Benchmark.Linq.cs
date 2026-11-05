using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net8
    {
        IEnumerable<int> _linqReversed;

        partial void BuildLinq()
        {
            _linqReversed = Enumerable.Range(0, 1000).Reverse();
        }

        [Benchmark] public List<int> N01a_Linq() => _source1024.Select(i => i * 2).ToList();
        [Benchmark] public List<byte> N01b_Linq() => Enumerable.Repeat((byte)'a', 1024).ToList();
        [Benchmark] public List<int> N01c_Linq() => Enumerable.Range(0, 1024).Select(i => i * 2).ToList();

        [Benchmark] public List<int> N02_Linq() => Enumerable.Range(0, 16_384).ToList();

        [Benchmark]
        public List<byte> N03_Linq()
        {
            _bytesList.Clear();
            _bytesList.AddRange(Enumerable.Repeat((byte)'a', 1024));
            return _bytesList;
        }

        [Benchmark] public byte N04_Linq() => _bytes4096.Max();

        [Benchmark] public int N05_Linq() => _source1024.Sum();

        [Benchmark]
        public int N06_Linq()
        {
            int sum = 0;
            foreach (int i in _linqReversed.Order())
                sum += i;
            return sum;
        }

        [Benchmark]
        public int N07_Linq()
        {
            int sum = 0;
            foreach (int i in _reversed1000000.OrderBy(i => i * 2))
                sum += i;
            return sum;
        }

        [Benchmark]
        public bool N08_Linq() =>
            _str.Any() &&
            _list3.Any() &&
            _array3.Any();
    }
}
