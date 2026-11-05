using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.Benchmarks.Synthetic
{
    partial class JoinShapes
    {
        [Benchmark, BenchmarkCategory("FullJoinFewKeys")]
        public int Handcoded_FullJoinFewKeys() =>
            FullJoinCount(_fewOuter, _fewInner);

        [Benchmark, BenchmarkCategory("JoinFewKeys")]
        public int Handcoded_JoinFewKeys() =>
            JoinCount(_fewOuter, _fewInner);

        [Benchmark, BenchmarkCategory("FullJoinWide")]
        public int Handcoded_FullJoinWide() =>
            FullJoinCount(_distinctOuter, _wideInner);

        [Benchmark, BenchmarkCategory("JoinWide")]
        public int Handcoded_JoinWide() =>
            JoinCount(_distinctOuter, _wideInner);

        [Benchmark, BenchmarkCategory("GroupJoin")]
        public int Handcoded_GroupJoin() =>
            JoinCount(_distinctOuter, _smallInner);

        internal static Dictionary<int, int> CountByKey<T>(T[] inner)
            where T : IKeyed
        {
            var counts = new Dictionary<int, int>();
            foreach (var element in inner)
                ++CollectionsMarshal.GetValueRefOrAddDefault(counts, element.Key, out _);
            return counts;
        }

        internal static int FullJoinCount<T>(int[] outer, T[] inner)
            where T : IKeyed
        {
            var counts = CountByKey(inner);
            var matched = new HashSet<int>();
            var count = 0;
            foreach (var key in outer)
            {
                if (counts.TryGetValue(key, out var matches))
                {
                    matched.Add(key);
                    count += matches;
                }
                else
                {
                    ++count;
                }
            }

            foreach (var (key, matches) in counts)
            {
                if (!matched.Contains(key))
                    count += matches;
            }

            return count;
        }

        internal static int JoinCount<T>(int[] outer, T[] inner)
            where T : IKeyed
        {
            var counts = CountByKey(inner);
            var count = 0;
            foreach (var key in outer)
            {
                if (counts.TryGetValue(key, out var matches))
                    count += matches;
            }
            return count;
        }
    }
}
