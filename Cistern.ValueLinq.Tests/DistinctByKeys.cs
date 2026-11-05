using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class DistinctByKeys
    {
        private static readonly IEqualityComparer<int> ByTens = EqualityComparer<int>.Create((x, y) => x % 10 == y % 10, x => x % 10);

        private static int[] Scrambled(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = (i * 37 + 11) % 101;

            return items;
        }

        private static IEnumerable<T> Iterate<T>(T[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        private static int KeyOf(int x) => x % 23;

        private static List<T> Pull<T>(IEnumerable<T> source)
        {
            var items = new List<T>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        private static List<T> Expected<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var seen = new HashSet<TKey>(comparer);
            var expected = new List<T>();
            foreach (var item in source)
            {
                if (seen.Add(keySelector(item)))
                    expected.Add(item);
            }

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeKeepsTheFirstElementOfEachKey(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);

            foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByTens })
            {
                var expected = Expected(items, KeyOf, comparer);

                Assert.Equal(expected, items.DistinctBy(KeyOf, comparer).ToArray());
                Assert.Equal(expected, list.DistinctBy(KeyOf, comparer).ToArray());
                Assert.Equal(expected, memory.DistinctBy(KeyOf, comparer).ToArray());
                Assert.Equal(expected, enumerable.DistinctBy(KeyOf, comparer).ToArray());
                Assert.Equal(expected, ((IEnumerable<int>)items).DistinctBy(KeyOf, comparer).ToArray());
                Assert.Equal(expected, ((IEnumerable<int>)list).DistinctBy(KeyOf, comparer).ToArray());
                Assert.Equal(expected, items.Select(x => x).DistinctBy(KeyOf, comparer).ToArray());

                Assert.Equal(expected, Pull(items.DistinctBy(KeyOf, comparer)));
                Assert.Equal(expected, Pull(list.DistinctBy(KeyOf, comparer)));
                Assert.Equal(expected, Pull(memory.DistinctBy(KeyOf, comparer)));
                Assert.Equal(expected, Pull(enumerable.DistinctBy(KeyOf, comparer)));
                Assert.Equal(expected, Pull(items.Select(x => x).DistinctBy(KeyOf, comparer)));
            }

            var withoutComparer = Expected(items, KeyOf, null);

            Assert.Equal(withoutComparer, items.DistinctBy(KeyOf).ToArray());
            Assert.Equal(withoutComparer, list.DistinctBy(KeyOf).ToArray());
            Assert.Equal(withoutComparer, memory.DistinctBy(KeyOf).ToArray());
            Assert.Equal(withoutComparer, enumerable.DistinctBy(KeyOf).ToArray());
            Assert.Equal(withoutComparer, items.Select(x => x).DistinctBy(KeyOf).ToArray());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).DistinctBy(KeyOf);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).DistinctBy(KeyOf);
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).DistinctBy(x => x % 31, ByTens);

                var pulled = Pull(filtered);

                var handCoded = new List<int>();
                var seen = new HashSet<int>();
                foreach (var item in items)
                {
                    if (item > keepAbove
                        && seen.Add(KeyOf(item)))
                    {
                        handCoded.Add(item);
                    }
                }

                var sum = 0;
                foreach (var item in handCoded)
                    sum += item;

                Assert.Equal(handCoded, pulled);
                Assert.Equal(pulled, filtered.ToList());
                Assert.Equal(pulled, filtered.ToArray());
                Assert.Equal(sum, filtered.Sum());
                Assert.Equal(pulled.Count, filtered.Count());
                Assert.Equal(pulled, filtered.Select(x => x).ToList());
                Assert.Equal(Pull(iterated), iterated.ToList());
                Assert.Equal(Pull(iterated), iterated.Select(x => x).ToArray());
                Assert.Equal(Pull(concatenated), concatenated.ToList());
                Assert.Equal(Pull(concatenated), concatenated.Select(x => x).ToList());
            }

            Assert.Equal(Pull(Enumerable.Range(3, length).DistinctBy(KeyOf)), Enumerable.Range(3, length).DistinctBy(KeyOf).ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            var expected = Expected(items, KeyOf, null).ToArray();

            var reversed = (int[])expected.Clone();
            Array.Reverse(reversed);

            IEnumerable<int>[] sequences =
            [
                items.DistinctBy(KeyOf),
                new List<int>(items).DistinctBy(KeyOf),
                Iterate(items).DistinctBy(KeyOf),
                items.Select(x => x).DistinctBy(KeyOf),
            ];

            foreach (var sequence in sequences)
            {
                Assert.Equal(expected.Length, sequence.Count());
                Assert.Equal(expected.Length, sequence.LongCount());
                Assert.Equal(expected, sequence.ToArray());
                Assert.Equal(expected, sequence.ToList());
                Assert.Equal(reversed, sequence.Reverse().ToArray());
                Assert.Equal(0, sequence.ElementAtOrDefault(expected.Length));

                for (var i = 0; i < expected.Length; i += Math.Max(1, expected.Length / 7))
                {
                    Assert.Equal(expected[i], sequence.ElementAt(i));
                    Assert.Equal(expected[^(i + 1)], sequence.ElementAt(^(i + 1)));
                }

                foreach (var count in new[] { 0, 1, 2, expected.Length, expected.Length + 1 })
                {
                    var skipped = expected[Math.Min(count, expected.Length)..];
                    var taken = expected[..Math.Min(count, expected.Length)];

                    Assert.Equal(skipped, sequence.Skip(count).ToArray());
                    Assert.Equal(taken, sequence.Take(count).ToArray());
                }

                if (expected is [var first, ..])
                {
                    Assert.Equal(first, sequence.First());
                    Assert.Equal(expected[^1], sequence.Last());
                }
            }

            Assert.False(Iterate(items).DistinctBy(KeyOf).TryGetNonEnumeratedCount(out _));
        }

        [Fact]
        public void TheKeySelectorRunsOncePerElementInOrderEachTimeTheSequenceIsRun()
        {
            var items = Scrambled(50);
            var seen = new List<int>();
            var sequence = items.DistinctBy(logged);

            Assert.Equal(Expected(items, KeyOf, null), sequence.ToList());
            Assert.Equal(items, seen);

            seen.Clear();

            Assert.Equal(Expected(items, KeyOf, null), Pull(sequence));
            Assert.Equal(items, seen);

            seen.Clear();

            Assert.Equal(Expected(items, KeyOf, null), sequence.ToList());
            Assert.Equal(items, seen);

            int logged(int x)
            {
                seen.Add(x);

                return KeyOf(x);
            }
        }

        [Fact]
        public void TheKeySelectorRunsOnlyAsFarAsTheConsumerReads()
        {
            var items = new[] { 1, 24, 2, 3, 25, 4 };
            var seen = new List<int>();

            Assert.Equal(1, items.DistinctBy(logged).First());
            Assert.Equal([1], seen);

            seen.Clear();

            Assert.Equal([1, 2], items.DistinctBy(logged).Take(2).ToArray());
            Assert.Equal([1, 24, 2], seen);

            seen.Clear();

            Assert.Equal([1, 2], Iterate(items).DistinctBy(logged).Take(2).ToList());
            Assert.Equal([1, 24, 2], seen);

            seen.Clear();

            Assert.Equal(2, items.Select(x => x).DistinctBy(logged).First(x => x > 1));
            Assert.Equal([1, 24, 2], seen);

            int logged(int x)
            {
                seen.Add(x);

                return KeyOf(x);
            }
        }

        [Fact]
        public void DistinctAndExceptStopWhereTheConsumerStops()
        {
            var items = new[] { 1, 1, 2, 3, 2, 4 };
            var list = new List<int>(items);

            Assert.Equal(1, items.Distinct().First());
            Assert.Equal(1, Iterate(items).Distinct().First());
            Assert.Equal(1, items.Select(x => x).Distinct().First());
            Assert.Equal([1, 2], list.Distinct().Take(2).ToArray());
            Assert.Equal(2, items.Except([1]).First());
            Assert.Equal(2, Iterate(items).Except([1, 3]).First());
            Assert.Equal([2, 4], items.Select(x => x).Except([1, 3]).Take(2).ToList());
        }

        [Fact]
        public void TheComparerDecidesWhichKeysAreEqual()
        {
            string[] words = ["apple", "Avocado", "banana", "Blueberry", "cherry", "APRICOT", "Cherry"];

            Assert.Equal(["apple", "Avocado", "banana", "Blueberry", "cherry", "Cherry"], words.DistinctBy(x => x[..1]).ToArray());
            Assert.Equal(["apple", "banana", "cherry"], words.DistinctBy(x => x[..1], StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(["apple", "banana", "cherry"], Pull(Iterate(words).DistinctBy(x => x[..1], StringComparer.OrdinalIgnoreCase)));
        }

        [Fact]
        public void ANullKeyIsAKeyLikeAnyOther()
        {
            string[] words = ["a", null, "b", null, "a", "c"];

            Assert.Equal(["a", null, "b", "c"], words.DistinctBy(x => x).ToArray());
            Assert.Equal(["a", null, "b", "c"], Pull(Iterate(words).DistinctBy(x => x)));
            Assert.Equal(["a", null], words.DistinctBy(x => x is null).ToArray());
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMade()
        {
            Func<int, int> nullSelector = null;

            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).DistinctBy(KeyOf));
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).DistinctBy(KeyOf, ByTens));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null).DistinctBy(KeyOf));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).DistinctBy(KeyOf));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).DistinctBy(nullSelector));

            Assert.Throws<ArgumentNullException>("keySelector", () => new[] { 1 }.DistinctBy(nullSelector));
            Assert.Throws<ArgumentNullException>("keySelector", () => new List<int> { 1 }.DistinctBy(nullSelector, ByTens));
            Assert.Throws<ArgumentNullException>("keySelector", () => ((ReadOnlyMemory<int>)new[] { 1 }).DistinctBy(nullSelector));
            Assert.Throws<ArgumentNullException>("keySelector", () => Iterate(new[] { 1 }).DistinctBy(nullSelector));
            Assert.Throws<ArgumentNullException>("keySelector", () => new[] { 1 }.Select(x => x).DistinctBy(nullSelector));
        }

        private sealed class Tracked
        {
            public int Disposed;

            public IEnumerable<int> Iterate(int count)
            {
                try
                {
                    for (var i = 0; i < count; ++i)
                        yield return i;
                }
                finally
                {
                    ++Disposed;
                }
            }
        }

        [Fact]
        public void TheSourceIsDisposedWhenTheOperatorStopsEarly()
        {
            var tracked = new Tracked();

            foreach (var _ in tracked.Iterate(10).DistinctBy(KeyOf))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal(0, tracked.Iterate(10).DistinctBy(KeyOf).First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([0, 1], tracked.Iterate(10).DistinctBy(KeyOf).Take(2).ToList());
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).DistinctBy(x => x < 5 ? x : throw new InvalidOperationException()).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(tracked.Iterate(10).DistinctBy(x => x < 5 ? x : throw new InvalidOperationException())));
            Assert.Equal(5, tracked.Disposed);
        }
    }
}
