using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class ExceptByKeys
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

        private static List<T> Expected<T, TKey>(IEnumerable<T> source, IEnumerable<TKey> second, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var seen = new HashSet<TKey>(second, comparer);
            var expected = new List<T>();
            foreach (var item in source)
            {
                if (seen.Add(keySelector(item)))
                    expected.Add(item);
            }

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        private static int[][] Seconds() => [[], [0], [3, 5, 3, 18], Scrambled(40), [100, 200]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeKeepsTheFirstElementOfEachKeyNotInSecond(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);

            foreach (var second in Seconds())
            {
                foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByTens })
                {
                    var expected = Expected(items, second, KeyOf, comparer);

                    Assert.Equal(expected, items.ExceptBy(second, KeyOf, comparer).ToArray());
                    Assert.Equal(expected, list.ExceptBy(second, KeyOf, comparer).ToArray());
                    Assert.Equal(expected, memory.OfMemory().ExceptBy(second, KeyOf, comparer).ToArray());
                    Assert.Equal(expected, enumerable.ExceptBy(second, KeyOf, comparer).ToArray());
                    Assert.Equal(expected, items.ExceptBy(Iterate(second), KeyOf, comparer).ToArray());
                    Assert.Equal(expected, items.Select(x => x).ExceptBy(second, KeyOf, comparer).ToArray());

                    Assert.Equal(expected, Pull(items.ExceptBy(second, KeyOf, comparer)));
                    Assert.Equal(expected, Pull(list.ExceptBy(second, KeyOf, comparer)));
                    Assert.Equal(expected, Pull(memory.OfMemory().ExceptBy(second, KeyOf, comparer)));
                    Assert.Equal(expected, Pull(enumerable.ExceptBy(Iterate(second), KeyOf, comparer)));
                    Assert.Equal(expected, Pull(items.Select(x => x).ExceptBy(second, KeyOf, comparer)));
                }

                var withoutComparer = Expected(items, second, KeyOf, null);

                Assert.Equal(withoutComparer, items.ExceptBy(second, KeyOf).ToArray());
                Assert.Equal(withoutComparer, list.ExceptBy(second, KeyOf).ToArray());
                Assert.Equal(withoutComparer, enumerable.ExceptBy(second, KeyOf).ToArray());
                Assert.Equal(withoutComparer, items.Select(x => x).ExceptBy(second, KeyOf).ToArray());
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);
            int[] second = [2, 7, 7, 30];

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).ExceptBy(second, KeyOf);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).ExceptBy(Iterate(second), KeyOf);
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).ExceptBy(second, x => x % 31, ByTens);

                var pulled = Pull(filtered);

                var handCoded = new List<int>();
                var seen = new HashSet<int>(second);
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

            Assert.Equal(Pull(Enumerable.Range(3, length).ExceptBy(second, KeyOf)), Enumerable.Range(3, length).ExceptBy(second, KeyOf).ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            int[] second = [1, 4, 9, 16];
            var expected = Expected(items, second, KeyOf, null).ToArray();

            var reversed = (int[])expected.Clone();
            Array.Reverse(reversed);

            IEnumerable<int>[] sequences =
            [
                items.ExceptBy(second, KeyOf),
                new List<int>(items).ExceptBy(second, KeyOf),
                Iterate(items).ExceptBy(Iterate(second), KeyOf),
                items.Select(x => x).ExceptBy(second, KeyOf),
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

            Assert.False(Iterate(items).ExceptBy(second, KeyOf).TryGetNonEnumeratedCount(out _));
        }

        private sealed class Logged<T>(string name, T[] items, List<string> events)
            : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                events.Add($"{name}.GetEnumerator");

                var enumerator = Iterate();

                return enumerator;
            }

            private IEnumerator<T> Iterate()
            {
                try
                {
                    foreach (var item in items)
                    {
                        events.Add($"{name}.{item}");
                        yield return item;
                    }

                    events.Add($"{name}.end");
                }
                finally
                {
                    events.Add($"{name}.Dispose");
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Fact]
        public void SecondIsReadWholeBeforeTheSourceIsPushed()
        {
            var events = new List<string>();
            var first = new Logged<int>("first", [1, 2, 3], events);
            var second = new Logged<int>("second", [2], events);

            var sequence = first.ExceptBy(second, x => x);

            Assert.Empty(events);

            Assert.Equal([1, 3], sequence.ToList());
            Assert.Equal(["second.GetEnumerator", "second.2", "second.end", "second.Dispose", "first.GetEnumerator", "first.1", "first.2", "first.3", "first.end", "first.Dispose"], events);

            events.Clear();

            Assert.Equal([1, 3], sequence.Select(x => x).ToArray());
            Assert.Equal(["second.GetEnumerator", "second.2", "second.end", "second.Dispose", "first.GetEnumerator", "first.1", "first.2", "first.3", "first.end", "first.Dispose"], events);
        }

        [Fact]
        public void SecondIsReadWholeOnTheFirstMoveNextBeforeTheSourceIsPulled()
        {
            var events = new List<string>();
            var first = new Logged<int>("first", [1, 2, 3], events);
            var second = new Logged<int>("second", [2], events);

            var sequence = first.ExceptBy(second, x => x);

            using (var enumerator = sequence.GetEnumerator())
            {
                Assert.DoesNotContain("second.GetEnumerator", events);

                Assert.True(enumerator.MoveNext());
                Assert.Equal(1, enumerator.Current);

                var secondRead = events.IndexOf("second.Dispose");
                var firstPulled = events.IndexOf("first.1");

                Assert.Equal(["second.GetEnumerator", "second.2", "second.end", "second.Dispose"], events.FindAll(e => e.StartsWith("second")));
                Assert.True(secondRead < firstPulled);

                Assert.True(enumerator.MoveNext());
                Assert.Equal(3, enumerator.Current);
                Assert.False(enumerator.MoveNext());
            }

            Assert.Single(events, "second.GetEnumerator");

            events.Clear();

            using (sequence.GetEnumerator())
            {
            }

            Assert.DoesNotContain("second.GetEnumerator", events);
        }

        [Fact]
        public void SecondIsReadAgainEachTimeTheSequenceIsRun()
        {
            var keys = new List<int> { 1 };
            int[] items = [1, 2, 3];
            var sequence = items.ExceptBy(keys, x => x);

            Assert.Equal([2, 3], sequence.ToArray());

            keys.Add(3);

            Assert.Equal([2], sequence.ToArray());
            Assert.Equal([2], Pull(sequence));

            keys.Clear();

            Assert.Equal([1, 2, 3], Pull(sequence));
            Assert.Equal([1, 2, 3], sequence.Select(x => x).ToList());
        }

        [Fact]
        public void TheKeySelectorRunsOncePerElementInOrderEachTimeTheSequenceIsRun()
        {
            var items = Scrambled(50);
            int[] second = [3, 7];
            var seen = new List<int>();
            var sequence = items.ExceptBy(second, logged);

            Assert.Equal(Expected(items, second, KeyOf, null), sequence.ToList());
            Assert.Equal(items, seen);

            seen.Clear();

            Assert.Equal(Expected(items, second, KeyOf, null), Pull(sequence));
            Assert.Equal(items, seen);

            seen.Clear();

            Assert.Equal(Expected(items, second, KeyOf, null), sequence.ToList());
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
            int[] items = [24, 1, 2, 25, 3, 4];
            int[] second = [1];
            var seen = new List<int>();

            Assert.Equal(2, items.ExceptBy(second, logged).First());
            Assert.Equal([24, 1, 2], seen);

            seen.Clear();

            Assert.Equal([2, 3], items.ExceptBy(second, logged).Take(2).ToArray());
            Assert.Equal([24, 1, 2, 25, 3], seen);

            seen.Clear();

            Assert.Equal([2, 3], Iterate(items).ExceptBy(second, logged).Take(2).ToList());
            Assert.Equal([24, 1, 2, 25, 3], seen);

            seen.Clear();

            Assert.Equal(3, items.Select(x => x).ExceptBy(second, logged).First(x => x > 2));
            Assert.Equal([24, 1, 2, 25, 3], seen);

            int logged(int x)
            {
                seen.Add(x);

                return KeyOf(x);
            }
        }

        [Fact]
        public void TheComparerAppliesToTheKeysOfSecond()
        {
            string[] words = ["apple", "Avocado", "banana", "Blueberry", "cherry", "APRICOT", "Cherry"];
            string[] second = ["B", "c"];

            Assert.Equal(["apple", "Avocado", "banana", "Cherry"], words.ExceptBy(second, x => x[..1]).ToArray());
            Assert.Equal(["apple"], words.ExceptBy(second, x => x[..1], StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(["apple"], Pull(Iterate(words).ExceptBy(Iterate(second), x => x[..1], StringComparer.OrdinalIgnoreCase)));
        }

        [Fact]
        public void ANullKeyIsAKeyLikeAnyOther()
        {
            string[] words = ["a", null, "b", null, "a", "c"];

            Assert.Equal(["a", "b", "c"], words.ExceptBy([null], x => x).ToArray());
            Assert.Equal(["a", "b", "c"], Pull(Iterate(words).ExceptBy([null], x => x)));
            Assert.Equal(["a", null, "c"], words.ExceptBy(["b"], x => x).ToArray());
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMadeInSystemLinqsOrder()
        {
            Func<int, int> nullSelector = null;
            IEnumerable<int> nullSecond = null;
            int[] second = [1];

            Assert.Throws<ArgumentNullException>("first", () => ((int[])null).ExceptBy(nullSecond, nullSelector));
            Assert.Throws<ArgumentNullException>("first", () => ((List<int>)null).ExceptBy(second, KeyOf));
            Assert.Throws<ArgumentNullException>("first", () => ((IEnumerable<int>)null).ExceptBy(second, KeyOf, ByTens));

            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.ExceptBy(nullSecond, nullSelector));
            Assert.Throws<ArgumentNullException>("second", () => Iterate(new[] { 1 }).ExceptBy(nullSecond, KeyOf, ByTens));
            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.Select(x => x).ExceptBy(nullSecond, nullSelector));

            Assert.Throws<ArgumentNullException>("keySelector", () => new[] { 1 }.ExceptBy(second, nullSelector));
            Assert.Throws<ArgumentNullException>("keySelector", () => new List<int> { 1 }.ExceptBy(second, nullSelector, ByTens));
            Assert.Throws<ArgumentNullException>("keySelector", () => ((ReadOnlyMemory<int>)new[] { 1 }).OfMemory().ExceptBy(second, nullSelector));
            Assert.Throws<ArgumentNullException>("keySelector", () => new[] { 1 }.Select(x => x).ExceptBy(second, nullSelector));
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
            int[] second = [0];

            foreach (var _ in tracked.Iterate(10).ExceptBy(second, KeyOf))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal(1, tracked.Iterate(10).ExceptBy(second, KeyOf).First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([1, 2], tracked.Iterate(10).ExceptBy(second, KeyOf).Take(2).ToList());
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).ExceptBy(second, x => x < 5 ? x : throw new InvalidOperationException()).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(tracked.Iterate(10).ExceptBy(second, x => x < 5 ? x : throw new InvalidOperationException())));
            Assert.Equal(5, tracked.Disposed);
        }

        [Fact]
        public void SecondIsDisposedOnceRead()
        {
            var tracked = new Tracked();
            int[] items = [5, 6, 7];

            Assert.Equal([5, 6, 7], items.ExceptBy(tracked.Iterate(3), x => x).ToArray());
            Assert.Equal(1, tracked.Disposed);

            Assert.Equal([5, 6, 7], Pull(items.ExceptBy(tracked.Iterate(3), x => x)));
            Assert.Equal(2, tracked.Disposed);
        }
    }
}
