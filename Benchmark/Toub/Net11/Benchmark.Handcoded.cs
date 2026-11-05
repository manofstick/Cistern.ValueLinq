using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Toub
{
    partial class Net11
    {
        [Benchmark]
        public int N01_Handcoded() =>
            _source.Length + 1 > 10
                ? _appended
                : default;

        [Benchmark]
        public int N02_Handcoded()
        {
            var sum = 0;
            foreach (var x in _intData)
                sum = checked(sum + x);
            return sum;
        }

        [Benchmark]
        public byte N03a_Handcoded() =>
            Max(_bytes16);

        [Benchmark]
        public byte N03b_Handcoded() =>
            Max(_bytes64);

        [Benchmark]
        public int N04a_Handcoded()
        {
            var rightCounts = new Dictionary<int, int>();
            foreach (var r in _right)
            {
                rightCounts.TryGetValue(r.Id, out var matches);
                rightCounts[r.Id] = matches + 1;
            }

            var leftKeys = new HashSet<int>();
            var count = 0;
            foreach (var l in _left)
            {
                leftKeys.Add(l.Id);
                count += rightCounts.TryGetValue(l.Id, out var matches) ? matches : 1;
            }

            foreach (var r in _right)
            {
                if (!leftKeys.Contains(r.Id))
                    ++count;
            }

            return count;
        }

        static byte Max(byte[] source)
        {
            var max = source[0];
            for (var i = 1; i < source.Length; ++i)
            {
                if (source[i] > max)
                    max = source[i];
            }
            return max;
        }
    }
}
