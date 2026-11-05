using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class ChunkedItems
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

        private static T[][] Expected<T>(T[] items, int size)
        {
            var chunks = new List<T[]>();
            var start = 0;
            while (start < items.Length)
            {
                var length = Math.Min(size, items.Length - start);
                chunks.Add(items[start..(start + length)]);
                start += length;
            }

            return chunks.ToArray();
        }

        private static List<T> Pull<T>(IEnumerable<T> source)
        {
            var items = new List<T>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        public static IEnumerable<object[]> LengthsAndSizes()
        {
            foreach (var length in new[] { 0, 1, 2, 7, 101, 1001 })
            {
                foreach (var size in new[] { 1, 3, 4, 5, 8, 100, int.MaxValue })
                    yield return [length, size];
            }
        }

        [Theory]
        [MemberData(nameof(LengthsAndSizes))]
        public void EveryCollectionShapeSplitsIntoChunksOfAtMostSize(int length, int size)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);
            var expected = Expected(items, size);

            Assert.Equal(expected, items.Chunk(size).ToArray());
            Assert.Equal(expected, list.Chunk(size).ToArray());
            Assert.Equal(expected, memory.Chunk(size).ToArray());
            Assert.Equal(expected, enumerable.Chunk(size).ToArray());
            Assert.Equal(expected, ((IEnumerable<int>)list).Chunk(size).ToArray());
            Assert.Equal(expected, items.Select(x => x).Chunk(size).ToArray());

            Assert.Equal(expected, Pull(items.Chunk(size)));
            Assert.Equal(expected, Pull(list.Chunk(size)));
            Assert.Equal(expected, Pull(memory.Chunk(size)));
            Assert.Equal(expected, Pull(enumerable.Chunk(size)));
            Assert.Equal(expected, Pull(items.Select(x => x).Chunk(size)));

            Assert.Empty(Enumerable.Empty<int>().Chunk(size).ToArray());
            Assert.Empty(Pull(Enumerable.Empty<int>().Chunk(size)));
        }

        [Theory]
        [MemberData(nameof(LengthsAndSizes))]
        public void PushAgreesWithPull(int length, int size)
        {
            var items = Scrambled(length);

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).Chunk(size);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).Chunk(size);
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).Chunk(size);
                var concatenatedArrays = items.Concat(Scrambled(length / 2)).Concat(Scrambled(3)).Chunk(size);

                var handCoded = new List<int>();
                foreach (var item in items)
                {
                    if (item > keepAbove)
                        handCoded.Add(item);
                }

                var pulled = Pull(filtered);

                Assert.Equal(Expected(handCoded.ToArray(), size), pulled);
                Assert.Equal(pulled, filtered.ToList());
                Assert.Equal(pulled, filtered.ToArray());
                Assert.Equal(handCoded.Count, filtered.Select(chunk => chunk.Length).Sum());
                Assert.Equal(Pull(iterated), iterated.ToList());
                Assert.Equal(Pull(iterated), iterated.Select(chunk => chunk).ToArray());
                Assert.Equal(Pull(concatenated), concatenated.ToList());
                Assert.Equal(Pull(concatenated), concatenated.Select(chunk => chunk).ToList());
                Assert.Equal(Pull(concatenatedArrays), concatenatedArrays.ToList());
                Assert.Equal(Pull(concatenatedArrays), concatenatedArrays.Select(chunk => chunk).ToList());
            }

            Assert.Equal(Pull(Enumerable.Range(3, length).Chunk(size)), Enumerable.Range(3, length).Chunk(size).ToList());
        }

        [Theory]
        [MemberData(nameof(LengthsAndSizes))]
        public void TheSpanPathChunksAsTheElementPathDoes(int length, int size)
        {
            var items = Scrambled(length);
            var expected = Expected(items, size);

            Assert.Equal(expected, items.Chunk(size).Select(chunk => chunk).ToArray());
            Assert.Equal(expected, Iterate(items).Chunk(size).Select(chunk => chunk).ToArray());
            Assert.Equal(expected, items.Concat(Array.Empty<int>()).Chunk(size).Select(chunk => chunk).ToArray());
            Assert.Equal(expected, items[..(length / 2)].Concat(items[(length / 2)..]).Chunk(size).Select(chunk => chunk).ToArray());

            var halfway = expected.Length / 2;
            var halted = items.Chunk(size).Select((chunk, index) => (chunk, index)).FirstOrDefault(pair => pair.index == halfway).chunk;
            var expectedHalted = halfway < expected.Length ? expected[halfway] : null;

            Assert.Equal(expectedHalted, halted);
        }

        [Fact]
        public void EachChunkIsAFreshArrayOfExactlyItsLength()
        {
            var items = Scrambled(10);

            var pushed = items.Chunk(4).ToList();
            var pulled = Pull(items.Chunk(4));
            var rerun = items.Chunk(4).ToList();

            Assert.Equal([4, 4, 2], pushed.Select(chunk => chunk.Length).ToArray());
            Assert.Equal([4, 4, 2], pulled.Select(chunk => chunk.Length).ToArray());
            Assert.NotSame(pushed[0], rerun[0]);
            Assert.NotSame(pushed[0], pulled[0]);

            pushed[0][0] = -1;
            pulled[0][0] = -1;

            Assert.Equal(Scrambled(10), items);
            Assert.Equal(Scrambled(10)[..4], rerun[0]);
            Assert.Equal(Scrambled(10)[..4], items.Chunk(4).ElementAt(0));
        }

        [Theory]
        [MemberData(nameof(LengthsAndSizes))]
        public void RequestsAgreeWithRunningTheSequence(int length, int size)
        {
            var items = Scrambled(length);
            var expected = Expected(items, size);

            IEnumerable<int[]>[] sequences =
            [
                items.Chunk(size),
                new List<int>(items).Chunk(size),
                ((ReadOnlyMemory<int>)items).Chunk(size),
                Iterate(items).Chunk(size),
                items.Concat(Array.Empty<int>()).Chunk(size),
                items.Where(x => x >= 0).Chunk(size),
                items.Select(x => x).Chunk(size),
                items.Chunk(size).Skip(0),
            ];

            foreach (var sequence in sequences)
            {
                Assert.Equal(expected.Length, sequence.Count());
                Assert.Equal(expected.Length, sequence.LongCount());
                Assert.Equal(expected, sequence.ToArray());
                Assert.Equal(expected, sequence.ToList());
                Assert.Equal(Reversed(expected), sequence.Reverse().ToArray());
                Assert.Null(sequence.ElementAtOrDefault(expected.Length));
                Assert.Null(sequence.ElementAtOrDefault(^(expected.Length + 1)));

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

            Assert.True(items.Chunk(size).TryGetNonEnumeratedCount(out var nonEnumeratedCount));
            Assert.Equal(expected.Length, nonEnumeratedCount);
            Assert.False(Iterate(items).Chunk(size).TryGetNonEnumeratedCount(out _));

            static int[][] Reversed(int[][] values)
            {
                var reversed = (int[][])values.Clone();
                Array.Reverse(reversed);

                return reversed;
            }
        }

        [Fact]
        public void ASliceStartsAtAChunkBoundaryOfTheSource()
        {
            var items = Scrambled(20);
            var expected = Expected(items, 3);

            Assert.Equal(expected[2..4], items.Chunk(3).Skip(2).Take(2).ToArray());
            Assert.Equal(expected[4..6], items.Chunk(3).Skip(2).Skip(2).Take(2).ToArray());
            Assert.Equal(expected[4..6], Pull(items.Chunk(3).Skip(2).Skip(2).Take(2)));
            Assert.Equal(expected[5], items.Chunk(3).Skip(1).ElementAt(4));
            Assert.Equal(expected[^1], items.Chunk(3).Skip(1).ElementAt(^1));
            Assert.Equal(expected[5..], new List<int>(items).Chunk(3).Skip(5).ToList());
            Assert.Equal(expected[..1], items.Chunk(3).Take(1).ToArray());
            Assert.Equal(Expected(items, int.MaxValue), items.Chunk(int.MaxValue).Take(int.MaxValue).ToArray());
            Assert.Empty(items.Chunk(int.MaxValue).Skip(2).ToArray());
        }

        [Fact]
        public void TheSourceIsReadWhenTheSequenceRuns()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var chunks = list.Chunk(2);

            list.Add(6);
            list.Add(7);

            Assert.Equal([[1, 2], [3, 4], [5, 6], [7]], chunks.ToArray());
            Assert.Equal([[1, 2], [3, 4], [5, 6], [7]], Pull(chunks));
        }

        [Fact]
        public void ALargeSizeOverAShortSourceYieldsOneChunk()
        {
            var expected = new[] { new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 } };

            Assert.Equal(expected, Enumerable.Range(0, 10).Chunk(int.MaxValue).ToArray());
            Assert.Equal(expected, Pull(Enumerable.Range(0, 10).Chunk(int.MaxValue)));
            Assert.Equal(expected, Iterate(expected[0]).Chunk(int.MaxValue).ToArray());
            Assert.Equal(expected, Pull(Iterate(expected[0]).Chunk(int.MaxValue)));
        }

        [Fact]
        public void ANonPositiveSizeOrNullSourceThrowsWhenTheSequenceIsMade()
        {
            Assert.Throws<ArgumentOutOfRangeException>("size", () => new[] { 1 }.Chunk(0));
            Assert.Throws<ArgumentOutOfRangeException>("size", () => new List<int> { 1 }.Chunk(-1));
            Assert.Throws<ArgumentOutOfRangeException>("size", () => Iterate(new[] { 1 }).Chunk(0));
            Assert.Throws<ArgumentOutOfRangeException>("size", () => new[] { 1 }.Select(x => x).Chunk(0));
            Assert.Throws<ArgumentOutOfRangeException>("size", () => Enumerable.Empty<int>().Chunk(0));

            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).Chunk(1));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null).Chunk(1));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).Chunk(1));
        }

        private sealed class Tracked
        {
            public int Yielded;
            public int Disposed;

            public IEnumerable<int> Iterate(int count)
            {
                try
                {
                    for (var i = 0; i < count; ++i)
                    {
                        ++Yielded;
                        yield return i;
                    }
                }
                finally
                {
                    ++Disposed;
                }
            }
        }

        [Fact]
        public void TheSourceIsReadOneChunkAtATime()
        {
            var tracked = new Tracked();

            using (var chunks = tracked.Iterate(10).Chunk(3).GetEnumerator())
            {
                Assert.True(chunks.MoveNext());
                Assert.Equal(3, tracked.Yielded);

                Assert.True(chunks.MoveNext());
                Assert.Equal(6, tracked.Yielded);
            }

            tracked.Yielded = 0;

            Assert.Equal([0, 1, 2], tracked.Iterate(10).Chunk(3).First());
            Assert.Equal(3, tracked.Yielded);
        }

        [Fact]
        public void TheSourceIsDisposedWhenTheOperatorStopsEarly()
        {
            var tracked = new Tracked();

            foreach (var _ in tracked.Iterate(10).Chunk(3))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal([0, 1, 2], tracked.Iterate(10).Chunk(3).First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([6, 7, 8], tracked.Iterate(10).Chunk(3).First(chunk => chunk[0] == 6));
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Chunk(3).Select(chunk => chunk[0] < 5 ? chunk : throw new InvalidOperationException()).ToList());
            Assert.Equal(4, tracked.Disposed);
        }
    }
}
