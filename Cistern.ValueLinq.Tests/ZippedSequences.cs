using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class ZippedSequences
    {
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

        private static int Combine(int first, int second) => (first * 1000) + second;

        private static int[] Combined(int[] first, int[] second)
        {
            var combined = new int[Math.Min(first.Length, second.Length)];
            for (var i = 0; i < combined.Length; ++i)
                combined[i] = Combine(first[i], second[i]);

            return combined;
        }

        private static (int, int)[] Paired(int[] first, int[] second)
        {
            var paired = new (int, int)[Math.Min(first.Length, second.Length)];
            for (var i = 0; i < paired.Length; ++i)
                paired[i] = (first[i], second[i]);

            return paired;
        }

        private static (int, int, int)[] Tripled(int[] first, int[] second, int[] third)
        {
            var tripled = new (int, int, int)[Math.Min(Math.Min(first.Length, second.Length), third.Length)];
            for (var i = 0; i < tripled.Length; ++i)
                tripled[i] = (first[i], second[i], third[i]);

            return tripled;
        }

        public static IEnumerable<object[]> Lengths() => [[0, 0], [0, 3], [3, 0], [1, 1], [7, 5], [5, 7], [101, 101], [1001, 999]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryShapeZipsAsTheHandCodedLoop(int firstLength, int secondLength)
        {
            var first = Scrambled(firstLength, 11);
            var second = Scrambled(secondLength, 29);
            var combined = Combined(first, second);

            foreach (var thirdLength in new[] { 0, Math.Min(firstLength, secondLength), firstLength + 1, secondLength / 2 })
            {
                var third = Scrambled(thirdLength, 53);
                var tripled = Tripled(first, second, third);

                Assert.Equal(tripled, first.Zip(second, third).ToArray());
                Assert.Equal(tripled, new List<int>(first).Zip(new List<int>(second), new List<int>(third)).ToList());
                Assert.Equal(tripled, Iterate(first).Zip(Iterate(second), Iterate(third)).ToArray());
                Assert.Equal(tripled, first.Select(x => x).Zip(second.Select(x => x), third.Select(x => x)).ToArray());
                Assert.Equal(tripled, Pull(first.Zip(second, third)));
                Assert.Equal(tripled, Pull(Iterate(first).Zip(Iterate(second), Iterate(third))));
                Assert.Equal(tripled, Pull(first.Select(x => x).Zip(second.Select(x => x), third.Select(x => x))));
                Assert.Equal(tripled, ((IEnumerable<(int, int, int)>)first.Zip(second, third)).Select(triple => triple).ToArray());
            }

            Assert.Equal(combined, first.Zip(second, Combine).ToArray());
            Assert.Equal(combined, new List<int>(first).Zip(new List<int>(second), Combine).ToList());
            Assert.Equal(combined, Iterate(first).Zip(Iterate(second), Combine).ToArray());
            Assert.Equal(combined, first.Select(x => x).Zip(second.Select(x => x), Combine).ToArray());
            Assert.Equal(combined, Pull(first.Zip(second, Combine)));
            Assert.Equal(combined, Pull(Iterate(first).Zip(Iterate(second), Combine)));
            Assert.Equal(combined, Pull(first.Select(x => x).Zip(second.Select(x => x), Combine)));
            Assert.Equal(combined, ((IEnumerable<int>)first.Zip(second, Combine)).Select(x => x).ToArray());

            var sum = 0L;
            foreach (var item in combined)
                sum += item;

            Assert.Equal(sum, first.Zip(second, Combine).Select(x => (long)x).Sum());
            Assert.Equal(sum, Iterate(first).Zip(second, Combine).Select(x => (long)x).Sum());

            Assert.Equal(Paired(first, second), first.Zip(second).ToArray());
            Assert.Equal(Paired(first, second), Pull(first.Select(x => x).Zip(second.Select(x => x))));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int firstLength, int secondLength)
        {
            var first = Scrambled(firstLength, 11);
            var second = Scrambled(secondLength, 29);

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = first.Where(x => x > keepAbove).Zip(second, Combine);
                var iterated = Iterate(first).Where(x => x > keepAbove).Zip(Iterate(second).Select(x => x + 1), Combine);
                var concatenated = first.Concat(second).Zip(second.Where(x => x > keepAbove), Combine);
                var ranged = Enumerable.Range(3, firstLength).Zip(first.Where(x => x > keepAbove), Combine);
                var tripled = first.Where(x => x > keepAbove).Zip(Iterate(second), first.Concat(second));

                var handCoded = new List<int>();
                var index = 0;
                foreach (var item in first)
                {
                    if (item <= keepAbove)
                        continue;

                    if (index == second.Length)
                        break;

                    handCoded.Add(Combine(item, second[index]));
                    ++index;
                }

                Assert.Equal(handCoded, Pull(filtered));
                Assert.Equal(handCoded, filtered.ToList());
                Assert.Equal(handCoded, filtered.ToArray());
                Assert.Equal(Pull(iterated), iterated.ToList());
                Assert.Equal(Pull(iterated), iterated.Select(x => x).ToArray());
                Assert.Equal(Pull(concatenated), concatenated.ToList());
                Assert.Equal(Pull(concatenated), concatenated.Select(x => x).ToList());
                Assert.Equal(Pull(ranged), ranged.ToArray());
                Assert.Equal(Pull(tripled), tripled.ToList());
                Assert.Equal(Pull(tripled), tripled.Select(triple => triple).ToArray());
            }
        }

        private sealed class Counting
        {
            public int Pulled;

            public IEnumerable<int> Iterate(int count)
            {
                for (var i = 0; i < count; ++i)
                {
                    ++Pulled;
                    yield return i;
                }
            }
        }

        private static int[] PullsOfAZip(params int[] lengths)
        {
            var pulls = new int[lengths.Length];
            while (true)
            {
                for (var i = 0; i < lengths.Length; ++i)
                {
                    if (pulls[i] == lengths[i])
                        return pulls;

                    ++pulls[i];
                }
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EachSequenceIsPulledAsSystemLinqPullsIt(int firstLength, int secondLength)
        {
            foreach (var thirdLength in new[] { 0, 2, firstLength, secondLength + 1 })
            {
                var expected = PullsOfAZip(firstLength, secondLength, thirdLength);

                var (first, second, third) = (new Counting(), new Counting(), new Counting());
                first.Iterate(firstLength).Zip(second.Iterate(secondLength), third.Iterate(thirdLength)).ToList();
                Assert.Equal(expected, new[] { first.Pulled, second.Pulled, third.Pulled });

                (first, second, third) = (new Counting(), new Counting(), new Counting());
                Pull(first.Iterate(firstLength).Zip(second.Iterate(secondLength), third.Iterate(thirdLength)));
                Assert.Equal(expected, new[] { first.Pulled, second.Pulled, third.Pulled });
            }

            var expectedOfTwo = PullsOfAZip(firstLength, secondLength);

            var (pushedFirst, pushedSecond) = (new Counting(), new Counting());
            pushedFirst.Iterate(firstLength).Zip(pushedSecond.Iterate(secondLength), Combine).ToList();
            Assert.Equal(expectedOfTwo, new[] { pushedFirst.Pulled, pushedSecond.Pulled });

            var (pulledFirst, pulledSecond) = (new Counting(), new Counting());
            Pull(pulledFirst.Iterate(firstLength).Zip(pulledSecond.Iterate(secondLength), Combine));
            Assert.Equal(expectedOfTwo, new[] { pulledFirst.Pulled, pulledSecond.Pulled });
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void TheResultSelectorRunsOncePerPairAndForEveryPairBeforeTheOneAsked(int firstLength, int secondLength)
        {
            var first = Scrambled(firstLength, 11);
            var second = Scrambled(secondLength, 29);
            var length = Math.Min(firstLength, secondLength);
            var calls = 0;

            var zipped = first.Zip(second, counted);
            var listed = new List<int>(first).Zip(second, counted);
            var iterated = Iterate(first).Zip(second, counted);

            foreach (var sequence in new IEnumerable<int>[] { zipped, listed, iterated })
            {
                calls = 0;
                sequence.ToList();
                Assert.Equal(length, calls);

                calls = 0;
                Pull(sequence);
                Assert.Equal(length, calls);

                calls = 0;
                Assert.Equal(length, sequence.Count());
                Assert.Equal(length, calls);

                if (length > 0)
                {
                    calls = 0;
                    sequence.ElementAt(length / 2);
                    Assert.Equal((length / 2) + 1, calls);

                    calls = 0;
                    sequence.Last();
                    Assert.Equal(length, calls);
                }
            }

            calls = 0;
            Assert.True(zipped.TryGetNonEnumeratedCount(out var nonEnumeratedCount));
            Assert.Equal(length, nonEnumeratedCount);
            Assert.Equal(0, calls);
            Assert.False(iterated.TryGetNonEnumeratedCount(out _));

            int counted(int x, int y)
            {
                ++calls;

                var combined = Combine(x, y);

                return combined;
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int firstLength, int secondLength)
        {
            var first = Scrambled(firstLength, 11);
            var second = Scrambled(secondLength, 29);
            var third = Scrambled((firstLength + secondLength) / 2, 53);

            assertRequests(Combined(first, second), [
                first.Zip(second, Combine),
                new List<int>(first).Zip(new List<int>(second), Combine),
                Iterate(first).Zip(second, Combine),
                first.Where(x => x >= 0).Zip(second, Combine),
                first.Select(x => x).Zip(second.Select(x => x), Combine),
            ]);

            assertRequests(Paired(first, second), [
                first.Zip(second),
                new List<int>(first).Zip(new List<int>(second)),
                first.Zip(Iterate(second)),
                first.Concat(Array.Empty<int>()).Zip(second),
                first.Zip(second).Skip(0),
            ]);

            assertRequests(Tripled(first, second, third), [
                first.Zip(second, third),
                new List<int>(first).Zip(new List<int>(second), new List<int>(third)),
                Iterate(first).Zip(second, third),
                first.Zip(second, Iterate(third)),
                first.Where(x => x >= 0).Zip(second, third),
            ]);

            static void assertRequests<T>(T[] expected, IEnumerable<T>[] sequences)
            {
                foreach (var sequence in sequences)
                {
                    Assert.Equal(expected.Length, sequence.Count());
                    Assert.Equal(expected.Length, sequence.LongCount());
                    Assert.Equal(expected, sequence.ToArray());
                    Assert.Equal(expected, sequence.ToList());
                    Assert.Equal(reversed(expected), sequence.Reverse().ToArray());
                    Assert.Equal(default, sequence.ElementAtOrDefault(expected.Length));
                    Assert.Equal(default, sequence.ElementAtOrDefault(^(expected.Length + 1)));

                    if (expected is [var firstExpected, .., var lastExpected])
                    {
                        Assert.Equal(firstExpected, sequence.First());
                        Assert.Equal(lastExpected, sequence.Last());
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
                    }
                }
            }

            static T[] reversed<T>(T[] values)
            {
                var reversedValues = (T[])values.Clone();
                Array.Reverse(reversedValues);

                return reversedValues;
            }
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMade()
        {
            int[] items = [1, 2, 3];
            Func<int, int, int> noSelector = null;

            Assert.Throws<ArgumentNullException>("first", () => ((IEnumerable<int>)null).Zip(items, Combine));
            Assert.Throws<ArgumentNullException>("second", () => items.Zip((IEnumerable<int>)null, Combine));
            Assert.Throws<ArgumentNullException>("resultSelector", () => items.Zip(items, noSelector));
            Assert.Throws<ArgumentNullException>("resultSelector", () => items.Select(x => x).Zip(items.Select(x => x), noSelector));
            Assert.Throws<ArgumentNullException>("first", () => ((IEnumerable<int>)null).Zip(items, items));
            Assert.Throws<ArgumentNullException>("second", () => items.Zip((IEnumerable<int>)null, items));
            Assert.Throws<ArgumentNullException>("third", () => items.Zip(items, (IEnumerable<int>)null));
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
        public void EverySequenceIsDisposedWhenTheOperatorStopsEarly()
        {
            var (first, second, third) = (new Tracked(), new Tracked(), new Tracked());

            foreach (var _ in first.Iterate(10).Zip(second.Iterate(10), third.Iterate(10)))
                break;

            Assert.Equal(new[] { 1, 1, 1 }, new[] { first.Disposed, second.Disposed, third.Disposed });

            Assert.Equal((0, 0, 0), first.Iterate(10).Zip(second.Iterate(10), third.Iterate(10)).First());
            Assert.Equal(new[] { 2, 2, 2 }, new[] { first.Disposed, second.Disposed, third.Disposed });

            Assert.Equal(3003, first.Iterate(10).Zip(second.Iterate(10), Combine).First(x => x == 3003));
            Assert.Equal(new[] { 3, 3 }, new[] { first.Disposed, second.Disposed });

            foreach (var _ in first.Iterate(10).Zip(second.Iterate(10), Combine))
                break;

            Assert.Equal(new[] { 4, 4 }, new[] { first.Disposed, second.Disposed });

            Assert.Throws<InvalidOperationException>(() => first.Iterate(10).Zip(second.Iterate(10), throwsAtFive).ToList());
            Assert.Equal(new[] { 5, 5 }, new[] { first.Disposed, second.Disposed });

            Assert.Throws<InvalidOperationException>(() => Pull(first.Iterate(10).Zip(second.Iterate(10), throwsAtFive)));
            Assert.Equal(new[] { 6, 6 }, new[] { first.Disposed, second.Disposed });

            static int throwsAtFive(int x, int y)
            {
                if (x >= 5)
                    throw new InvalidOperationException();

                var combined = Combine(x, y);

                return combined;
            }
        }
    }
}
