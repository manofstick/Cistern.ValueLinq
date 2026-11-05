using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;

namespace Cistern.Benchmarks.Synthetic
{
    // the join family over inner sequences of different shapes, as the inner side is what the join builds: many elements
    // under few keys, wide elements under distinct keys, and GroupJoin, which hands each outer element its matches
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public partial class JoinShapes
    {
        public interface IKeyed
        {
            int Key { get; }
        }

        public readonly struct Small(int key, int value)
            : IKeyed
        {
            public int Key { get; } = key;
            public int Value { get; } = value;
        }

        public readonly struct Wide(int key)
            : IKeyed
        {
            public int Key { get; } = key;
            public long A { get; } = key;
            public long B { get; } = key;
            public long C { get; } = key;
            public long D { get; } = key;
            public long E { get; } = key;
            public long F { get; } = key;
            public long G { get; } = key;
        }

        int[] _fewOuter;
        Small[] _fewInner;
        int[] _distinctOuter;
        Wide[] _wideInner;
        Small[] _smallInner;

        public JoinShapes()
        {
            // 100,000 elements under keys 0 to 9, joined by keys 5 to 14: half the groupings matched, half the outer unmatched
            _fewOuter = Keys(5, 10);
            _fewInner = FewKeys(100_000, 10);

            // 10,000 against 10,000 distinct keys, a quarter of each side unmatched
            _distinctOuter = Keys(0, 10_000);
            _wideInner = new Wide[10_000];
            for (var i = 0; i < _wideInner.Length; ++i)
                _wideInner[i] = new Wide(i + 2_500);
            _smallInner = DistinctKeys(10_000, 2_500);
        }

        internal static int[] Keys(int start, int count)
        {
            var keys = new int[count];
            for (var i = 0; i < count; ++i)
                keys[i] = start + i;
            return keys;
        }

        internal static Small[] FewKeys(int length, int keys)
        {
            var inner = new Small[length];
            for (var i = 0; i < length; ++i)
                inner[i] = new Small(i % keys, i);
            return inner;
        }

        internal static Small[] DistinctKeys(int length, int start)
        {
            var inner = new Small[length];
            for (var i = 0; i < length; ++i)
                inner[i] = new Small(i + start, i);
            return inner;
        }

        internal static void SanityCheck()
        {
            var check = new JoinShapes();

            Same(100_005, check.Handcoded_FullJoinFewKeys(), check.CisternValueLinq_FullJoinFewKeys());
            Same(50_000, check.Handcoded_JoinFewKeys(), check.CisternValueLinq_JoinFewKeys());
            Same(12_500, check.Handcoded_FullJoinWide(), check.CisternValueLinq_FullJoinWide());
            Same(7_500, check.Handcoded_JoinWide(), check.CisternValueLinq_JoinWide());
            Same(7_500, check.Handcoded_GroupJoin(), check.CisternValueLinq_GroupJoin());
        }

        internal static void Same(int expected, int handcoded, int cistern)
        {
            if (handcoded != expected || cistern != expected)
                throw new Exception($"expected {expected}, hand-coded {handcoded}, Cistern {cistern}");
        }
    }
}
