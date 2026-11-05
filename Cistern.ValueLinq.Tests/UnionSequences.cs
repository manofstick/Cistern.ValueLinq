using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class UnionSequences
    {
        private static readonly IEqualityComparer<int> ByTens = EqualityComparer<int>.Create((x, y) => x % 10 == y % 10, x => x % 10);

        private static int[] Scrambled(int count, int seed = 11)
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

        private static List<T> Expected<T>(IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            var seen = new HashSet<T>(comparer);
            var expected = new List<T>();
            foreach (var item in first)
            {
                if (seen.Add(item))
                    expected.Add(item);
            }

            foreach (var item in second)
            {
                if (seen.Add(item))
                    expected.Add(item);
            }

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        private static int[][] Seconds() => [[], [11], [48, 11, 48, 85], Scrambled(40, 3), [100, 200], Scrambled(1001, 5)];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeYieldsEachValueOnceAtItsFirstOccurrence(int length)
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

                    Assert.Equal(expected, items.Union(second, comparer).ToArray());
                    Assert.Equal(expected, list.Union(second, comparer).ToArray());
                    Assert.Equal(expected, memory.OfMemory().Union(second, comparer).ToArray());
                    Assert.Equal(expected, enumerable.Union(second, comparer).ToArray());
                    Assert.Equal(expected, items.Union(Iterate(second), comparer).ToArray());
                    Assert.Equal(expected, items.Select(x => x).Union(second, comparer).ToArray());

                    Assert.Equal(expected, Pull(items.Union(second, comparer)));
                    Assert.Equal(expected, Pull(list.Union(second, comparer)));
                    Assert.Equal(expected, Pull(memory.OfMemory().Union(second, comparer)));
                    Assert.Equal(expected, Pull(enumerable.Union(Iterate(second), comparer)));
                    Assert.Equal(expected, Pull(items.Select(x => x).Union(second, comparer)));
                }

                var withoutComparer = Expected(items, second, null);

                Assert.Equal(withoutComparer, items.Union(second).ToArray());
                Assert.Equal(withoutComparer, list.Union(second).ToArray());
                Assert.Equal(withoutComparer, enumerable.Union(second).ToArray());
                Assert.Equal(withoutComparer, items.Select(x => x).Union(second).ToArray());
            }
        }

        [Fact]
        public void FirstIsYieldedBeforeSecond()
        {
            int[] items = [5, 3, 5, 9, 3];
            int[] second = [9, 4, 1, 4, 5];

            Assert.Equal([5, 3, 9, 4, 1], items.Union(second).ToArray());
            Assert.Equal([5, 3, 9, 4, 1], Pull(Iterate(items).Union(second)));
            Assert.Equal([9, 4, 1, 5, 3], second.Union(items).ToList());
            Assert.Equal([3, 15], new[] { 3, 13 }.Union([23, 15, 5], ByTens).ToArray());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);
            var second = Scrambled(30, 7);

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).Union(second);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).Union(Iterate(second));
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).Union(second, ByTens);
                var chained = Iterate(items).Where(x => x > keepAbove).Union(second).Union(Scrambled(length, 3)).Union(Iterate(second));

                var pulled = Pull(filtered);

                var handCoded = new List<int>();
                var seen = new HashSet<int>();
                foreach (var item in items)
                {
                    if (item > keepAbove
                        && seen.Add(item))
                    {
                        handCoded.Add(item);
                    }
                }

                foreach (var item in second)
                {
                    if (seen.Add(item))
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

            Assert.Equal(Pull(Enumerable.Range(3, length).Union(second)), Enumerable.Range(3, length).Union(second).ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            var second = Scrambled(60, 7);
            var expected = Expected(items, second, null).ToArray();

            var reversed = (int[])expected.Clone();
            Array.Reverse(reversed);

            IEnumerable<int>[] sequences =
            [
                items.Union(second),
                new List<int>(items).Union(second),
                Iterate(items).Union(Iterate(second)),
                items.Select(x => x).Union(second),
                Iterate(items).Union([]).Union(second),
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

            Assert.False(Iterate(items).Union(second).TryGetNonEnumeratedCount(out _));
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
            var first = new Logged<int>("first", [1, 2, 1], events);
            var second = new Logged<int>("second", [2, 3, 3], events);

            var sequence = first.Union(second);

            Assert.Empty(events);

            List<string> expected = ["first.GetEnumerator", "first.1", "first.2", "first.1", "first.end", "first.Dispose", "second.GetEnumerator", "second.2", "second.3", "second.3", "second.end", "second.Dispose"];

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

            var sequence = first.Union(second);

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
            var sequence = items.Union(values);
            var typed = items.OfList().Union(values);

            items.Add(1);
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

        private sealed class CountingComparer
            : IEqualityComparer<int>
        {
            public int Calls;

            public bool Equals(int x, int y)
            {
                ++Calls;

                return x == y;
            }

            public int GetHashCode(int x)
            {
                ++Calls;

                return x;
            }

            public override bool Equals(object obj) => obj is CountingComparer;

            public override int GetHashCode() => 0;
        }

        [Fact]
        public void AUnionOfAUnionUnderAnEqualComparerHashesEachElementOnceWithTheFirstComparer()
        {
            int[] a = [1, 2];
            int[] b = [2, 3];
            int[] c = [3, 4];

            var inner = new CountingComparer();
            var outer = new CountingComparer();

            Assert.Equal([1, 2, 3, 4], a.Union(b, inner).Union(c, outer).ToList());
            Assert.Equal(8, inner.Calls);
            Assert.Equal(0, outer.Calls);

            inner.Calls = 0;

            Assert.Equal([1, 2, 3, 4], Pull(a.Union(b, inner).Union(c, outer)));
            Assert.Equal(8, inner.Calls);
            Assert.Equal(0, outer.Calls);

            inner.Calls = 0;

            IEnumerable<int> throughEnumerable = a.Union(b, inner);

            Assert.Equal([1, 2, 3, 4], throughEnumerable.Union(c, outer).ToArray());
            Assert.Equal(8, inner.Calls);
            Assert.Equal(0, outer.Calls);

            inner.Calls = 0;

            Assert.Equal(4, a.Union(b, inner).Union(c, inner).Count());
            Assert.Equal(8, inner.Calls);
        }

        [Fact]
        public void AUnionOfAUnionUnderAnotherComparerUnionsItsResult()
        {
            var first = new CountingComparer();
            int[] a = [1, 2];
            int[] b = [2, 3];
            int[] c = [3, 4];

            Assert.Equal([1, 2, 3, 4], a.Union(b, first).Union(c).ToList());
            Assert.Equal(5, first.Calls);

            first.Calls = 0;

            Assert.Equal([1, 2, 3, 4], a.Union(b).Union(c, EqualityComparer<int>.Default).Union([], first).ToList());
            Assert.Equal(4, first.Calls);

            string[] words = ["Bob", "bob"];

            Assert.Equal(["Bob"], words.Union(["BOB"]).Union(["bOb"], StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(["Bob", "bOb"], words.Union(["BOB"], StringComparer.OrdinalIgnoreCase).Union(["bOb"]).ToArray());
        }

        [Fact]
        public void AnExceptOfTheSameShapeIsNotTakenForAUnion()
        {
            var except =
                Iterate(new[] { 1, 2 })
                .OfEnumerable()
                .Concat(Iterate(new[] { 2, 3 }).OfEnumerable())
                .Except(new[] { 2 });

            Assert.Equal([1, 3, 2, 4], except.Union([2, 4]).ToArray());
            Assert.Equal([1, 3, 2, 4], Pull(except.Union([2, 4])));
            Assert.Equal([1, 3, 2, 4], ((IEnumerable<int>)except).Union([2, 4]).ToList());
        }

        [Fact]
        public void AChainOfUnionsKeepsOneTypeAndIsFlattened()
        {
            int[] start = [0];
            IEnumerable<int> typed = start.Union([1]);
            IEnumerable<int> untyped = start;

            for (var i = 1; i < 100_000; ++i)
            {
                int[] next = [i % 1000];

                typed = typed.Union(next);
                untyped = untyped.Union(next);
            }

            var expected = Enumerable.Range(0, 1000).ToArray();

            Assert.Equal(expected, typed.ToArray());
            Assert.Equal(expected, Pull(typed));
            Assert.Equal(1000, typed.Count());
            Assert.Equal(expected, untyped.ToArray());
            Assert.Equal(expected, Pull(untyped));
            Assert.Equal(999, untyped.Last());
        }

        [Fact]
        public void TheComparerAppliesAcrossBothSequences()
        {
            string[] words = ["apple", "Avocado", "apple", "APPLE"];
            string[] second = ["Apple", "cherry", "CHERRY", "avocado"];

            Assert.Equal(["apple", "Avocado", "APPLE", "Apple", "cherry", "CHERRY", "avocado"], words.Union(second).ToArray());
            Assert.Equal(["apple", "Avocado", "cherry"], words.Union(second, StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(["apple", "Avocado", "cherry"], Pull(Iterate(words).Union(Iterate(second), StringComparer.OrdinalIgnoreCase)));
        }

        [Fact]
        public void ANullIsAValueLikeAnyOther()
        {
            string[] words = ["a", null, "b", null, "a"];

            Assert.Equal(["a", null, "b", "c"], words.Union([null, "c", "b"]).ToArray());
            Assert.Equal(["a", null, "b", "c"], Pull(Iterate(words).Union([null, "c"])));
            Assert.Equal([null, "a"], new string[] { null }.Union(["a", null]).ToArray());
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMadeInSystemLinqsOrder()
        {
            IEnumerable<int> nullSecond = null;
            int[] second = [1];

            Assert.Throws<ArgumentNullException>("first", () => ((int[])null).Union(nullSecond));
            Assert.Throws<ArgumentNullException>("first", () => ((List<int>)null).Union(second));
            Assert.Throws<ArgumentNullException>("first", () => ((IEnumerable<int>)null).Union(second, ByTens));

            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.Union(nullSecond));
            Assert.Throws<ArgumentNullException>("second", () => Iterate(new[] { 1 }).Union(nullSecond, ByTens));
            Assert.Throws<ArgumentNullException>("second", () => ((ReadOnlyMemory<int>)new[] { 1 }).OfMemory().Union(nullSecond));
            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.Select(x => x).Union(nullSecond));
            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.Union(second).Union(nullSecond));
            Assert.Throws<ArgumentNullException>("second", () => new[] { 1 }.Union(second, ByTens).Union(nullSecond));
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

            foreach (var _ in tracked.Iterate(10).Union(tracked.Iterate(5)))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal(0, tracked.Iterate(10).Union(tracked.Iterate(5)).First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 20], tracked.Iterate(10).Union(new[] { 20, 21 }).Union(new[] { 22 }).Take(11).ToList());
            Assert.Equal(3, tracked.Disposed);

            Assert.Equal([0, 1, 2], tracked.Iterate(2).Union(tracked.Iterate(5)).Take(3).ToArray());
            Assert.Equal(5, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Select(x => x < 5 ? x : throw new InvalidOperationException()).Union(tracked.Iterate(3)).ToList());
            Assert.Equal(6, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(tracked.Iterate(3).Union(tracked.Iterate(10).Select(x => x < 5 ? x : throw new InvalidOperationException()))));
            Assert.Equal(8, tracked.Disposed);
        }
    }
}
