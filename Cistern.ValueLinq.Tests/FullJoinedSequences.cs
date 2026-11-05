using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // FullJoin against System.Linq's: each outer element joined to its matches or to nothing, then the inner groupings
    // no outer element matched, in the lookup's order (first key seen first), the elements with a null key among them.
    // Inner is read before outer, and a side that is an empty array runs neither key selector.
    public class FullJoinedSequences
    {
        private static readonly IEqualityComparer<int> ByTens = EqualityComparer<int>.Create((x, y) => x % 10 == y % 10, x => x % 10);

        private static int[] Scrambled(int count, int seed)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = (i * 37 + seed) % 101;

            return items;
        }

        private static IEnumerable<T> Iterate<T>(T[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        private static List<T> Pull<T>(IEnumerable<T> source)
        {
            var items = new List<T>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        private static (int, int) Pair(int outer, int inner) => (outer, inner);

        public static IEnumerable<object[]> Lengths() => [[0, 0], [0, 5], [5, 0], [1, 1], [7, 40], [40, 7], [101, 101]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryOuterShape_JoinsAsSystemLinq(int outerLength, int innerLength)
        {
            var outer = Scrambled(outerLength, 11);
            var inner = Scrambled(innerLength, 5);
            var expected = System.Linq.Enumerable.ToList(System.Linq.Enumerable.FullJoin(outer, Iterate(inner), x => x % 23, x => x % 17, Pair));

            Assert.Equal(expected, outer.FullJoin(Iterate(inner), x => x % 23, x => x % 17, Pair).ToList());
            Assert.Equal(expected, Pull(outer.FullJoin(Iterate(inner), x => x % 23, x => x % 17, Pair)));
            Assert.Equal(expected, new List<int>(outer).FullJoin(inner, x => x % 23, x => x % 17, Pair).ToList());
            Assert.Equal(expected, Iterate(outer).FullJoin(inner, x => x % 23, x => x % 17, Pair).ToArray());
            Assert.Equal(expected, Pull(Iterate(outer).FullJoin(inner, x => x % 23, x => x % 17, Pair)));
            Assert.Equal(expected, outer.Select(x => x).FullJoin(inner, x => x % 23, x => x % 17, Pair).ToList());
            Assert.Equal(expected, Pull(outer.Select(x => x).FullJoin(inner, x => x % 23, x => x % 17, Pair)));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void TheTupleOverload_AndAComparer_JoinAsSystemLinq(int outerLength, int innerLength)
        {
            var outer = Scrambled(outerLength, 11);
            var inner = Scrambled(innerLength, 5);

            var expectedPairs = System.Linq.Enumerable.ToList(System.Linq.Enumerable.FullJoin(outer, inner, x => x % 23, x => x % 17));
            Assert.Equal(expectedPairs, outer.FullJoin(inner, x => x % 23, x => x % 17).ToList());
            Assert.Equal(expectedPairs, Pull(outer.FullJoin(inner, x => x % 23, x => x % 17)));

            var expectedByTens = System.Linq.Enumerable.ToList(System.Linq.Enumerable.FullJoin(outer, inner, x => x, x => x, Pair, ByTens));
            Assert.Equal(expectedByTens, outer.FullJoin(inner, x => x, x => x, Pair, ByTens).ToList());
            Assert.Equal(expectedByTens, Pull(outer.FullJoin(inner, x => x, x => x, Pair, ByTens)));
        }

        [Fact]
        public void NullKeys_MatchNothing_AndTheInnerOnesFollowUnmatched()
        {
            string?[] outer = ["a", null, "b", "c", null];
            string?[] inner = [null, "c", "d", null, "a", "d"];
            var expected = System.Linq.Enumerable.ToList(System.Linq.Enumerable.FullJoin(outer, inner, x => x, x => x, (o, i) => $"{o}-{i}"));

            Assert.Equal(expected, outer.FullJoin(inner, x => x, x => x, (o, i) => $"{o}-{i}").ToList());
            Assert.Equal(expected, Pull(outer.FullJoin(inner, x => x, x => x, (o, i) => $"{o}-{i}")));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(9)]
        [InlineData(20)]
        public void AHaltedDownstream_IsGivenNoUnmatchedInner(int take)
        {
            int[] outer = [1, 2, 3, 4];
            int[] inner = [3, 3, 5, 6, 1, 7, 8];
            var expected = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Take(System.Linq.Enumerable.FullJoin(outer, inner, x => x, x => x, Pair), take));

            Assert.Equal(expected, outer.FullJoin(inner, x => x, x => x, Pair).Take(take).ToList());
            Assert.Equal(expected, Pull(outer.FullJoin(inner, x => x, x => x, Pair).Take(take)));
            Assert.Equal(System.Linq.Enumerable.Last(System.Linq.Enumerable.FullJoin(System.Linq.Enumerable.Append(outer, 8), inner, x => x, x => x, Pair)), outer.Append(8).FullJoin(inner, x => x, x => x, Pair).Last());
        }

        [Fact]
        public void Inner_IsReadBeforeOuter()
        {
            var log = new List<string>();
            IEnumerable<int> Logged(string name, int[] items)
            {
                foreach (var item in items)
                {
                    log.Add(name);
                    yield return item;
                }
            }

            _ = Logged("outer", [1, 2]).FullJoin(Logged("inner", [2, 3]), x => x, x => x, Pair).ToList();
            Assert.Equal(["inner", "inner", "outer", "outer"], log);

            log.Clear();
            _ = Pull(Logged("outer", [1, 2]).FullJoin(Logged("inner", [2, 3]), x => x, x => x, Pair));
            Assert.Equal(["inner", "inner", "outer", "outer"], log);
        }

        [Fact]
        public void AnEmptyArraySide_RunsNoKeySelector()
        {
            var calls = 0;
            int Key(int x)
            {
                ++calls;
                return x;
            }

            Assert.Equal([(0, 4), (0, 5)], new int[0].FullJoin(new[] { 4, 5 }, Key, Key, Pair).ToList());
            Assert.Equal([(0, 4), (0, 5)], Pull(new int[0].FullJoin(new[] { 4, 5 }, Key, Key, Pair)));
            Assert.Equal([(4, 0), (5, 0)], new[] { 4, 5 }.FullJoin(new int[0], Key, Key, Pair).ToList());
            Assert.Equal([(4, 0), (5, 0)], Pull(new[] { 4, 5 }.FullJoin(new int[0], Key, Key, Pair)));
            Assert.Equal(0, calls);
        }

        [Fact]
        public void NullArguments_Throw()
        {
            int[] items = [1];

            Assert.Throws<ArgumentNullException>("outer", () => ((IEnumerable<int>)null!).FullJoin(items, x => x, x => x, Pair));
            Assert.Throws<ArgumentNullException>("inner", () => items.FullJoin((IEnumerable<int>)null!, x => x, x => x, Pair));
            Assert.Throws<ArgumentNullException>("outerKeySelector", () => items.FullJoin(items, (Func<int, int>)null!, x => x, Pair));
            Assert.Throws<ArgumentNullException>("innerKeySelector", () => items.FullJoin(items, x => x, (Func<int, int>)null!, Pair));
            Assert.Throws<ArgumentNullException>("resultSelector", () => items.FullJoin(items, x => x, x => x, (Func<int, int, int>)null!));
        }
    }
}
