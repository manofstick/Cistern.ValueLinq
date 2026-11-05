using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class OrderedDescendingValues
    {
        private static readonly IComparer<int> ByLastDigit = Comparer<int>.Create((x, y) => (x % 10).CompareTo(y % 10));

        private sealed record Item(int Key, int Id);

        private static readonly IComparer<Item> ByKey = Comparer<Item>.Create((x, y) => x.Key.CompareTo(y.Key));

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

        private static T[] Expected<T>(T[] items, IComparer<T> comparer)
        {
            var indexes = new int[items.Length];
            for (var i = 0; i < indexes.Length; ++i)
                indexes[i] = i;

            Array.Sort(indexes, (x, y) => comparer.Compare(items[y], items[x]) switch { 0 => x.CompareTo(y), var order => order });

            var expected = new T[items.Length];
            for (var i = 0; i < indexes.Length; ++i)
                expected[i] = items[indexes[i]];

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeSortsDescendingStably(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);
            var expected = Expected(items, Comparer<int>.Default);
            var byLastDigit = Expected(items, ByLastDigit);

            Assert.Equal(expected, items.OrderDescending().ToArray());
            Assert.Equal(expected, list.OrderDescending().ToArray());
            Assert.Equal(expected, memory.OrderDescending().ToArray());
            Assert.Equal(expected, enumerable.OrderDescending().ToArray());
            Assert.Equal(expected, ((IEnumerable<int>)list).OrderDescending().ToArray());
            Assert.Equal(expected, items.Select(x => x).OrderDescending().ToArray());
            Assert.Equal(expected, items.OrderDescending(null).ToArray());
            Assert.Equal(expected, enumerable.OrderDescending(null).ToArray());

            Assert.Equal(byLastDigit, items.OrderDescending(ByLastDigit).ToArray());
            Assert.Equal(byLastDigit, list.OrderDescending(ByLastDigit).ToArray());
            Assert.Equal(byLastDigit, memory.OrderDescending(ByLastDigit).ToArray());
            Assert.Equal(byLastDigit, enumerable.OrderDescending(ByLastDigit).ToArray());
            Assert.Equal(byLastDigit, items.Select(x => x).OrderDescending(ByLastDigit).ToArray());

            Assert.Equal(expected, Pull(items.OrderDescending()));
            Assert.Equal(expected, Pull(list.OrderDescending()));
            Assert.Equal(expected, Pull(memory.OrderDescending()));
            Assert.Equal(expected, Pull(enumerable.OrderDescending()));
            Assert.Equal(byLastDigit, Pull(items.OrderDescending(ByLastDigit)));
            Assert.Equal(byLastDigit, Pull(enumerable.OrderDescending(ByLastDigit)));
            Assert.Equal(byLastDigit, Pull(items.Select(x => x).OrderDescending(ByLastDigit)));

            Assert.Empty(Enumerable.Empty<int>().OrderDescending().ToArray());
            Assert.Empty(Pull(Enumerable.Empty<int>().OrderDescending(ByLastDigit)));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EqualElementsKeepTheirSourceOrderAndIdentity(int length)
        {
            var items = new Item[length];
            for (var i = 0; i < length; ++i)
                items[i] = new Item((i * 7) % 5, i);

            var expected = Expected(items, ByKey);

            var pushed = Iterate(items).OrderDescending(ByKey).ToArray();
            var pulled = Pull(items.OrderDescending(ByKey));

            Assert.Equal(expected.Length, pushed.Length);
            Assert.Equal(expected.Length, pulled.Count);
            for (var i = 0; i < expected.Length; ++i)
            {
                Assert.Same(expected[i], pushed[i]);
                Assert.Same(expected[i], pulled[i]);
            }

            if (expected is [var first, .., var last])
            {
                Assert.Same(first, items.OrderDescending(ByKey).First());
                Assert.Same(last, items.OrderDescending(ByKey).Last());
                Assert.Same(first, Iterate(items).OrderDescending(ByKey).First());
                Assert.Same(last, Iterate(items).OrderDescending(ByKey).Last());
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).OrderDescending();
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).OrderDescending(ByLastDigit);
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).OrderDescending();

                var handCoded = new List<int>();
                var sum = 0L;
                foreach (var item in items)
                {
                    if (item > keepAbove)
                    {
                        handCoded.Add(item);
                        sum += item;
                    }
                }

                handCoded.Sort((x, y) => y.CompareTo(x));

                var pulled = Pull(filtered);

                Assert.Equal(handCoded, pulled);
                Assert.Equal(pulled, filtered.ToList());
                Assert.Equal(pulled, filtered.ToArray());
                Assert.Equal(sum, filtered.Select(x => (long)x).Sum());
                Assert.Equal(Pull(iterated), iterated.ToList());
                Assert.Equal(Pull(iterated), iterated.Select(x => x).ToArray());
                Assert.Equal(Pull(concatenated), concatenated.ToList());
                Assert.Equal(Pull(concatenated), concatenated.Select(x => x).ToList());
            }

            Assert.Equal(Pull(Enumerable.Range(3, length).OrderDescending(ByLastDigit)), Enumerable.Range(3, length).OrderDescending(ByLastDigit).ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ThenByOrdersTheTiesOfAnOrderDescending(int length)
        {
            var items = Scrambled(length);
            var byDigitThenValue = Comparer<int>.Create((x, y) => ByLastDigit.Compare(x, y) switch { 0 => y.CompareTo(x), var order => order });
            var byDigitThenValueDescending = Comparer<int>.Create((x, y) => ByLastDigit.Compare(x, y) switch { 0 => x.CompareTo(y), var order => order });

            var ascending = Expected(items, byDigitThenValue);
            var descending = Expected(items, byDigitThenValueDescending);

            Assert.Equal(ascending, items.OrderDescending(ByLastDigit).ThenBy(x => x).ToArray());
            Assert.Equal(ascending, Pull(Iterate(items).OrderDescending(ByLastDigit).ThenBy(x => x)));
            Assert.Equal(descending, items.OrderDescending(ByLastDigit).ThenByDescending(x => x).ToArray());
            Assert.Equal(descending, Pull(new List<int>(items).OrderDescending(ByLastDigit).ThenByDescending(x => x)));
            Assert.Equal(ascending, items.OrderDescending(ByLastDigit).ThenBy(x => x, Comparer<int>.Default).ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);

            IEnumerable<int>[] sequences =
            [
                items.OrderDescending(),
                new List<int>(items).OrderDescending(ByLastDigit),
                Iterate(items).OrderDescending(),
                Iterate(items).OrderDescending(ByLastDigit),
                items.Where(x => x > 50).OrderDescending(ByLastDigit),
            ];

            foreach (var sequence in sequences)
            {
                var pulled = Pull(sequence);

                Assert.Equal(pulled.Count, sequence.Count());
                Assert.Equal(pulled.Count, sequence.LongCount());

                if (sequence.TryGetNonEnumeratedCount(out var count))
                    Assert.Equal(pulled.Count, count);

                for (var i = 0; i < pulled.Count; i += 1 + (pulled.Count / 5))
                {
                    Assert.Equal(pulled[i], sequence.ElementAt(i));
                    Assert.Equal(pulled[i], sequence.ElementAt(^(pulled.Count - i)));
                }

                Assert.Equal(0, sequence.ElementAtOrDefault(pulled.Count));

                if (pulled is [var first, .., var last])
                {
                    Assert.Equal(first, sequence.First());
                    Assert.Equal(last, sequence.Last());
                }

                var (firstOrDefault, lastOrDefault) =
                    pulled is [var head, ..]
                        ? (head, pulled[^1])
                        : (0, 0);

                Assert.Equal(firstOrDefault, sequence.FirstOrDefault());
                Assert.Equal(lastOrDefault, sequence.LastOrDefault());

                var reversed = new List<int>(pulled);
                reversed.Reverse();

                var three = Math.Min(3, pulled.Count);
                var afterThree = pulled.GetRange(three, pulled.Count - three);
                var firstThree = pulled.GetRange(0, three);

                Assert.Equal(reversed, sequence.Reverse().ToArray());
                Assert.Equal(afterThree, sequence.Skip(3).ToArray());
                Assert.Equal(firstThree, sequence.Take(3).ToList());
                Assert.Equal(Pull(sequence.Skip(2).Take(3)), sequence.Skip(2).Take(3).ToArray());
            }

            Assert.True(items.OrderDescending().TryGetNonEnumeratedCount(out var arrayCount));
            Assert.Equal(length, arrayCount);
        }

        private sealed class Counted
        {
            public int Read;

            public IEnumerable<int> Iterate(int[] items)
            {
                ++Read;
                foreach (var item in items)
                    yield return item;
            }
        }

        [Fact]
        public void TheSourceIsSortedOnTheFirstMoveNext()
        {
            int[] items = [3, 1, 2];

            var counted = new Counted();
            var ordered = counted.Iterate(items).OrderDescending();
            using (var enumerator = ordered.GetEnumerator())
            {
                Assert.Equal(0, counted.Read);
                Assert.True(enumerator.MoveNext());
                Assert.Equal(1, counted.Read);
                Assert.Equal(3, enumerator.Current);
            }

            using (var enumerator = ((IEnumerable<int>)ordered).GetEnumerator())
            {
                Assert.Equal(1, counted.Read);
                Assert.True(enumerator.MoveNext());
                Assert.Equal(2, counted.Read);
                Assert.Equal(3, enumerator.Current);
            }
        }

        [Fact]
        public void NullSourceThrowsWhenTheSequenceIsMade()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).OrderDescending());
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).OrderDescending(ByLastDigit));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null).OrderDescending());
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).OrderDescending());
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).OrderDescending(null));
        }

        private sealed class Tracked
        {
            public int Disposed;

            public IEnumerable<int> Iterate(int count)
            {
                try
                {
                    for (var i = 1; i <= count; ++i)
                        yield return i;
                }
                finally
                {
                    ++Disposed;
                }
            }
        }

        [Fact]
        public void TheSourceIsDisposedOnceSortedAndWhenTheComparerThrows()
        {
            var tracked = new Tracked();
            var throwing = Comparer<int>.Create((x, y) => throw new InvalidOperationException());

            foreach (var _ in tracked.Iterate(10).OrderDescending())
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal(10, tracked.Iterate(10).OrderDescending().First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([10, 9], Pull(tracked.Iterate(10).OrderDescending().Take(2)));
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).OrderDescending(throwing).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(tracked.Iterate(10).OrderDescending(throwing)));
            Assert.Equal(5, tracked.Disposed);
        }
    }
}
