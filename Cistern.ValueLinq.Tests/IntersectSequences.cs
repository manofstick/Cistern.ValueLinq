using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class IntersectSequences
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

        private static List<T> Pull<T>(IEnumerable<T> source)
        {
            var items = new List<T>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        private static List<T> Expected<T>(IEnumerable<T> source, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            var remaining = new HashSet<T>(second, comparer);
            var expected = new List<T>();
            foreach (var item in source)
            {
                if (remaining.Remove(item))
                    expected.Add(item);
            }

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        private static int[][] Seconds() => [[], [11], [48, 11, 48, 85], Scrambled(40), [100, 200], Scrambled(1001)];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeKeepsTheFirstElementOfEachValueInSecond(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);

            foreach (var second in Seconds())
            {
                foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByTens })
                {
                    var expected = Expected(items, second, comparer);

                    Assert.Equal(expected, items.Intersect(second, comparer).ToArray());
                    Assert.Equal(expected, list.Intersect(second, comparer).ToArray());
                    Assert.Equal(expected, memory.OfMemory().Intersect(second, comparer).ToArray());
                    Assert.Equal(expected, enumerable.Intersect(second, comparer).ToArray());
                    Assert.Equal(expected, items.Intersect(Iterate(second), comparer).ToArray());
                    Assert.Equal(expected, items.Select(x => x).Intersect(second, comparer).ToArray());

                    Assert.Equal(expected, Pull(items.Intersect(second, comparer)));
                    Assert.Equal(expected, Pull(list.Intersect(second, comparer)));
                    Assert.Equal(expected, Pull(memory.OfMemory().Intersect(second, comparer)));
                    Assert.Equal(expected, Pull(enumerable.Intersect(Iterate(second), comparer)));
                    Assert.Equal(expected, Pull(items.Select(x => x).Intersect(second, comparer)));
                }

                var withoutComparer = Expected(items, second, null);

                Assert.Equal(withoutComparer, items.Intersect(second).ToArray());
                Assert.Equal(withoutComparer, list.Intersect(second).ToArray());
                Assert.Equal(withoutComparer, enumerable.Intersect(second).ToArray());
                Assert.Equal(withoutComparer, items.Select(x => x).Intersect(second).ToArray());
            }
        }

        [Fact]
        public void EachValueOfSecondIsYieldedOnceAtItsFirstOccurrenceInTheSource()
        {
            int[] items = [5, 3, 5, 9, 3, 1, 9];
            int[] second = [9, 9, 3, 4];

            Assert.Equal([3, 9], items.Intersect(second).ToArray());
            Assert.Equal([3, 9], Pull(Iterate(items).Intersect(second)));
            Assert.Equal([5, 3, 9, 1], items.Intersect([1, 3, 5, 7, 9]).ToList());
            Assert.Equal([3, 15], new[] { 3, 13, 15, 25 }.Intersect([23, 5], ByTens).ToArray());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);
            var second = Scrambled(30);

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).Intersect(second);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).Intersect(Iterate(second));
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).Intersect(second, ByTens);

                var pulled = Pull(filtered);

                var handCoded = new List<int>();
                var remaining = new HashSet<int>(second);
                foreach (var item in items)
                {
                    if (item > keepAbove
                        && remaining.Remove(item))
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

            Assert.Equal(Pull(Enumerable.Range(3, length).Intersect(second)), Enumerable.Range(3, length).Intersect(second).ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            var second = Scrambled(60);
            var expected = Expected(items, second, null).ToArray();

            var reversed = (int[])expected.Clone();
            Array.Reverse(reversed);

            IEnumerable<int>[] sequences =
            [
                items.Intersect(second),
                new List<int>(items).Intersect(second),
                Iterate(items).Intersect(Iterate(second)),
                items.Select(x => x).Intersect(second),
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

            Assert.False(Iterate(items).Intersect(second).TryGetNonEnumeratedCount(out _));
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
            var first = new Logged<int>("first", [1, 2, 3, 2], events);
            var second = new Logged<int>("second", [2, 3, 3], events);

            var sequence = first.Intersect(second);

            Assert.Empty(events);

            Assert.Equal([2, 3], sequence.ToList());
            Assert.Equal(["second.GetEnumerator", "second.2", "second.3", "second.3", "second.end", "second.Dispose", "first.GetEnumerator", "first.1", "first.2", "first.3", "first.2", "first.end", "first.Dispose"], events);

            events.Clear();

            Assert.Equal([2, 3], sequence.Select(x => x).ToArray());
            Assert.Equal(["second.GetEnumerator", "second.2", "second.3", "second.3", "second.end", "second.Dispose", "first.GetEnumerator", "first.1", "first.2", "first.3", "first.2", "first.end", "first.Dispose"], events);
        }

        [Fact]
        public void SecondIsReadWholeOnTheFirstMoveNextBeforeTheSourceIsPulled()
        {
            var events = new List<string>();
            var first = new Logged<int>("first", [1, 2, 3, 2], events);
            var second = new Logged<int>("second", [2, 3, 3], events);

            var sequence = first.Intersect(second);

            using (var enumerator = sequence.GetEnumerator())
            {
                Assert.DoesNotContain("second.GetEnumerator", events);

                Assert.True(enumerator.MoveNext());
                Assert.Equal(2, enumerator.Current);

                var secondRead = events.IndexOf("second.Dispose");
                var firstPulled = events.IndexOf("first.1");

                Assert.Equal(["second.GetEnumerator", "second.2", "second.3", "second.3", "second.end", "second.Dispose"], events.FindAll(e => e.StartsWith("second")));
                Assert.True(secondRead < firstPulled);

                Assert.True(enumerator.MoveNext());
                Assert.Equal(3, enumerator.Current);
                Assert.False(enumerator.MoveNext());
            }

            Assert.Single(events, "second.GetEnumerator");
            Assert.Equal(["first.GetEnumerator", "first.1", "first.2", "first.3", "first.2", "first.end", "first.Dispose"], events.FindAll(e => e.StartsWith("first")));

            events.Clear();

            using (sequence.GetEnumerator())
            {
            }

            Assert.DoesNotContain("second.GetEnumerator", events);
        }

        [Fact]
        public void SecondIsReadAgainEachTimeTheSequenceIsRun()
        {
            var values = new List<int> { 1 };
            int[] items = [1, 2, 3];
            var sequence = items.Intersect(values);

            Assert.Equal([1], sequence.ToArray());

            values.Add(3);

            Assert.Equal([1, 3], sequence.ToArray());
            Assert.Equal([1, 3], Pull(sequence));

            values.Clear();

            Assert.Equal([], Pull(sequence));
            Assert.Equal([], sequence.Select(x => x).ToList());
        }

        [Fact]
        public void TheSourceIsReadToItsEndOnceSecondIsUsedUp()
        {
            var events = new List<string>();
            var first = new Logged<int>("first", [1, 2, 3], events);

            Assert.Equal([1], first.Intersect([1]).ToList());
            Assert.Equal(["first.GetEnumerator", "first.1", "first.2", "first.3", "first.end", "first.Dispose"], events);

            events.Clear();

            Assert.Equal([1], Pull(first.Intersect([1])));
            Assert.Equal(["first.GetEnumerator", "first.1", "first.2", "first.3", "first.end", "first.Dispose"], events);
        }

        [Fact]
        public void TheSourceIsReadOnlyAsFarAsTheConsumerReads()
        {
            int[] items = [24, 1, 2, 25, 3, 4];
            int[] second = [2, 3, 4];
            var seen = new List<int>();

            Assert.Equal(2, items.Select(logged).Intersect(second).First());
            Assert.Equal([24, 1, 2], seen);

            seen.Clear();

            Assert.Equal([2, 3], items.Select(logged).Intersect(second).Take(2).ToArray());
            Assert.Equal([24, 1, 2, 25, 3], seen);

            seen.Clear();

            Assert.Equal([2, 3], Iterate(items).Select(logged).Intersect(second).Take(2).ToList());
            Assert.Equal([24, 1, 2, 25, 3], seen);

            seen.Clear();

            Assert.Equal(3, items.Select(logged).Intersect(second).First(x => x > 2));
            Assert.Equal([24, 1, 2, 25, 3], seen);

            int logged(int x)
            {
                seen.Add(x);

                return x;
            }
        }

        [Fact]
        public void TheComparerAppliesToSecond()
        {
            string[] words = ["apple", "Avocado", "banana", "Blueberry", "cherry", "APPLE", "Cherry"];
            string[] second = ["Apple", "cherry", "CHERRY"];

            Assert.Equal(["cherry"], words.Intersect(second).ToArray());
            Assert.Equal(["apple", "cherry"], words.Intersect(second, StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(["apple", "cherry"], Pull(Iterate(words).Intersect(Iterate(second), StringComparer.OrdinalIgnoreCase)));
        }

        [Fact]
        public void ANullIsAValueLikeAnyOther()
        {
            string[] words = ["a", null, "b", null, "a", "c"];

            Assert.Equal([null, "b"], words.Intersect([null, "b"]).ToArray());
            Assert.Equal([null, "b"], Pull(Iterate(words).Intersect([null, "b"])));
            Assert.Equal(["a", "c"], words.Intersect(["c", "a"]).ToArray());
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMadeInSystemLinqsOrder()
        {
            IEnumerable<int> nullSecond = null;
            int[] second = [1];

            Assert.Throws<ArgumentNullException>("first", () => ((int[])null).Intersect(nullSecond));
            Assert.Throws<ArgumentNullException>("first", () => ((List<int>)null).Intersect(second));
            Assert.Throws<ArgumentNullException>("first", () => ((IEnumerable<int>)null).Intersect(second, ByTens));

            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.Intersect(nullSecond));
            Assert.Throws<ArgumentNullException>("second", () => Iterate(new[] { 1 }).Intersect(nullSecond, ByTens));
            Assert.Throws<ArgumentNullException>("second", () => ((ReadOnlyMemory<int>)new[] { 1 }).OfMemory().Intersect(nullSecond));
            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.Select(x => x).Intersect(nullSecond));
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
            int[] second = [1, 2, 3, 4];

            foreach (var _ in tracked.Iterate(10).Intersect(second))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal(1, tracked.Iterate(10).Intersect(second).First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([1, 2], tracked.Iterate(10).Intersect(second).Take(2).ToList());
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Select(x => x < 5 ? x : throw new InvalidOperationException()).Intersect(second).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(tracked.Iterate(10).Select(x => x < 5 ? x : throw new InvalidOperationException()).Intersect(second)));
            Assert.Equal(5, tracked.Disposed);
        }

        [Fact]
        public void SecondIsDisposedOnceRead()
        {
            var tracked = new Tracked();
            int[] items = [2, 6, 1];

            Assert.Equal([2, 1], items.Intersect(tracked.Iterate(3)).ToArray());
            Assert.Equal(1, tracked.Disposed);

            Assert.Equal([2, 1], Pull(items.Intersect(tracked.Iterate(3))));
            Assert.Equal(2, tracked.Disposed);
        }
    }
}
