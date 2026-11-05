using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class UnionByKeys
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

        private static List<T> Expected<T, TKey>(IEnumerable<T> first, IEnumerable<T> second, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var seen = new HashSet<TKey>(comparer);
            var expected = new List<T>();
            foreach (var item in first)
            {
                if (seen.Add(keySelector(item)))
                    expected.Add(item);
            }

            foreach (var item in second)
            {
                if (seen.Add(keySelector(item)))
                    expected.Add(item);
            }

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        private static int[][] Seconds() => [[], [0], [3, 5, 3, 18, 49], Scrambled(40), [100, 200]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeYieldsTheFirstElementOfEachKey(int length)
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

                    Assert.Equal(expected, items.UnionBy(second, KeyOf, comparer).ToArray());
                    Assert.Equal(expected, list.UnionBy(second, KeyOf, comparer).ToArray());
                    Assert.Equal(expected, memory.OfMemory().UnionBy(second, KeyOf, comparer).ToArray());
                    Assert.Equal(expected, enumerable.UnionBy(second, KeyOf, comparer).ToArray());
                    Assert.Equal(expected, items.UnionBy(Iterate(second), KeyOf, comparer).ToArray());
                    Assert.Equal(expected, items.Select(x => x).UnionBy(second, KeyOf, comparer).ToArray());

                    Assert.Equal(expected, Pull(items.UnionBy(second, KeyOf, comparer)));
                    Assert.Equal(expected, Pull(list.UnionBy(second, KeyOf, comparer)));
                    Assert.Equal(expected, Pull(memory.OfMemory().UnionBy(second, KeyOf, comparer)));
                    Assert.Equal(expected, Pull(enumerable.UnionBy(Iterate(second), KeyOf, comparer)));
                    Assert.Equal(expected, Pull(items.Select(x => x).UnionBy(second, KeyOf, comparer)));
                }

                var withoutComparer = Expected(items, second, KeyOf, null);

                Assert.Equal(withoutComparer, items.UnionBy(second, KeyOf).ToArray());
                Assert.Equal(withoutComparer, list.UnionBy(second, KeyOf).ToArray());
                Assert.Equal(withoutComparer, enumerable.UnionBy(second, KeyOf).ToArray());
                Assert.Equal(withoutComparer, items.Select(x => x).UnionBy(second, KeyOf).ToArray());
            }
        }

        [Fact]
        public void FirstIsYieldedBeforeSecondEachKeyAtItsFirstOccurrence()
        {
            int[] items = [24, 1, 47, 2, 25];
            int[] second = [3, 49, 26, 4, 72];

            Assert.Equal([24, 2, 3, 4], items.UnionBy(second, KeyOf).ToArray());
            Assert.Equal([24, 2, 3, 4], Pull(items.UnionBy(second, KeyOf)));
            Assert.Equal([24, 2, 3, 4], Pull(Iterate(items).UnionBy(Iterate(second), KeyOf)));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);
            int[] second = [2, 7, 7, 30, 11, 0, 99];

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).UnionBy(second, KeyOf);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).UnionBy(Iterate(second), KeyOf);
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).UnionBy(second, x => x % 31, ByTens);
                var chained = items.Where(x => x > keepAbove).UnionBy(second, KeyOf).UnionBy(Scrambled(length / 3), x => x % 17);

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

                foreach (var item in second)
                {
                    if (seen.Add(KeyOf(item)))
                        handCoded.Add(item);
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
                Assert.Equal(Pull(chained), chained.ToList());
                Assert.Equal(Pull(chained), chained.Select(x => x).ToArray());
            }

            Assert.Equal(Pull(Enumerable.Range(3, length).UnionBy(second, KeyOf)), Enumerable.Range(3, length).UnionBy(second, KeyOf).ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            int[] second = [1, 4, 9, 16, 20, 22, 0, 5, 45];
            var expected = Expected(items, second, KeyOf, null).ToArray();

            var reversed = (int[])expected.Clone();
            Array.Reverse(reversed);

            IEnumerable<int>[] sequences =
            [
                items.UnionBy(second, KeyOf),
                new List<int>(items).UnionBy(second, KeyOf),
                Iterate(items).UnionBy(Iterate(second), KeyOf),
                items.Select(x => x).UnionBy(second, KeyOf),
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

                Assert.Equal(expected[0], sequence.First());
                Assert.Equal(expected[^1], sequence.Last());
            }

            Assert.False(Iterate(items).UnionBy(second, KeyOf).TryGetNonEnumeratedCount(out _));
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
        public void SecondIsOpenedOnlyOnceFirstIsDone()
        {
            var events = new List<string>();
            var first = new Logged<int>("first", [1, 2, 24], events);
            var second = new Logged<int>("second", [25, 3, 26], events);

            var sequence = first.UnionBy(second, KeyOf);

            Assert.Empty(events);

            List<string> expected = ["first.GetEnumerator", "first.1", "first.2", "first.24", "first.end", "first.Dispose", "second.GetEnumerator", "second.25", "second.3", "second.26", "second.end", "second.Dispose"];

            Assert.Equal([1, 2, 3], sequence.ToList());
            Assert.Equal(expected, events);

            events.Clear();

            Assert.Equal([1, 2, 3], sequence.Select(x => x).ToArray());
            Assert.Equal(expected, events);

            events.Clear();

            Assert.Equal([1, 2, 3], Pull(sequence));
            Assert.Equal(expected, events);
        }

        [Fact]
        public void SecondIsNotReadWhileTheConsumerIsInFirst()
        {
            var events = new List<string>();
            var first = new Logged<int>("first", [1, 2], events);
            var second = new Logged<int>("second", [3], events);

            var sequence = first.UnionBy(second, KeyOf);

            using (var enumerator = sequence.GetEnumerator())
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(1, enumerator.Current);
                Assert.DoesNotContain("second.GetEnumerator", events);
            }

            Assert.Equal(["first.GetEnumerator", "first.1", "first.Dispose"], events);

            events.Clear();

            Assert.Equal(1, sequence.First());
            Assert.Equal(["first.GetEnumerator", "first.1", "first.Dispose"], events);

            events.Clear();

            Assert.Equal([1, 2, 3], sequence.Take(3).ToArray());
            Assert.Equal(["first.GetEnumerator", "first.1", "first.2", "first.end", "first.Dispose", "second.GetEnumerator", "second.3", "second.Dispose"], events);

            events.Clear();

            using (sequence.GetEnumerator())
            {
            }

            Assert.DoesNotContain("second.GetEnumerator", events);
        }

        [Fact]
        public void BothSequencesAreReadAgainEachTimeTheSequenceIsRun()
        {
            var items = new List<int>();
            var values = new List<int>();
            var sequence = items.UnionBy(values, KeyOf);
            var typed = items.OfList().UnionBy(values, KeyOf);

            items.Add(1);
            values.Add(24);
            values.Add(2);

            Assert.Equal([1, 2], sequence.ToArray());
            Assert.Equal([1, 2], sequence.ToList());
            Assert.Equal([1, 2], typed.ToArray());
            Assert.Equal([1, 2], typed.ToList());
            Assert.Equal(2, sequence.Count());
            Assert.Equal(2, sequence.ElementAt(1));

            values.Add(3);

            Assert.Equal([1, 2, 3], sequence.ToArray());
            Assert.Equal([1, 2, 3], Pull(sequence));
            Assert.Equal([1, 2, 3], typed.ToList());

            items.Clear();
            values.Clear();

            Assert.Equal([], Pull(sequence));
            Assert.Equal([], sequence.ToArray());
            Assert.Equal([], sequence.Select(x => x).ToList());
        }

        [Fact]
        public void TheKeySelectorRunsOncePerElementInOrderEachTimeTheSequenceIsRun()
        {
            var items = Scrambled(50);
            int[] second = [3, 7, 101, 200];
            var seen = new List<int>();
            var sequence = items.UnionBy(second, logged);

            List<int> all = [.. items, .. second];

            Assert.Equal(Expected(items, second, KeyOf, null), sequence.ToList());
            Assert.Equal(all, seen);

            seen.Clear();

            Assert.Equal(Expected(items, second, KeyOf, null), Pull(sequence));
            Assert.Equal(all, seen);

            seen.Clear();

            Assert.Equal(Expected(items, second, KeyOf, null), sequence.ToList());
            Assert.Equal(all, seen);

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
            int[] second = [5, 6];
            var seen = new List<int>();

            Assert.Equal(24, items.UnionBy(second, logged).First());
            Assert.Equal([24], seen);

            seen.Clear();

            Assert.Equal([24, 2], items.UnionBy(second, logged).Take(2).ToArray());
            Assert.Equal([24, 1, 2], seen);

            seen.Clear();

            Assert.Equal([24, 2], Iterate(items).UnionBy(second, logged).Take(2).ToList());
            Assert.Equal([24, 1, 2], seen);

            seen.Clear();

            Assert.Equal(5, items.Select(x => x).UnionBy(second, logged).First(x => x > 4 && x < 10));
            Assert.Equal([24, 1, 2, 25, 3, 4, 5], seen);

            int logged(int x)
            {
                seen.Add(x);

                return KeyOf(x);
            }
        }

        [Fact]
        public void AUnionByOfAUnionByRunsEachKeySelectorOverItsOwnInput()
        {
            int[] a = [1, 2];
            int[] b = [2, 3];
            int[] c = [3, 4];
            var inner = new List<int>();
            var outer = new List<int>();

            Assert.Equal([1, 2, 3, 4], a.UnionBy(b, innerKey).UnionBy(c, outerKey).ToArray());
            Assert.Equal([1, 2, 2, 3], inner);
            Assert.Equal([1, 2, 3, 3, 4], outer);

            inner.Clear();
            outer.Clear();

            Assert.Equal([1, 2, 3, 4], Pull(Iterate(a).UnionBy(b, innerKey).UnionBy(c, outerKey)));
            Assert.Equal([1, 2, 2, 3], inner);
            Assert.Equal([1, 2, 3, 3, 4], outer);

            int innerKey(int x)
            {
                inner.Add(x);

                return x;
            }

            int outerKey(int x)
            {
                outer.Add(x);

                return x;
            }
        }

        [Fact]
        public void TheComparerAppliesAcrossBothSequences()
        {
            string[] words = ["apple", "Avocado", "banana"];
            string[] second = ["Blueberry", "cherry", "APRICOT", "Cherry"];

            Assert.Equal(["apple", "Avocado", "banana", "Blueberry", "cherry", "Cherry"], words.UnionBy(second, x => x[..1]).ToArray());
            Assert.Equal(["apple", "banana", "cherry"], words.UnionBy(second, x => x[..1], StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(["apple", "banana", "cherry"], Pull(Iterate(words).UnionBy(Iterate(second), x => x[..1], StringComparer.OrdinalIgnoreCase)));
        }

        [Fact]
        public void ANullKeyIsAKeyLikeAnyOther()
        {
            string[] words = ["a", null, "b", null, "a"];

            Assert.Equal(["a", null, "b", "c"], words.UnionBy([null, "c", "b"], x => x).ToArray());
            Assert.Equal(["a", null, "b", "c"], Pull(Iterate(words).UnionBy([null, "c"], x => x)));
            Assert.Equal(["a", "bb"], new[] { "a", "b" }.UnionBy(["bb", "cc"], x => x.Length > 1 ? null : x[..0]).ToArray());
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMadeInSystemLinqsOrder()
        {
            Func<int, int> nullSelector = null;
            IEnumerable<int> nullSecond = null;
            int[] second = [1];

            Assert.Throws<ArgumentNullException>("first", () => ((int[])null).UnionBy(nullSecond, nullSelector));
            Assert.Throws<ArgumentNullException>("first", () => ((List<int>)null).UnionBy(second, KeyOf));
            Assert.Throws<ArgumentNullException>("first", () => ((IEnumerable<int>)null).UnionBy(second, KeyOf, ByTens));

            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.UnionBy(nullSecond, nullSelector));
            Assert.Throws<ArgumentNullException>("second", () => Iterate(new[] { 1 }).UnionBy(nullSecond, KeyOf, ByTens));
            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.Select(x => x).UnionBy(nullSecond, nullSelector));

            Assert.Throws<ArgumentNullException>("keySelector", () => new[] { 1 }.UnionBy(second, nullSelector));
            Assert.Throws<ArgumentNullException>("keySelector", () => new List<int> { 1 }.UnionBy(second, nullSelector, ByTens));
            Assert.Throws<ArgumentNullException>("keySelector", () => ((ReadOnlyMemory<int>)new[] { 1 }).OfMemory().UnionBy(second, nullSelector));
            Assert.Throws<ArgumentNullException>("keySelector", () => new[] { 1 }.Select(x => x).UnionBy(second, nullSelector));
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
        public void TheSequencesAreDisposedWhenTheOperatorStopsEarly()
        {
            var tracked = new Tracked();

            foreach (var _ in tracked.Iterate(10).UnionBy(tracked.Iterate(5), KeyOf))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal(0, tracked.Iterate(10).UnionBy(tracked.Iterate(5), KeyOf).First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([0, 1, 2], tracked.Iterate(2).UnionBy(tracked.Iterate(5), KeyOf).Take(3).ToArray());
            Assert.Equal(4, tracked.Disposed);

            Assert.Equal([0, 1, 2], Pull(tracked.Iterate(2).UnionBy(tracked.Iterate(5), KeyOf).Take(3)));
            Assert.Equal(6, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).UnionBy(tracked.Iterate(3), x => x < 5 ? x : throw new InvalidOperationException()).ToList());
            Assert.Equal(7, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(tracked.Iterate(3).UnionBy(tracked.Iterate(10), x => x < 5 ? x : throw new InvalidOperationException())));
            Assert.Equal(9, tracked.Disposed);
        }
    }
}
