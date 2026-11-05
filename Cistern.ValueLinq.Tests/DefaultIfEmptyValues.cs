using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class DefaultIfEmptyValues
    {
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

        private static int[] Expected(int[] items, int defaultValue)
        {
            int[] expected =
                items is []
                    ? [defaultValue]
                    : items;

            return expected;
        }

        private static List<T> Pull<T>(IEnumerable<T> source)
        {
            var items = new List<T>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeYieldsItsElementsOrTheDefault(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);
            var expected = Expected(items, -5);
            var expectedWithoutValue = Expected(items, 0);

            Assert.Equal(expected, items.DefaultIfEmpty(-5).ToArray());
            Assert.Equal(expected, list.DefaultIfEmpty(-5).ToArray());
            Assert.Equal(expected, memory.DefaultIfEmpty(-5).ToArray());
            Assert.Equal(expected, enumerable.DefaultIfEmpty(-5).ToArray());
            Assert.Equal(expected, ((IEnumerable<int>)list).DefaultIfEmpty(-5).ToArray());
            Assert.Equal(expected, items.Select(x => x).DefaultIfEmpty(-5).ToArray());

            Assert.Equal(expectedWithoutValue, items.DefaultIfEmpty().ToArray());
            Assert.Equal(expectedWithoutValue, list.DefaultIfEmpty().ToArray());
            Assert.Equal(expectedWithoutValue, memory.DefaultIfEmpty().ToArray());
            Assert.Equal(expectedWithoutValue, enumerable.DefaultIfEmpty().ToArray());
            Assert.Equal(expectedWithoutValue, items.Select(x => x).DefaultIfEmpty().ToArray());

            Assert.Equal(expected, Pull(items.DefaultIfEmpty(-5)));
            Assert.Equal(expected, Pull(list.DefaultIfEmpty(-5)));
            Assert.Equal(expected, Pull(memory.DefaultIfEmpty(-5)));
            Assert.Equal(expected, Pull(enumerable.DefaultIfEmpty(-5)));
            Assert.Equal(expected, Pull(items.Select(x => x).DefaultIfEmpty(-5)));
            Assert.Equal(expectedWithoutValue, Pull(items.Select(x => x).DefaultIfEmpty()));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).DefaultIfEmpty(-5);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).DefaultIfEmpty(-5);
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).DefaultIfEmpty(-5);

                var pulled = Pull(filtered);

                var handCoded = new List<int>();
                foreach (var item in items)
                {
                    if (item > keepAbove)
                        handCoded.Add(item);
                }

                if (handCoded is [])
                    handCoded.Add(-5);

                var sum = 0;
                foreach (var item in handCoded)
                    sum += item;

                Assert.Equal(handCoded, pulled);
                Assert.Equal(pulled, filtered.ToList());
                Assert.Equal(pulled, filtered.ToArray());
                Assert.Equal(sum, filtered.Sum());
                Assert.Equal(pulled, filtered.Select(x => x).ToList());
                Assert.Equal(Pull(iterated), iterated.ToList());
                Assert.Equal(Pull(iterated), iterated.Select(x => x).ToArray());
                Assert.Equal(Pull(concatenated), concatenated.ToList());
                Assert.Equal(Pull(concatenated), concatenated.Select(x => x).ToList());
            }

            Assert.Equal(Expected(items, -5).Length, items.DefaultIfEmpty(-5).Select(x => x).Count());
            Assert.Equal(Pull(Enumerable.Range(3, length).DefaultIfEmpty(-5)), Enumerable.Range(3, length).DefaultIfEmpty(-5).ToList());
        }

        [Fact]
        public void AnEmptySourceGivesTheDefaultToAnAggregatorThatTakesSpans()
        {
            Assert.Equal(5, Array.Empty<int>().DefaultIfEmpty(5).Sum());
            Assert.Equal(5, new List<int>().DefaultIfEmpty(5).Sum());
            Assert.Equal(5, Array.Empty<int>().Concat(Array.Empty<int>()).DefaultIfEmpty(5).Sum());
            Assert.Equal(6, new[] { 1, 2, 3 }.DefaultIfEmpty(5).Sum());
            Assert.Equal(5, new[] { 1, 2, 3 }.Where(x => x > 3).DefaultIfEmpty(5).Sum());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            var expected = Expected(items, -5);
            var typed = items.DefaultIfEmpty(-5);

            Assert.Equal(expected.Length, typed.Count());
            Assert.Equal(expected, typed.ToArray());
            Assert.Equal(expected[0], typed.ElementAt(0));
            Assert.Equal(expected[^1], typed.ElementAt(^1));
            Assert.Equal(0, typed.ElementAtOrDefault(expected.Length));
            Assert.Equal(expected[1..], typed.Skip(1).ToArray());
            Assert.Equal(expected[..1], typed.Take(1).ToArray());
            Assert.Equal(Reversed(expected), typed.Reverse().ToArray());

            IEnumerable<int>[] sequences =
            [
                items.DefaultIfEmpty(-5),
                new List<int>(items).DefaultIfEmpty(-5),
                Iterate(items).DefaultIfEmpty(-5),
                items.Select(x => x).DefaultIfEmpty(-5),
            ];

            foreach (var sequence in sequences)
            {
                Assert.Equal(expected.Length, sequence.Count());
                Assert.Equal(expected.Length, sequence.LongCount());
                Assert.Equal(expected, sequence.ToArray());
                Assert.Equal(expected, sequence.ToList());
                Assert.Equal(expected[0], sequence.First());
                Assert.Equal(expected[^1], sequence.Last());
                Assert.Equal(Reversed(expected), sequence.Reverse().ToArray());
                Assert.Equal(0, sequence.ElementAtOrDefault(expected.Length));
                Assert.Equal(0, sequence.ElementAtOrDefault(^(expected.Length + 1)));

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
                    Assert.Equal(skipped.Length, sequence.Skip(count).Count());
                    Assert.Equal(taken, sequence.Take(count).ToArray());
                    Assert.Equal(Pull(sequence.Skip(count).Take(2)), sequence.Skip(count).Take(2).ToArray());
                }
            }

            Assert.True(items.DefaultIfEmpty(-5).TryGetNonEnumeratedCount(out var nonEnumeratedCount));
            Assert.Equal(expected.Length, nonEnumeratedCount);
            Assert.False(Iterate(items).DefaultIfEmpty(-5).TryGetNonEnumeratedCount(out _));

            static int[] Reversed(int[] values)
            {
                var reversed = (int[])values.Clone();
                Array.Reverse(reversed);

                return reversed;
            }
        }

        [Fact]
        public void TheDefaultOfAReferenceTypeIsNull()
        {
            Assert.Equal([null], Array.Empty<string>().DefaultIfEmpty().ToArray());
            Assert.Equal([null], Pull(Iterate(Array.Empty<string>()).DefaultIfEmpty()));
            Assert.Equal(["none"], new List<string>().DefaultIfEmpty("none").ToArray());
            Assert.Equal(["a", "b"], new[] { "a", "b" }.DefaultIfEmpty("none").ToArray());
        }

        [Fact]
        public void APulledSequenceStaysEndedAndHandsOutTheDefaultOnce()
        {
            using var enumerator = Array.Empty<int>().DefaultIfEmpty(7).GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.Equal(7, enumerator.Current);
            Assert.False(enumerator.MoveNext());
            Assert.False(enumerator.MoveNext());

            using var nonEmpty = new[] { 1, 2 }.DefaultIfEmpty(7).GetEnumerator();

            Assert.True(nonEmpty.MoveNext());
            Assert.Equal(1, nonEmpty.Current);
            Assert.True(nonEmpty.MoveNext());
            Assert.Equal(2, nonEmpty.Current);
            Assert.False(nonEmpty.MoveNext());
            Assert.False(nonEmpty.MoveNext());
        }

        [Fact]
        public void AHaltedDownstreamGetsNoDefault()
        {
            var seen = new List<int>();

            var first = new[] { 10, 11 }.DefaultIfEmpty(7).Select(logged).First();

            Assert.Equal(10, first);
            Assert.Equal([10], seen);
            Assert.Equal(7, Array.Empty<int>().DefaultIfEmpty(7).Select(x => x).First());

            int logged(int x)
            {
                seen.Add(x);

                return x;
            }
        }

        [Fact]
        public void NullSourceThrowsWhenTheSequenceIsMade()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).DefaultIfEmpty());
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).DefaultIfEmpty(1));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null).DefaultIfEmpty(1));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).DefaultIfEmpty());
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).DefaultIfEmpty(1));
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

            foreach (var _ in tracked.Iterate(10).DefaultIfEmpty(7))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal(0, tracked.Iterate(10).DefaultIfEmpty(7).First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal(7, tracked.Iterate(0).DefaultIfEmpty(7).First());
            Assert.Equal(3, tracked.Disposed);

            Assert.Equal([7], Pull(tracked.Iterate(0).DefaultIfEmpty(7)));
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Select(x => x < 5 ? x : throw new InvalidOperationException()).DefaultIfEmpty(7).ToList());
            Assert.Equal(5, tracked.Disposed);
        }
    }
}
