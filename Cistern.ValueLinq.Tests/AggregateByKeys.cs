using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class AggregateByKeys
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

        private static int SeedOf(int key) => key * 1000;

        private static int Fold(int accumulate, int x) => (accumulate * 3) + x;

        private static string Text(IEnumerable<KeyValuePair<int, int>> accumulates)
        {
            var entries = new List<string>();
            foreach (var (key, accumulate) in accumulates)
                entries.Add($"{key}:{accumulate}");

            var text = string.Join(",", entries);

            return text;
        }

        private static string Pulled(IEnumerable<int> source, Func<int, int> keySelector, Func<int, int> seedSelector, Func<int, int, int> func, IEqualityComparer<int> comparer)
        {
            var accumulates = new Dictionary<int, int>(comparer);
            var order = new List<int>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                if (accumulates.TryGetValue(key, out var accumulate))
                {
                    accumulates[key] = func(accumulate, item);
                }
                else
                {
                    accumulates[key] = func(seedSelector(key), item);
                    order.Add(key);
                }
            }

            var entries = new List<KeyValuePair<int, int>>();
            foreach (var key in order)
                entries.Add(new KeyValuePair<int, int>(key, accumulates[key]));

            var text = Text(entries);

            return text;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeFoldsEachKeyInFirstSeenOrder(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);

            foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByTens })
            {
                var seeded = Pulled(items, KeyOf, _ => 7, Fold, comparer);
                var selected = Pulled(items, KeyOf, SeedOf, Fold, comparer);

                Assert.Equal(seeded, Text(items.AggregateBy(KeyOf, 7, Fold, comparer)));
                Assert.Equal(seeded, Text(list.AggregateBy(KeyOf, 7, Fold, comparer)));
                Assert.Equal(seeded, Text(memory.AggregateBy(KeyOf, 7, Fold, comparer)));
                Assert.Equal(seeded, Text(enumerable.AggregateBy(KeyOf, 7, Fold, comparer)));
                Assert.Equal(seeded, Text(((IEnumerable<int>)list).AggregateBy(KeyOf, 7, Fold, comparer)));
                Assert.Equal(seeded, Text(items.Select(x => x).AggregateBy(KeyOf, 7, Fold, comparer)));

                Assert.Equal(selected, Text(items.AggregateBy(KeyOf, SeedOf, Fold, comparer)));
                Assert.Equal(selected, Text(list.AggregateBy(KeyOf, SeedOf, Fold, comparer)));
                Assert.Equal(selected, Text(memory.AggregateBy(KeyOf, SeedOf, Fold, comparer)));
                Assert.Equal(selected, Text(enumerable.AggregateBy(KeyOf, SeedOf, Fold, comparer)));
                Assert.Equal(selected, Text(((IEnumerable<int>)list).AggregateBy(KeyOf, SeedOf, Fold, comparer)));
                Assert.Equal(selected, Text(items.Select(x => x).AggregateBy(KeyOf, SeedOf, Fold, comparer)));
            }

            var withoutComparer = Pulled(items, KeyOf, SeedOf, Fold, null);

            Assert.Equal(withoutComparer, Text(items.AggregateBy(KeyOf, SeedOf, Fold)));
            Assert.Equal(withoutComparer, Text(list.AggregateBy(KeyOf, SeedOf, Fold)));
            Assert.Equal(withoutComparer, Text(memory.AggregateBy(KeyOf, SeedOf, Fold)));
            Assert.Equal(withoutComparer, Text(enumerable.AggregateBy(KeyOf, SeedOf, Fold)));
            Assert.Equal(withoutComparer, Text(items.Select(x => x).AggregateBy(KeyOf, SeedOf, Fold)));
            Assert.Equal(Pulled(items, KeyOf, _ => 0, Fold, null), Text(items.Select(x => x).AggregateBy(KeyOf, 0, Fold)));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);

            foreach (var comparer in new[] { null, ByTens })
            {
                var filtered = items.Where(x => x % 5 != 4).AggregateBy(KeyOf, SeedOf, Fold, comparer);
                var iterated = Iterate(items).Select(x => x + 1).AggregateBy(KeyOf, 3, Fold, comparer);
                var ranged = Enumerable.Range(5, length).AggregateBy(KeyOf, SeedOf, Fold, comparer);
                var concatenated = items.Select(x => x).Concat(Scrambled(length / 2)).AggregateBy(KeyOf, SeedOf, Fold, comparer);

                var pulled = Text(filtered);

                Assert.Equal(Pulled(items.Where(x => x % 5 != 4), KeyOf, SeedOf, Fold, comparer), pulled);
                Assert.Equal(pulled, Text(filtered.ToList()));
                Assert.Equal(pulled, Text(filtered.ToArray()));
                Assert.Equal(pulled, Text(filtered.Select(pair => pair).ToList()));

                var iteratedPulled = Text(iterated);

                Assert.Equal(Pulled(Iterate(items).Select(x => x + 1), KeyOf, _ => 3, Fold, comparer), iteratedPulled);
                Assert.Equal(iteratedPulled, Text(iterated.ToList()));
                Assert.Equal(iteratedPulled, Text(iterated.Where(pair => pair.Key >= 0).ToArray()));

                Assert.Equal(Pulled(Enumerable.Range(5, length), KeyOf, SeedOf, Fold, comparer), Text(ranged.ToList()));
                Assert.Equal(Text(concatenated), Text(concatenated.ToList()));
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            var aggregateBy = items.AggregateBy(KeyOf, SeedOf, Fold);
            var iterated = Iterate(items).Where(x => x % 3 != 0).AggregateBy(KeyOf, 1, Fold, ByTens);

            var keys = 0;
            foreach (var _ in aggregateBy)
                ++keys;

            var iteratedKeys = 0;
            foreach (var _ in iterated)
                ++iteratedKeys;

            Assert.Equal(keys, aggregateBy.Count());
            Assert.Equal(keys, aggregateBy.LongCount());
            Assert.Equal(Text(aggregateBy), Text(aggregateBy.ToArray()));
            Assert.Equal(iteratedKeys, iterated.Count());
            Assert.Equal(Text(iterated), Text(iterated.ToArray()));
            Assert.Equal(Text(aggregateBy).Split(',')[^1], Text(aggregateBy.Reverse()).Split(',')[0]);
        }

        [Fact]
        public void TheDelegatesRunInOrderEachTimeTheSequenceIsRun()
        {
            int[] items = [1, 12, 2, 23, 13];
            var calls = new List<string>();

            var aggregateBy = items.AggregateBy(loggedKeyOf, loggedSeedOf, loggedFold);

            Assert.Empty(calls);

            const string run = "k1,s1,f1,k12,s2,f12,k2,f2,k23,s3,f23,k13,f13";

            foreach (var _ in aggregateBy) { }
            Assert.Equal(run, string.Join(",", calls));

            calls.Clear();
            _ = aggregateBy.Count();
            Assert.Equal(run, string.Join(",", calls));

            calls.Clear();
            _ = aggregateBy.ToArray();
            Assert.Equal(run, string.Join(",", calls));

            calls.Clear();
            _ = Iterate(items).AggregateBy(loggedKeyOf, loggedSeedOf, loggedFold).ToList();
            Assert.Equal(run, string.Join(",", calls));

            calls.Clear();
            _ = items.AggregateBy(loggedKeyOf, 0, loggedFold).First();
            Assert.Equal("k1,f1,k12,f12,k2,f2,k23,f23,k13,f13", string.Join(",", calls));

            int loggedKeyOf(int x)
            {
                calls.Add($"k{x}");

                return x / 10;
            }

            int loggedSeedOf(int key)
            {
                calls.Add($"s{key + 1}");

                return key;
            }

            int loggedFold(int accumulate, int x)
            {
                calls.Add($"f{x}");

                return accumulate + x;
            }
        }

        [Fact]
        public void EachRunFoldsTheSourceAsItIsThen()
        {
            var list = new List<int> { 1, 2, 1 };
            var aggregateBy = list.AggregateBy(x => x, 0, (sum, x) => sum + x);

            Assert.Equal("1:2,2:2", Text(aggregateBy));

            list.Add(3);
            list.Add(2);

            Assert.Equal("1:2,2:4,3:3", Text(aggregateBy));
            Assert.Equal(3, aggregateBy.Count());
        }

        [Fact]
        public void TheCountIsKnownWithoutRunningOnlyForAnEmptySource()
        {
            var calls = 0;

            Assert.True(Array.Empty<int>().AggregateBy(countedKeyOf, 0, Fold).TryGetNonEnumeratedCount(out var emptyCount));
            Assert.Equal(0, emptyCount);
            Assert.False(Scrambled(10).AggregateBy(countedKeyOf, SeedOf, Fold).TryGetNonEnumeratedCount(out _));
            Assert.False(Iterate(Scrambled(10)).AggregateBy(countedKeyOf, 0, Fold).TryGetNonEnumeratedCount(out _));
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

            Assert.Throws<ArgumentNullException>("key", () => items.AggregateBy(x => x, 0, (count, _) => count + 1).ToList());
            Assert.Throws<ArgumentNullException>("key", () => Iterate(items).AggregateBy(x => x, _ => 0, (count, _) => count + 1).Count());
            Assert.Throws<ArgumentNullException>("key", () => items.Select(x => x).AggregateBy(x => x, 0, (count, _) => count + 1).ToArray());
            Assert.Throws<ArgumentNullException>("key", pullAll);

            void pullAll()
            {
                foreach (var _ in items.AggregateBy(x => x, _ => 0, (count, _) => count + 1)) { }
            }
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMade()
        {
            Func<int, int> noKeySelector = null;
            Func<int, int> noSeedSelector = null;
            Func<int, int, int> noFunc = null;

            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).AggregateBy(KeyOf, 0, Fold));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null).AggregateBy(KeyOf, SeedOf, Fold));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).AggregateBy(KeyOf, 0, Fold, ByTens));
            Assert.Throws<ArgumentNullException>("keySelector", () => Scrambled(3).AggregateBy(noKeySelector, 0, Fold));
            Assert.Throws<ArgumentNullException>("keySelector", () => Iterate(Scrambled(3)).AggregateBy(noKeySelector, SeedOf, Fold));
            Assert.Throws<ArgumentNullException>("keySelector", () => Scrambled(3).Select(x => x).AggregateBy(noKeySelector, noSeedSelector, noFunc));
            Assert.Throws<ArgumentNullException>("seedSelector", () => Scrambled(3).AggregateBy(KeyOf, noSeedSelector, noFunc));
            Assert.Throws<ArgumentNullException>("seedSelector", () => Scrambled(3).Select(x => x).AggregateBy(KeyOf, noSeedSelector, Fold));
            Assert.Throws<ArgumentNullException>("func", () => Scrambled(3).AggregateBy(KeyOf, 0, noFunc));
            Assert.Throws<ArgumentNullException>("func", () => Iterate(Scrambled(3)).AggregateBy(KeyOf, SeedOf, noFunc));
            Assert.Throws<ArgumentNullException>("func", () => Scrambled(3).Select(x => x).AggregateBy(KeyOf, 0, noFunc));
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
        public void TheSourceIsDisposedOnceItIsFolded()
        {
            var tracked = new Tracked();

            using (var enumerator = tracked.Iterate(10).AggregateBy(x => x % 3, 0, Fold).GetEnumerator())
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(1, tracked.Disposed);
            }

            foreach (var _ in tracked.Iterate(10).AggregateBy(x => x % 3, SeedOf, Fold))
                break;

            Assert.Equal(2, tracked.Disposed);

            Assert.Equal(new KeyValuePair<int, int>(0, 18), tracked.Iterate(10).AggregateBy(x => x % 3, 0, (sum, x) => sum + x).First());
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).AggregateBy(x => x % 3, 0, (sum, x) => x < 5 ? sum + x : throw new InvalidOperationException()).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).AggregateBy(x => x, key => key < 5 ? key : throw new InvalidOperationException(), Fold).ToList());
            Assert.Equal(5, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(pullUntilTheFoldThrows);
            Assert.Equal(6, tracked.Disposed);

            void pullUntilTheFoldThrows()
            {
                foreach (var _ in tracked.Iterate(10).Select(x => x).AggregateBy(x => x % 3, SeedOf, (sum, x) => x < 5 ? sum + x : throw new InvalidOperationException())) { }
            }
        }
    }
}
