using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class MaxByKey
    {
        private const int Threw = int.MinValue;

        private static readonly IComparer<int> Descending = Comparer<int>.Create((x, y) => y.CompareTo(x));

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

        private static int Outcome(Func<int> call)
        {
            try
            {
                return call();
            }
            catch (InvalidOperationException)
            {
                return Threw;
            }
        }

        private static T Pulled<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            comparer ??= Comparer<TKey>.Default;

            var found = false;
            T value = default;
            TKey key = default;

            foreach (var item in source)
            {
                var nextKey = keySelector(item);
                if (!found)
                {
                    found = true;
                    value = item;
                    key = nextKey;
                }
                else if (nextKey != null
                    && (key == null || comparer.Compare(nextKey, key) > 0))
                {
                    value = item;
                    key = nextKey;
                }
            }

            if (!found
                && default(T) != null)
            {
                throw new InvalidOperationException();
            }

            return value;
        }

        private static int LastDigit(int x) => x % 10;

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [10], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeAnswersTheMaximum(int length)
        {
            var array = Scrambled(length);
            var list = new List<int>(array);
            ReadOnlyMemory<int> memory = array;
            IEnumerable<int> enumerable = Iterate(array);

            foreach (var comparer in new[] { null, Comparer<int>.Default, Descending })
            {
                var expected = Outcome(() => Pulled(array, LastDigit, comparer));

                Assert.Equal(expected, Outcome(() => array.MaxBy(LastDigit, comparer)));
                Assert.Equal(expected, Outcome(() => list.MaxBy(LastDigit, comparer)));
                Assert.Equal(expected, Outcome(() => memory.MaxBy(LastDigit, comparer)));
                Assert.Equal(expected, Outcome(() => enumerable.MaxBy(LastDigit, comparer)));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<int>)array).MaxBy(LastDigit, comparer)));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<int>)list).MaxBy(LastDigit, comparer)));
            }

            var withoutComparer = Outcome(() => Pulled(array, LastDigit, Comparer<int>.Default));

            Assert.Equal(withoutComparer, Outcome(() => array.MaxBy(LastDigit)));
            Assert.Equal(withoutComparer, Outcome(() => list.MaxBy(LastDigit)));
            Assert.Equal(withoutComparer, Outcome(() => memory.MaxBy(LastDigit)));
            Assert.Equal(withoutComparer, Outcome(() => enumerable.MaxBy(LastDigit)));
            Assert.Equal(withoutComparer, Outcome(() => array.Select(x => x).MaxBy(LastDigit)));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ValueEnumerablePushAgreesWithPull(int length)
        {
            var array = Scrambled(length);

            foreach (var comparer in new[] { null, Descending })
            {
                var selected = array.Select(x => x * 3);
                Assert.Equal(Outcome(() => Pulled(selected, LastDigit, comparer)), Outcome(() => selected.MaxBy(LastDigit, comparer)));

                var filtered = array.Where(x => x % 5 != 4);
                Assert.Equal(Outcome(() => Pulled(filtered, LastDigit, comparer)), Outcome(() => filtered.MaxBy(LastDigit, comparer)));

                var iterated = Iterate(array).Select(x => x + 1).Where(x => x % 2 == 0);
                Assert.Equal(Outcome(() => Pulled(iterated, LastDigit, comparer)), Outcome(() => iterated.MaxBy(LastDigit, comparer)));

                var ranged = Enumerable.Range(5, length);
                Assert.Equal(Outcome(() => Pulled(ranged, LastDigit, comparer)), Outcome(() => ranged.MaxBy(LastDigit, comparer)));

                var ordered = Scrambled(length).OrderBy(x => x);
                Assert.Equal(Outcome(() => Pulled(ordered, LastDigit, comparer)), Outcome(() => ordered.MaxBy(LastDigit, comparer)));

                var concatenated = array.Select(x => x).Concat(Scrambled(length / 2).Select(x => x - 7));
                Assert.Equal(Outcome(() => Pulled(concatenated, LastDigit, comparer)), Outcome(() => concatenated.MaxBy(LastDigit, comparer)));
            }
        }

        [Fact]
        public void TiesKeepTheFirstElement()
        {
            var items = new[] { 41, 23, 3, 13, 31, 33 };

            Assert.Equal(23, items.MaxBy(LastDigit));
            Assert.Equal(23, Iterate(items).MaxBy(LastDigit));
            Assert.Equal(23, items.Select(x => x).MaxBy(LastDigit));
            Assert.Equal(23, new List<int>(items).MaxBy(LastDigit));
            Assert.Equal(23, new[] { 41, 23, 3 }.Select(x => x).Concat(new[] { 13, 31, 33 }.Select(x => x)).MaxBy(LastDigit));

            var allTie = Comparer<int>.Create((_, _) => 0);

            Assert.Equal(41, items.MaxBy(LastDigit, allTie));
            Assert.Equal(41, Iterate(items).MaxBy(LastDigit, allTie));
            Assert.Equal(3, Array.Empty<int>().Select(x => x).Concat(new[] { 3, 13 }.Select(x => x)).MaxBy(LastDigit, allTie));
        }

        [Fact]
        public void KeySelectorAndComparerAreCalledAsSystemLinqCallsThem()
        {
            var items = new[] { 5, 2, 8, 1, 9 };
            var selected = new List<int>();
            var compared = new List<(int, int)>();
            var recordingComparer = Comparer<int>.Create(recordingCompare);

            (int, int)[] expected = [(20, 50), (80, 50), (10, 80), (90, 80)];

            check(() => items.MaxBy(recordingKey, recordingComparer));
            check(() => Iterate(items).MaxBy(recordingKey, recordingComparer));
            check(() => items.Select(x => x).MaxBy(recordingKey, recordingComparer));
            check(() => new List<int>(items).MaxBy(recordingKey, recordingComparer));
            check(() => new[] { 5, 2 }.Select(x => x).Concat(new[] { 8, 1, 9 }.Select(x => x)).MaxBy(recordingKey, recordingComparer));
            check(() => Array.Empty<int>().Select(x => x).Concat(new[] { 5, 2, 8, 1, 9 }.Select(x => x)).MaxBy(recordingKey, recordingComparer));

            int recordingKey(int x)
            {
                selected.Add(x);

                return x * 10;
            }

            int recordingCompare(int x, int y)
            {
                compared.Add((x, y));

                return x.CompareTo(y);
            }

            void check(Func<int> call)
            {
                selected.Clear();
                compared.Clear();

                Assert.Equal(9, call());
                Assert.Equal(items, selected);
                Assert.Equal(expected, compared);
            }
        }

        [Fact]
        public void NullKeysAreSkippedAndNeverCompared()
        {
            var strict = Comparer<string>.Create(strictCompare);
            var items = new[] { 3, 9, 5, 6, 8, 2, 12 };

            Assert.Equal(5, items.MaxBy(keyOf, strict));
            Assert.Equal(5, Iterate(items).MaxBy(keyOf, strict));
            Assert.Equal(5, items.Select(x => x).MaxBy(keyOf, strict));
            Assert.Equal(5, new[] { 3, 9 }.Select(x => x).Concat(new[] { 5, 6, 8, 2, 12 }.Select(x => x)).MaxBy(keyOf, strict));

            var allNull = new[] { 3, 6, 9 };

            Assert.Equal(3, allNull.MaxBy(keyOf, strict));
            Assert.Equal(3, Iterate(allNull).MaxBy(keyOf, strict));
            Assert.Equal(3, allNull.Select(x => x).MaxBy(keyOf, strict));
            Assert.Equal(3, new[] { 3 }.Select(x => x).Concat(new[] { 6, 9 }.Select(x => x)).MaxBy(keyOf, strict));

            var strictNullable = Comparer<int?>.Create(strictNullableCompare);

            int?[] nullables = [null, 4, null, 7, 2];

            Assert.Equal(7, nullables.MaxBy(x => x, strictNullable));
            Assert.Equal(7, Iterate(nullables).MaxBy(x => x, strictNullable));
            Assert.Null(new int?[] { null, null }.MaxBy(x => x, strictNullable));

            static int strictCompare(string x, string y)
            {
                Assert.NotNull(x);
                Assert.NotNull(y);

                return string.CompareOrdinal(x, y);
            }

            static string keyOf(int x)
            {
                string key =
                    x % 3 == 0
                        ? null
                        : $"{x % 7}";

                return key;
            }

            static int strictNullableCompare(int? x, int? y)
            {
                Assert.True(x.HasValue);
                Assert.True(y.HasValue);

                return x.Value.CompareTo(y.Value);
            }
        }

        [Fact]
        public void EmptySources()
        {
            Assert.Throws<InvalidOperationException>(() => Array.Empty<int>().MaxBy(LastDigit));
            Assert.Throws<InvalidOperationException>(() => new List<int>().MaxBy(LastDigit, Descending));
            Assert.Throws<InvalidOperationException>(() => ReadOnlyMemory<int>.Empty.MaxBy(LastDigit));
            Assert.Throws<InvalidOperationException>(() => Iterate(Array.Empty<int>()).MaxBy(LastDigit));
            Assert.Throws<InvalidOperationException>(() => Enumerable.Range(0, 0).MaxBy(LastDigit));
            Assert.Throws<InvalidOperationException>(() => Scrambled(10).Where(x => x < 0).MaxBy(LastDigit, comparer: null));

            Assert.Null(Array.Empty<string>().MaxBy(s => s.Length));
            Assert.Null(Iterate(Array.Empty<string>()).MaxBy(s => s.Length));
            Assert.Null(Array.Empty<int?>().Select(x => x).MaxBy(x => x));
        }

        [Fact]
        public void NullArgumentsThrow()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null).MaxBy(LastDigit));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null).MaxBy(LastDigit, Descending));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).MaxBy(LastDigit));

            Assert.Throws<ArgumentNullException>("keySelector", () => Array.Empty<int>().MaxBy((Func<int, int>)null));
            Assert.Throws<ArgumentNullException>("keySelector", () => Iterate(Array.Empty<int>()).MaxBy((Func<int, int>)null, Descending));
            Assert.Throws<ArgumentNullException>("keySelector", () => Enumerable.Range(0, 0).MaxBy((Func<int, int>)null));
            Assert.Throws<ArgumentNullException>("keySelector", () => Enumerable.Range(0, 3).Select(x => x).MaxBy((Func<int, int>)null, Descending));
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
        public void SourceIsDisposedWhenTheKeySelectorThrows()
        {
            var tracked = new Tracked();

            Assert.Throws<DivideByZeroException>(() => tracked.Iterate(10).MaxBy(throwing));
            Assert.Equal(1, tracked.Disposed);

            Assert.Throws<DivideByZeroException>(() => tracked.Iterate(10).Select(x => x).MaxBy(throwing));
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal(2, tracked.Iterate(3).MaxBy(throwing));
            Assert.Equal(3, tracked.Disposed);

            static int throwing(int x)
            {
                if (x == 4)
                    throw new DivideByZeroException();

                return x;
            }
        }
    }
}
