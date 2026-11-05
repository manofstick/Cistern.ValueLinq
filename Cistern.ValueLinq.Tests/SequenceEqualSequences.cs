using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class SequenceEqualSequences
    {
        private static readonly IEqualityComparer<int> SameLastDigit = EqualityComparer<int>.Create((x, y) => x % 10 == y % 10, x => x % 10);

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

        private static int[] Changed(int[] items, int index, int by)
        {
            var changed = (int[])items.Clone();
            changed[index] += by;

            return changed;
        }

        private static bool Pulled<T>(IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            comparer ??= EqualityComparer<T>.Default;

            using var firstEnumerator = first.GetEnumerator();
            using var secondEnumerator = second.GetEnumerator();

            while (firstEnumerator.MoveNext())
            {
                if (!secondEnumerator.MoveNext()
                    || !comparer.Equals(firstEnumerator.Current, secondEnumerator.Current))
                {
                    return false;
                }
            }

            var secondHasMore = secondEnumerator.MoveNext();

            return !secondHasMore;
        }

        private static IEnumerable<(int[] First, int[] Second)> Pairs(int length)
        {
            var items = Scrambled(length);

            yield return (items, (int[])items.Clone());
            yield return (items, [.. items, 5]);
            yield return ([.. items, 5], items);

            if (length > 0)
            {
                yield return (items, Changed(items, 0, 1));
                yield return (items, Changed(items, length / 2, 10));
                yield return (items, Changed(items, length - 1, 1));
                yield return (items, items[..^1]);
            }
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [10], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeAnswersAsAPull(int length)
        {
            foreach (var (first, second) in Pairs(length))
            {
                var list = new List<int>(first);
                ReadOnlyMemory<int> memory = first;
                IEnumerable<int> iterated = Iterate(first);
                IEnumerable<int> secondIterated = Iterate(second);

                foreach (var comparer in new[] { null, EqualityComparer<int>.Default, SameLastDigit })
                {
                    var expected = Pulled(first, second, comparer);

                    Assert.Equal(expected, list.SequenceEqual(second, comparer));
                    Assert.Equal(expected, list.SequenceEqual(secondIterated, comparer));
                    Assert.Equal(expected, memory.SequenceEqual(second, comparer));
                    Assert.Equal(expected, memory.SequenceEqual(secondIterated, comparer));
                    Assert.Equal(expected, iterated.SequenceEqual(second, comparer));
                    Assert.Equal(expected, iterated.SequenceEqual(secondIterated, comparer));
                    Assert.Equal(expected, ((IEnumerable<int>)first).SequenceEqual(new List<int>(second), comparer));
                    Assert.Equal(expected, first.OfArray().SequenceEqual(second.OfArray(), comparer));
                    Assert.Equal(expected, first.OfArray().SequenceEqual(second.OfEnumerable(), comparer));
                    Assert.Equal(expected, first.OfArray().SequenceEqual(secondIterated, comparer));
                }

                var withoutComparer = Pulled(first, second, null);

                Assert.Equal(withoutComparer, list.SequenceEqual(second));
                Assert.Equal(withoutComparer, memory.SequenceEqual(second));
                Assert.Equal(withoutComparer, iterated.SequenceEqual(secondIterated));
                Assert.Equal(withoutComparer, first.OfArray().SequenceEqual(new List<int>(second).OfList()));
                Assert.Equal(withoutComparer, first.OfArray().SequenceEqual(secondIterated));
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ChainsPushedAgreeWithChainsPulled(int length)
        {
            foreach (var (first, second) in Pairs(length))
            {
                foreach (var comparer in new[] { null, SameLastDigit })
                {
                    var selected = first.Select(x => x * 3);
                    var secondSelected = second.Select(x => x * 3);
                    Assert.Equal(Pulled(selected, secondSelected, comparer), selected.SequenceEqual(secondSelected, comparer));

                    var filtered = first.Where(x => x % 5 != 4);
                    var secondFiltered = Iterate(second).Where(x => x % 5 != 4);
                    Assert.Equal(Pulled(filtered, secondFiltered, comparer), filtered.SequenceEqual(secondFiltered, comparer));

                    var ordered = first.OrderBy(x => x);
                    var secondOrdered = second.OrderBy(x => x);
                    Assert.Equal(Pulled(ordered, secondOrdered, comparer), ordered.SequenceEqual(secondOrdered, comparer));

                    var concatenated = first.Select(x => x).Concat(first.Select(x => x - 7));
                    var secondConcatenated = second.Select(x => x).Concat(second.Select(x => x - 7));
                    Assert.Equal(Pulled(concatenated, secondConcatenated, comparer), concatenated.SequenceEqual(secondConcatenated, comparer));

                    var skipped = first.OfArray().Skip(1);
                    var secondSkipped = second.OfArray().Skip(1);
                    Assert.Equal(Pulled(skipped, secondSkipped, comparer), skipped.SequenceEqual(secondSkipped, comparer));
                }

                var ranged = Enumerable.Range(0, first.Length);
                var secondRanged = Enumerable.Range(0, second.Length);
                Assert.Equal(Pulled(ranged, secondRanged, null), ranged.SequenceEqual(secondRanged));
            }
        }

        [Fact]
        public void ComparerSeesPairsInOrderUntilTheFirstMismatch()
        {
            var first = new[] { 1, 2, 3, 4, 5 };
            var second = new[] { 1, 2, 9, 4, 5 };
            var compared = new List<(int, int)>();
            var recording = EqualityComparer<int>.Create(recordingEquals, x => x);

            (int, int)[] expected = [(1, 1), (2, 2), (3, 9)];

            check(() => new List<int>(first).SequenceEqual(second, recording));
            check(() => Iterate(first).SequenceEqual(Iterate(second), recording));
            check(() => first.OfArray().SequenceEqual(Iterate(second), recording));
            check(() => first.Select(x => x).SequenceEqual(second.Select(x => x), recording));
            check(() => new[] { 1, 2 }.Select(x => x).Concat(new[] { 3, 4, 5 }.Select(x => x)).SequenceEqual(second.OfArray(), recording));
            check(() => Iterate(first).Where(x => x > 0).SequenceEqual(second, recording));

            compared.Clear();

            Assert.False(first.OfArray().SequenceEqual(new[] { 1, 2 }.OfArray(), recording));
            Assert.Empty(compared);

            bool recordingEquals(int x, int y)
            {
                compared.Add((x, y));

                return x == y;
            }

            void check(Func<bool> call)
            {
                compared.Clear();

                Assert.False(call());
                Assert.Equal(expected, compared);
            }
        }

        private sealed class Tracked
        {
            public int Pulled;
            public int Disposed;

            public IEnumerable<int> Iterate(int count)
            {
                try
                {
                    for (var i = 0; i < count; ++i)
                    {
                        ++Pulled;
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
        public void PullingStopsAtAMismatchAndPastTheEndOfTheFirst()
        {
            var first = new Tracked();
            var second = new Tracked();

            Assert.False(first.Iterate(100).SequenceEqual(second.Iterate(100).Select(x => x == 3 ? -1 : x)));
            Assert.Equal(4, first.Pulled);
            Assert.Equal(4, second.Pulled);
            Assert.Equal(1, first.Disposed);
            Assert.Equal(1, second.Disposed);

            first = new Tracked();
            second = new Tracked();

            Assert.False(first.Iterate(3).SequenceEqual(second.Iterate(100)));
            Assert.Equal(3, first.Pulled);
            Assert.Equal(4, second.Pulled);
            Assert.Equal(1, first.Disposed);
            Assert.Equal(1, second.Disposed);

            second = new Tracked();

            Assert.True(new List<int> { 0, 1, 2 }.SequenceEqual(second.Iterate(3)));
            Assert.Equal(3, second.Pulled);
            Assert.Equal(1, second.Disposed);
        }

        [Fact]
        public void BothSequencesAreDisposedWhenTheComparerThrows()
        {
            var first = new Tracked();
            var second = new Tracked();
            var throwing = EqualityComparer<int>.Create(throwingEquals, x => x);

            Assert.Throws<DivideByZeroException>(() => first.Iterate(10).SequenceEqual(second.Iterate(10), throwing));
            Assert.Equal(1, first.Disposed);
            Assert.Equal(1, second.Disposed);

            Assert.Throws<DivideByZeroException>(() => first.Iterate(10).Select(x => x).SequenceEqual(second.Iterate(10), throwing));
            Assert.Equal(2, first.Disposed);
            Assert.Equal(2, second.Disposed);

            Assert.Throws<DivideByZeroException>(() => new[] { 0, 1, 2, 3, 4, 5 }.OfArray().SequenceEqual(second.Iterate(10), throwing));
            Assert.Equal(3, second.Disposed);

            static bool throwingEquals(int x, int y)
            {
                if (x == 4)
                    throw new DivideByZeroException();

                return x == y;
            }
        }

        [Fact]
        public void NullArgumentsThrow()
        {
            Assert.Throws<ArgumentNullException>("first", () => ((IEnumerable<int>)null).SequenceEqual(Array.Empty<int>()));
            Assert.Throws<ArgumentNullException>("first", () => ((IEnumerable<int>)null).SequenceEqual(null, SameLastDigit));
            Assert.Throws<ArgumentNullException>("first", () => ((List<int>)null).SequenceEqual(Array.Empty<int>()));
            Assert.Throws<ArgumentNullException>("second", () => Iterate(Array.Empty<int>()).SequenceEqual(null));
            Assert.Throws<ArgumentNullException>("second", () => new List<int>().SequenceEqual(null, SameLastDigit));
            Assert.Throws<ArgumentNullException>("second", () => ReadOnlyMemory<int>.Empty.SequenceEqual(null));
            Assert.Throws<ArgumentNullException>("second", () => Array.Empty<int>().OfArray().SequenceEqual((IEnumerable<int>)null));
        }
    }
}
