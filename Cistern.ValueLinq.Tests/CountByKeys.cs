using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class CountByKeys
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

        private static string Text(IEnumerable<KeyValuePair<int, int>> counts)
        {
            var entries = new List<string>();
            foreach (var (key, count) in counts)
                entries.Add($"{key}:{count}");

            var text = string.Join(",", entries);

            return text;
        }

        private static string Pulled(IEnumerable<int> source, Func<int, int> keySelector, IEqualityComparer<int> comparer)
        {
            var counts = new Dictionary<int, int>(comparer);
            foreach (var item in source)
            {
                ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(counts, keySelector(item), out _);
                ++count;
            }

            var text = Text(counts);

            return text;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeCountsEachKeyInFirstSeenOrder(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);

            foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByTens })
            {
                var expected = Pulled(items, KeyOf, comparer);

                Assert.Equal(expected, Text(items.CountBy(KeyOf, comparer)));
                Assert.Equal(expected, Text(list.CountBy(KeyOf, comparer)));
                Assert.Equal(expected, Text(memory.CountBy(KeyOf, comparer)));
                Assert.Equal(expected, Text(enumerable.CountBy(KeyOf, comparer)));
                Assert.Equal(expected, Text(((IEnumerable<int>)items).CountBy(KeyOf, comparer)));
                Assert.Equal(expected, Text(((IEnumerable<int>)list).CountBy(KeyOf, comparer)));
                Assert.Equal(expected, Text(items.Select(x => x).CountBy(KeyOf, comparer)));
            }

            var withoutComparer = Pulled(items, KeyOf, null);

            Assert.Equal(withoutComparer, Text(items.CountBy(KeyOf)));
            Assert.Equal(withoutComparer, Text(list.CountBy(KeyOf)));
            Assert.Equal(withoutComparer, Text(memory.CountBy(KeyOf)));
            Assert.Equal(withoutComparer, Text(enumerable.CountBy(KeyOf)));
            Assert.Equal(withoutComparer, Text(items.Select(x => x).CountBy(KeyOf)));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);

            foreach (var comparer in new[] { null, ByTens })
            {
                var filtered = items.Where(x => x % 5 != 4).CountBy(KeyOf, comparer);
                var iterated = Iterate(items).Select(x => x + 1).CountBy(KeyOf, comparer);
                var ranged = Enumerable.Range(5, length).CountBy(KeyOf, comparer);
                var concatenated = items.Select(x => x).Concat(Scrambled(length / 2)).CountBy(KeyOf, comparer);

                var pulled = Text(filtered);

                Assert.Equal(Pulled(items.Where(x => x % 5 != 4), KeyOf, comparer), pulled);
                Assert.Equal(pulled, Text(filtered.ToList()));
                Assert.Equal(pulled, Text(filtered.ToArray()));
                Assert.Equal(pulled, Text(filtered.Select(pair => pair).ToList()));

                var iteratedPulled = Text(iterated);

                Assert.Equal(Pulled(Iterate(items).Select(x => x + 1), KeyOf, comparer), iteratedPulled);
                Assert.Equal(iteratedPulled, Text(iterated.ToList()));
                Assert.Equal(iteratedPulled, Text(iterated.Where(pair => pair.Value > 0).ToArray()));

                Assert.Equal(Pulled(Enumerable.Range(5, length), KeyOf, comparer), Text(ranged.ToList()));
                Assert.Equal(Text(concatenated), Text(concatenated.ToList()));
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            var countBy = items.CountBy(KeyOf);
            var iterated = Iterate(items).Where(x => x % 3 != 0).CountBy(KeyOf, ByTens);

            var keys = 0;
            foreach (var _ in countBy)
                ++keys;

            var iteratedKeys = 0;
            foreach (var _ in iterated)
                ++iteratedKeys;

            Assert.Equal(keys, countBy.Count());
            Assert.Equal(keys, countBy.LongCount());
            Assert.Equal(Text(countBy), Text(countBy.ToArray()));
            Assert.Equal(iteratedKeys, iterated.Count());
            Assert.Equal(Text(iterated), Text(iterated.ToArray()));
            Assert.Equal(Text(countBy).Split(',')[^1], Text(countBy.Reverse()).Split(',')[0]);
        }

        [Fact]
        public void TheKeySelectorRunsOncePerElementEachTimeTheSequenceIsRun()
        {
            var items = Scrambled(50);
            var calls = 0;

            var countBy = items.CountBy(countedKeyOf);

            Assert.Equal(0, calls);

            foreach (var _ in countBy) { }
            Assert.Equal(50, calls);

            _ = countBy.Count();
            Assert.Equal(100, calls);

            _ = countBy.ToArray();
            Assert.Equal(150, calls);

            _ = countBy.ToList();
            Assert.Equal(200, calls);

            _ = countBy.First();
            Assert.Equal(250, calls);

            int countedKeyOf(int x)
            {
                ++calls;

                return KeyOf(x);
            }
        }

        [Fact]
        public void EachRunCountsTheSourceAsItIsThen()
        {
            var list = new List<int> { 1, 2, 1 };
            var countBy = list.CountBy(x => x);

            Assert.Equal("1:2,2:1", Text(countBy));

            list.Add(3);
            list.Add(2);

            Assert.Equal("1:2,2:2,3:1", Text(countBy));
            Assert.Equal(3, countBy.Count());
        }

        [Fact]
        public void TheCountIsKnownWithoutRunningOnlyForAnEmptySource()
        {
            var calls = 0;

            Assert.True(Array.Empty<int>().CountBy(countedKeyOf).TryGetNonEnumeratedCount(out var emptyCount));
            Assert.Equal(0, emptyCount);
            Assert.False(Scrambled(10).CountBy(countedKeyOf).TryGetNonEnumeratedCount(out _));
            Assert.False(Iterate(Scrambled(10)).CountBy(countedKeyOf).TryGetNonEnumeratedCount(out _));
            Assert.Equal(0, calls);

            int countedKeyOf(int x)
            {
                ++calls;

                return x;
            }
        }

        [Fact]
        public void NullKeysThrow()
        {
            string[] items = ["a", null, "a"];

            Assert.Throws<ArgumentNullException>("key", () => items.CountBy(x => x).ToList());
            Assert.Throws<ArgumentNullException>("key", () => Iterate(items).CountBy(x => x).Count());
            Assert.Throws<ArgumentNullException>("key", () => items.Select(x => x).CountBy(x => x).ToArray());
            Assert.Throws<ArgumentNullException>("key", pullAll);

            void pullAll()
            {
                foreach (var _ in items.CountBy(x => x)) { }
            }
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMade()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).CountBy(KeyOf));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null).CountBy(KeyOf));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).CountBy(KeyOf, ByTens));
            Assert.Throws<ArgumentNullException>("keySelector", () => Scrambled(3).CountBy<int, int>(null));
            Assert.Throws<ArgumentNullException>("keySelector", () => Iterate(Scrambled(3)).CountBy<int, int>(null));
            Assert.Throws<ArgumentNullException>("keySelector", () => Scrambled(3).Select(x => x).CountBy<int, int>(null));
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
        public void TheSourceIsDisposedOnceItIsCounted()
        {
            var tracked = new Tracked();

            using (var enumerator = tracked.Iterate(10).CountBy(x => x % 3).GetEnumerator())
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(1, tracked.Disposed);
            }

            foreach (var _ in tracked.Iterate(10).CountBy(x => x % 3))
                break;

            Assert.Equal(2, tracked.Disposed);

            Assert.Equal(new KeyValuePair<int, int>(0, 4), tracked.Iterate(10).CountBy(x => x % 3).First());
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).CountBy(x => x < 5 ? x : throw new InvalidOperationException()).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(pullUntilTheKeySelectorThrows);
            Assert.Equal(5, tracked.Disposed);

            void pullUntilTheKeySelectorThrows()
            {
                foreach (var _ in tracked.Iterate(10).Select(x => x).CountBy(x => x < 5 ? x : throw new InvalidOperationException())) { }
            }
        }
    }
}
