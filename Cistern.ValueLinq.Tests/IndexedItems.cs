using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class IndexedItems
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

        private static (int Index, T Item)[] Expected<T>(T[] items)
        {
            var expected = new (int Index, T Item)[items.Length];
            for (var i = 0; i < items.Length; ++i)
                expected[i] = (i, items[i]);

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
        public void EveryCollectionShapePairsEachElementWithItsPosition(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);
            var expected = Expected(items);

            Assert.Equal(expected, items.Index().ToArray());
            Assert.Equal(expected, list.Index().ToArray());
            Assert.Equal(expected, memory.Index().ToArray());
            Assert.Equal(expected, enumerable.Index().ToArray());
            Assert.Equal(expected, ((IEnumerable<int>)list).Index().ToArray());
            Assert.Equal(expected, items.Select(x => x).Index().ToArray());

            Assert.Equal(expected, Pull(items.Index()));
            Assert.Equal(expected, Pull(list.Index()));
            Assert.Equal(expected, Pull(memory.Index()));
            Assert.Equal(expected, Pull(enumerable.Index()));
            Assert.Equal(expected, Pull(items.Select(x => x).Index()));

            Assert.Empty(Enumerable.Empty<int>().Index().ToArray());
            Assert.Empty(Pull(Enumerable.Empty<int>().Index()));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).Index();
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).Index();
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).Index();

                var pulled = Pull(filtered);

                var handCoded = new List<(int Index, int Item)>();
                var sum = 0L;
                foreach (var item in items)
                {
                    if (item > keepAbove)
                    {
                        sum += (long)handCoded.Count * item;
                        handCoded.Add((handCoded.Count, item));
                    }
                }

                Assert.Equal(handCoded, pulled);
                Assert.Equal(pulled, filtered.ToList());
                Assert.Equal(pulled, filtered.ToArray());
                Assert.Equal(sum, filtered.Select(pair => (long)pair.Index * pair.Item).Sum());
                Assert.Equal(Pull(iterated), iterated.ToList());
                Assert.Equal(Pull(iterated), iterated.Select(pair => pair).ToArray());
                Assert.Equal(Pull(concatenated), concatenated.ToList());
                Assert.Equal(Pull(concatenated), concatenated.Select(pair => pair).ToList());
            }

            Assert.Equal(Pull(Enumerable.Range(3, length).Index()), Enumerable.Range(3, length).Index().ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void TheSpanPathIndexesAsTheElementPathDoes(int length)
        {
            var items = Scrambled(length);
            var expected = Expected(items);

            var sum = 0L;
            foreach (var (index, item) in expected)
                sum += (long)index * item;

            Assert.Equal(sum, items.Index().Select(pair => (long)pair.Index * pair.Item).Sum());
            Assert.Equal(sum, new List<int>(items).Index().Select(pair => (long)pair.Index * pair.Item).Sum());
            Assert.Equal(sum, items.Concat(Array.Empty<int>()).Index().Select(pair => (long)pair.Index * pair.Item).Sum());
            Assert.Equal(sum, items.Index().Where(pair => pair.Index >= 0).Select(pair => (long)pair.Index * pair.Item).Sum());

            var halfway = length / 2;
            var halted = items.Index().Where(pair => pair.Index >= halfway).Select(pair => pair).FirstOrDefault();
            var firstPast = halfway < length ? expected[halfway] : default;

            Assert.Equal(firstPast, halted);
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            var expected = Expected(items);

            IEnumerable<(int Index, int Item)>[] sequences =
            [
                items.Index(),
                new List<int>(items).Index(),
                ((ReadOnlyMemory<int>)items).Index(),
                Iterate(items).Index(),
                items.Concat(Array.Empty<int>()).Index(),
                items.Where(x => x >= 0).Index(),
                items.Select(x => x).Index(),
                items.Index().Skip(0),
            ];

            foreach (var sequence in sequences)
            {
                Assert.Equal(expected.Length, sequence.Count());
                Assert.Equal(expected.Length, sequence.LongCount());
                Assert.Equal(expected, sequence.ToArray());
                Assert.Equal(expected, sequence.ToList());
                Assert.Equal(Reversed(expected), sequence.Reverse().ToArray());
                Assert.Equal(default, sequence.ElementAtOrDefault(expected.Length));
                Assert.Equal(default, sequence.ElementAtOrDefault(^(expected.Length + 1)));

                if (expected is [var first, .., var last])
                {
                    Assert.Equal(first, sequence.First());
                    Assert.Equal(last, sequence.Last());
                }

                for (var i = 0; i < expected.Length; i += Math.Max(1, expected.Length / 7))
                {
                    Assert.Equal(expected[i], sequence.ElementAt(i));
                    Assert.Equal(expected[^(i + 1)], sequence.ElementAt(^(i + 1)));
                }

                foreach (var count in new[] { 0, 1, 2, expected.Length / 2, expected.Length, expected.Length + 1 })
                {
                    var skipped = expected[Math.Min(count, expected.Length)..];
                    var taken = expected[..Math.Min(count, expected.Length)];

                    Assert.Equal(skipped, sequence.Skip(count).ToArray());
                    Assert.Equal(skipped.Length, sequence.Skip(count).Count());
                    Assert.Equal(taken, sequence.Take(count).ToArray());
                    Assert.Equal(Pull(sequence.Skip(count).Skip(1).Take(2)), sequence.Skip(count).Skip(1).Take(2).ToArray());
                    Assert.Equal(Pull(sequence.Skip(count)), sequence.Skip(count).ToList());

                    if (skipped is [var firstSkipped, .., var lastSkipped])
                    {
                        Assert.Equal(firstSkipped, sequence.Skip(count).ElementAt(0));
                        Assert.Equal(lastSkipped, sequence.Skip(count).ElementAt(^1));
                    }
                }
            }

            Assert.True(items.Index().TryGetNonEnumeratedCount(out var nonEnumeratedCount));
            Assert.Equal(expected.Length, nonEnumeratedCount);
            Assert.False(Iterate(items).Index().TryGetNonEnumeratedCount(out _));

            static (int Index, int Item)[] Reversed((int Index, int Item)[] values)
            {
                var reversed = ((int Index, int Item)[])values.Clone();
                Array.Reverse(reversed);

                return reversed;
            }
        }

        [Fact]
        public void ASliceIsIndexedFromWhereItStartsInTheSource()
        {
            var items = Scrambled(20);
            var expected = Expected(items);

            Assert.Equal(expected[5..9], items.Index().Skip(5).Take(4).ToArray());
            Assert.Equal(expected[8..10], items.Index().Skip(5).Skip(3).Take(2).ToArray());
            Assert.Equal(expected[8..10], Pull(items.Index().Skip(5).Skip(3).Take(2)));
            Assert.Equal(expected[9], items.Index().Skip(5).ElementAt(4));
            Assert.Equal(expected[17], items.Index().Skip(5).ElementAt(^3));
            Assert.Equal(expected[5..], new List<int>(items).Index().Skip(5).ToList());
        }

        [Fact]
        public void NullSourceThrowsWhenTheSequenceIsMade()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).Index());
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null).Index());
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).Index());
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

            foreach (var _ in tracked.Iterate(10).Index())
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal((0, 0), tracked.Iterate(10).Index().First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal((3, 3), tracked.Iterate(10).Index().First(pair => pair.Index == 3));
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Index().Select(pair => pair.Index < 5 ? pair : throw new InvalidOperationException()).ToList());
            Assert.Equal(4, tracked.Disposed);
        }
    }
}
