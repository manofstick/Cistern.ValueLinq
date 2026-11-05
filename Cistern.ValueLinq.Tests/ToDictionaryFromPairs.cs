using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class ToDictionaryFromPairs
    {
        private static readonly IEqualityComparer<int> ByThousands = EqualityComparer<int>.Create((x, y) => x % 1000 == y % 1000, x => x % 1000);

        private static int[] Scrambled(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = (i * 37 + 11) % 101 + i / 101 * 1000;

            return items;
        }

        private static IEnumerable<T> Iterate<T>(T[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        private static KeyValuePair<int, string> PairOf(int x) => new(x, $"v{x}");

        private static (int Key, string Value) TupleOf(int x) => (x, $"v{x}");

        private static KeyValuePair<int, string>[] Pairs(int[] items)
        {
            var pairs = new KeyValuePair<int, string>[items.Length];
            for (var i = 0; i < items.Length; ++i)
                pairs[i] = PairOf(items[i]);

            return pairs;
        }

        private static (int Key, string Value)[] Tuples(int[] items)
        {
            var tuples = new (int Key, string Value)[items.Length];
            for (var i = 0; i < items.Length; ++i)
                tuples[i] = TupleOf(items[i]);

            return tuples;
        }

        private static string Text(Dictionary<int, string> dictionary)
        {
            var entries = new List<string>();
            foreach (var (key, value) in dictionary)
                entries.Add($"{key}:{value}");

            var text = string.Join(",", entries);

            return text;
        }

        private static string Outcome(Func<Dictionary<int, string>> call, IEqualityComparer<int> comparer)
        {
            try
            {
                var dictionary = call();

                Assert.Same(comparer ?? EqualityComparer<int>.Default, dictionary.Comparer);

                return Text(dictionary);
            }
            catch (ArgumentException exception)
            {
                return exception.GetType().Name;
            }
        }

        private static string Pulled(IEnumerable<KeyValuePair<int, string>> source, IEqualityComparer<int> comparer)
        {
            try
            {
                var dictionary = new Dictionary<int, string>(comparer);
                foreach (var (key, value) in source)
                    dictionary.Add(key, value);

                return Text(dictionary);
            }
            catch (ArgumentException exception)
            {
                return exception.GetType().Name;
            }
        }

        private static string Pulled(IEnumerable<(int Key, string Value)> source, IEqualityComparer<int> comparer)
        {
            try
            {
                var dictionary = new Dictionary<int, string>(comparer);
                foreach (var (key, value) in source)
                    dictionary.Add(key, value);

                return Text(dictionary);
            }
            catch (ArgumentException exception)
            {
                return exception.GetType().Name;
            }
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [102], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeAnswersTheDictionaryFromKeyValuePairs(int length)
        {
            var items = Scrambled(length);
            var pairs = Pairs(items);
            var list = new List<KeyValuePair<int, string>>(pairs);
            ReadOnlyMemory<KeyValuePair<int, string>> memory = pairs;
            IEnumerable<KeyValuePair<int, string>> enumerable = Iterate(pairs);

            foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByThousands })
            {
                var expected = Pulled(pairs, comparer);

                Assert.Equal(expected, Outcome(() => pairs.ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => list.ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => memory.ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => enumerable.ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<KeyValuePair<int, string>>)pairs).ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<KeyValuePair<int, string>>)list).ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => items.Select(PairOf).ToDictionary(comparer), comparer));
            }

            var withoutComparer = Pulled(pairs, null);

            Assert.Equal(withoutComparer, Outcome(() => pairs.ToDictionary(), null));
            Assert.Equal(withoutComparer, Outcome(() => list.ToDictionary(), null));
            Assert.Equal(withoutComparer, Outcome(() => memory.ToDictionary(), null));
            Assert.Equal(withoutComparer, Outcome(() => enumerable.ToDictionary(), null));
            Assert.Equal(withoutComparer, Outcome(() => items.Select(PairOf).ToDictionary(), null));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeAnswersTheDictionaryFromTuples(int length)
        {
            var items = Scrambled(length);
            var tuples = Tuples(items);
            var list = new List<(int Key, string Value)>(tuples);
            ReadOnlyMemory<(int Key, string Value)> memory = tuples;
            IEnumerable<(int Key, string Value)> enumerable = Iterate(tuples);

            foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByThousands })
            {
                var expected = Pulled(tuples, comparer);

                Assert.Equal(expected, Outcome(() => tuples.ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => list.ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => memory.ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => enumerable.ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<(int Key, string Value)>)tuples).ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<(int Key, string Value)>)list).ToDictionary(comparer), comparer));
                Assert.Equal(expected, Outcome(() => items.Select(TupleOf).ToDictionary(comparer), comparer));
            }

            var withoutComparer = Pulled(tuples, null);

            Assert.Equal(withoutComparer, Outcome(() => tuples.ToDictionary(), null));
            Assert.Equal(withoutComparer, Outcome(() => list.ToDictionary(), null));
            Assert.Equal(withoutComparer, Outcome(() => memory.ToDictionary(), null));
            Assert.Equal(withoutComparer, Outcome(() => enumerable.ToDictionary(), null));
            Assert.Equal(withoutComparer, Outcome(() => items.Select(TupleOf).ToDictionary(), null));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ValueEnumerablePushAgreesWithPull(int length)
        {
            var items = Scrambled(length);

            foreach (var comparer in new[] { null, ByThousands })
            {
                var filtered = items.Where(x => x % 5 != 4).Select(PairOf);
                Assert.Equal(Pulled(filtered, comparer), Outcome(() => filtered.ToDictionary(comparer), comparer));

                var iterated = Iterate(items).Select(x => x + 1).Where(x => x % 2 == 0).Select(TupleOf);
                Assert.Equal(Pulled(iterated, comparer), Outcome(() => iterated.ToDictionary(comparer), comparer));

                var ranged = Enumerable.Range(5, length).Select(PairOf);
                Assert.Equal(Pulled(ranged, comparer), Outcome(() => ranged.ToDictionary(comparer), comparer));

                var ordered = Scrambled(length).OrderBy(x => x).Select(TupleOf);
                Assert.Equal(Pulled(ordered, comparer), Outcome(() => ordered.ToDictionary(comparer), comparer));

                var concatenated = Pairs(items).Select(x => x).Concat(Pairs(Scrambled(length / 2)).Select(x => new KeyValuePair<int, string>(x.Key + 5000, x.Value)));
                Assert.Equal(Pulled(concatenated, comparer), Outcome(() => concatenated.ToDictionary(comparer), comparer));

                var spans = Pairs(items).AsEnumerable().Concat(Pairs(Scrambled(length / 2)));
                Assert.Equal(Pulled(spans, comparer), Outcome(() => spans.ToDictionary(comparer), comparer));
            }
        }

        [Fact]
        public void DuplicateKeysThrowOnEveryPath()
        {
            var pairs = Pairs([3, 8, 5, 8, 1]);
            var tuples = Tuples([3, 8, 5, 8, 1]);

            Assert.Throws<ArgumentException>(() => pairs.ToDictionary());
            Assert.Throws<ArgumentException>(() => Iterate(pairs).ToDictionary());
            Assert.Throws<ArgumentException>(() => pairs.Select(x => x).ToDictionary());
            Assert.Throws<ArgumentException>(() => Pairs([3, 8]).Select(x => x).Concat(Pairs([5, 8, 1]).Select(x => x)).ToDictionary());
            Assert.Throws<ArgumentException>(() => Pairs([3, 8]).AsEnumerable().Concat(Pairs([5, 8, 1])).ToDictionary());
            Assert.Throws<ArgumentException>(() => tuples.ToDictionary());
            Assert.Throws<ArgumentException>(() => Iterate(tuples).ToDictionary());
            Assert.Throws<ArgumentException>(() => tuples.Select(x => x).ToDictionary());

            var byParity = EqualityComparer<int>.Create((x, y) => x % 2 == y % 2, x => x % 2);

            Assert.Throws<ArgumentException>(() => Pairs([1, 2, 3]).ToDictionary(byParity));
            Assert.Throws<ArgumentException>(() => Iterate(Tuples([1, 2, 3])).ToDictionary(byParity));
        }

        [Fact]
        public void NullKeysThrow()
        {
            KeyValuePair<string, int>[] pairs = [new("a", 1), new(null, 2)];
            (string Key, int Value)[] tuples = [("a", 1), (null, 2)];

            Assert.Throws<ArgumentNullException>("key", () => pairs.ToDictionary());
            Assert.Throws<ArgumentNullException>("key", () => Iterate(pairs).ToDictionary());
            Assert.Throws<ArgumentNullException>("key", () => pairs.Select(x => x).ToDictionary());
            Assert.Throws<ArgumentNullException>("key", () => tuples.ToDictionary());
            Assert.Throws<ArgumentNullException>("key", () => Iterate(tuples).ToDictionary());
            Assert.Throws<ArgumentNullException>("key", () => tuples.Select(x => x).ToDictionary());
        }

        [Fact]
        public void NullSourcesThrow()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((KeyValuePair<int, string>[])null).ToDictionary());
            Assert.Throws<ArgumentNullException>("source", () => ((List<KeyValuePair<int, string>>)null).ToDictionary(ByThousands));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<KeyValuePair<int, string>>)null).ToDictionary());
            Assert.Throws<ArgumentNullException>("source", () => (((int Key, string Value)[])null).ToDictionary(ByThousands));
            Assert.Throws<ArgumentNullException>("source", () => ((List<(int Key, string Value)>)null).ToDictionary());
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<(int Key, string Value)>)null).ToDictionary(ByThousands));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ASpanSizesTheDictionaryToItsLength(int length)
        {
            var items = new int[length];
            for (var i = 0; i < length; ++i)
                items[i] = i;

            var expected = new Dictionary<int, string>(length).Capacity;
            var pairs = Pairs(items);
            var tuples = Tuples(items);

            Assert.Equal(expected, pairs.ToDictionary().Capacity);
            Assert.Equal(expected, new List<KeyValuePair<int, string>>(pairs).ToDictionary().Capacity);
            Assert.Equal(expected, new ReadOnlyMemory<KeyValuePair<int, string>>(pairs).ToDictionary().Capacity);
            Assert.Equal(expected, tuples.ToDictionary().Capacity);
            Assert.Equal(expected, new List<(int Key, string Value)>(tuples).ToDictionary().Capacity);
        }

        private sealed class Tracked
        {
            public int Disposed;

            public IEnumerable<KeyValuePair<int, string>> Iterate(int count)
            {
                try
                {
                    for (var i = 0; i < count; ++i)
                        yield return PairOf(i % 4);
                }
                finally
                {
                    ++Disposed;
                }
            }
        }

        [Fact]
        public void SourceIsDisposedWhenAKeyRepeats()
        {
            var tracked = new Tracked();

            Assert.Throws<ArgumentException>(() => tracked.Iterate(10).ToDictionary());
            Assert.Equal(1, tracked.Disposed);

            Assert.Throws<ArgumentException>(() => tracked.Iterate(10).Select(x => x).ToDictionary());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal(3, tracked.Iterate(3).ToDictionary().Count);
            Assert.Equal(3, tracked.Disposed);
        }
    }
}
