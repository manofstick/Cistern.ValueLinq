using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class MinWithComparer
    {
        private const int Threw = int.MinValue;

        private static readonly IComparer<int> Descending = Comparer<int>.Create((x, y) => y.CompareTo(x));
        private static readonly IComparer<int> ByLastDigit = Comparer<int>.Create((x, y) => (x % 10).CompareTo(y % 10));

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

        private static T Pulled<T>(IEnumerable<T> source, IComparer<T> comparer)
        {
            comparer ??= Comparer<T>.Default;

            var found = false;
            T value = default;
            foreach (var item in source)
            {
                if (!found)
                {
                    found = true;
                    value = item;
                }
                else if (value == null || (item != null && comparer.Compare(item, value) < 0))
                {
                    value = item;
                }
            }

            if (!found && default(T) != null)
                throw new InvalidOperationException();

            return value;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [10], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeAnswersTheMinimum(int length)
        {
            var array = Scrambled(length);
            var list = new List<int>(array);
            ReadOnlyMemory<int> memory = array;
            IEnumerable<int> enumerable = Iterate(array);

            foreach (var comparer in new[] { null, Comparer<int>.Default, Descending, ByLastDigit })
            {
                var expected = Outcome(() => Pulled(array, comparer));
                Assert.Equal(expected, Outcome(() => array.Min(comparer)));
                Assert.Equal(expected, Outcome(() => list.Min(comparer)));
                Assert.Equal(expected, Outcome(() => memory.Min(comparer)));
                Assert.Equal(expected, Outcome(() => enumerable.Min(comparer)));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<int>)array).Min(comparer)));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<int>)list).Min(comparer)));
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ValueEnumerablePushAgreesWithPull(int length)
        {
            var array = Scrambled(length);

            foreach (var comparer in new[] { null, Descending, ByLastDigit })
            {
                var selected = array.Select(x => x * 3);
                Assert.Equal(Outcome(() => Pulled(selected, comparer)), Outcome(() => selected.Min(comparer)));

                var filtered = array.Where(x => x % 5 != 4);
                Assert.Equal(Outcome(() => Pulled(filtered, comparer)), Outcome(() => filtered.Min(comparer)));

                var iterated = Iterate(array).Select(x => x + 1).Where(x => x % 2 == 0);
                Assert.Equal(Outcome(() => Pulled(iterated, comparer)), Outcome(() => iterated.Min(comparer)));

                var ranged = Enumerable.Range(5, length);
                Assert.Equal(Outcome(() => Pulled(ranged, comparer)), Outcome(() => ranged.Min(comparer)));

                var ordered = Scrambled(length).OrderByDescending(x => x);
                Assert.Equal(Outcome(() => Pulled(ordered, comparer)), Outcome(() => ordered.Min(comparer)));

                var concatenated = array.Select(x => x).Concat(Scrambled(length / 2).Select(x => x - 7));
                Assert.Equal(Outcome(() => Pulled(concatenated, comparer)), Outcome(() => concatenated.Min(comparer)));
            }
        }

        [Fact]
        public void TiesKeepTheFirstElement()
        {
            var items = new[] { 23, 41, 3, 13, 31, 33 };

            Assert.Equal(41, items.Min(ByLastDigit));
            Assert.Equal(41, Iterate(items).Min(ByLastDigit));
            Assert.Equal(41, items.Select(x => x).Min(ByLastDigit));
            Assert.Equal(41, new List<int>(items).Min(ByLastDigit));
            Assert.Equal(41, new[] { 23, 41, 3 }.Select(x => x).Concat(new[] { 13, 31, 33 }.Select(x => x)).Min(ByLastDigit));

            var allTie = Comparer<int>.Create((_, _) => 0);
            Assert.Equal(23, items.Min(allTie));
            Assert.Equal(23, Iterate(items).Min(allTie));
            Assert.Equal(23, new[] { 23, 41 }.Select(x => x).Concat(new[] { 3, 13 }.Select(x => x)).Min(allTie));
            Assert.Equal(3, Array.Empty<int>().Select(x => x).Concat(new[] { 3, 13 }.Select(x => x)).Min(allTie));
        }

        [Fact]
        public void ComparerSeesTheNextElementFirst()
        {
            var items = new[] { 5, 2, 8, 1, 9 };
            var calls = new List<(int, int)>();
            var recording = Comparer<int>.Create((x, y) =>
            {
                calls.Add((x, y));
                return x.CompareTo(y);
            });

            (int, int)[] expected = [(2, 5), (8, 2), (1, 2), (9, 1)];

            Assert.Equal(1, items.Min(recording));
            Assert.Equal(expected, calls);

            calls.Clear();
            Assert.Equal(1, Iterate(items).Min(recording));
            Assert.Equal(expected, calls);

            calls.Clear();
            Assert.Equal(1, items.Select(x => x).Min(recording));
            Assert.Equal(expected, calls);

            calls.Clear();
            Assert.Equal(1, new[] { 5, 2 }.Select(x => x).Concat(new[] { 8, 1, 9 }.Select(x => x)).Min(recording));
            Assert.Equal(expected, calls);
        }

        [Fact]
        public void NullsAreNeverCompared()
        {
            var strict = Comparer<string>.Create((x, y) =>
            {
                Assert.NotNull(x);
                Assert.NotNull(y);
                return string.CompareOrdinal(x, y);
            });

            string[] strings = [null, "m", null, "c", "x", null];
            Assert.Equal("c", strings.Min(strict));
            Assert.Equal("c", Iterate(strings).Min(strict));
            Assert.Equal("c", strings.Select(s => s).Min(strict));
            Assert.Equal("c", new List<string>(strings).Min(strict));
            Assert.Null(new string[] { null, null }.Min(strict));
            Assert.Null(new string[] { null, null }.Select(s => s).Min(strict));
            Assert.Null(Array.Empty<string>().Min(strict));
            Assert.Null(Iterate(Array.Empty<string>()).Min(strict));

            var strictInt = Comparer<int?>.Create((x, y) =>
            {
                Assert.True(x.HasValue && y.HasValue);
                return x!.Value.CompareTo(y!.Value);
            });

            int?[] ints = [null, 4, null, 2, 7];
            Assert.Equal(2, ints.Min(strictInt));
            Assert.Equal(2, Iterate(ints).Min(strictInt));
            Assert.Equal(2, ints.Select(x => x).Min(strictInt));
            Assert.Null(new int?[] { null }.Min(strictInt));
            Assert.Null(Array.Empty<int?>().Min(strictInt));
            Assert.Null(Array.Empty<int?>().Select(x => x).Min(strictInt));
        }

        [Fact]
        public void EmptyNonNullableSourceThrows()
        {
            Assert.Throws<InvalidOperationException>(() => Array.Empty<int>().Min(Descending));
            Assert.Throws<InvalidOperationException>(() => new List<int>().Min(Descending));
            Assert.Throws<InvalidOperationException>(() => ReadOnlyMemory<int>.Empty.Min(Descending));
            Assert.Throws<InvalidOperationException>(() => Iterate(Array.Empty<int>()).Min(Descending));
            Assert.Throws<InvalidOperationException>(() => Enumerable.Range(0, 0).Min(Descending));
            Assert.Throws<InvalidOperationException>(() => Scrambled(10).Where(x => x < 0).Min(comparer: null));
        }

        [Fact]
        public void DefaultComparerOrdersNaNLowest()
        {
            double[] items = [3.0, double.NaN, -1.0, double.NegativeInfinity];
            Assert.Equal(double.NaN, items.Min(comparer: null));
            Assert.Equal(double.NaN, Iterate(items).Min(Comparer<double>.Default));
            Assert.Equal(double.NaN, items.Select(x => x).Min(comparer: null));
            Assert.Equal(double.NegativeInfinity, new[] { 3.0, -1.0, double.NegativeInfinity }.Min(comparer: null));
        }

        [Fact]
        public void NullSourceThrows()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null!).Min(Descending));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null!).Min(Descending));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).Min(comparer: null));
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
        public void SourceIsDisposedWhenTheComparerThrows()
        {
            var tracked = new Tracked();
            var throwing = Comparer<int>.Create((x, _) => x == 4 ? throw new DivideByZeroException() : 0);

            Assert.Throws<DivideByZeroException>(() => tracked.Iterate(10).Min(throwing));
            Assert.Equal(1, tracked.Disposed);

            Assert.Throws<DivideByZeroException>(() => tracked.Iterate(10).Select(x => x).Min(throwing));
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal(0, tracked.Iterate(3).Min(throwing));
            Assert.Equal(3, tracked.Disposed);
        }
    }
}
