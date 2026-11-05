using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class MaxWithComparer
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
                else if (value == null || (item != null && comparer.Compare(item, value) > 0))
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
        public void EveryCollectionShapeAnswersTheMaximum(int length)
        {
            var array = Scrambled(length);
            var list = new List<int>(array);
            ReadOnlyMemory<int> memory = array;
            IEnumerable<int> enumerable = Iterate(array);

            foreach (var comparer in new[] { null, Comparer<int>.Default, Descending, ByLastDigit })
            {
                var expected = Outcome(() => Pulled(array, comparer));
                Assert.Equal(expected, Outcome(() => array.Max(comparer)));
                Assert.Equal(expected, Outcome(() => list.Max(comparer)));
                Assert.Equal(expected, Outcome(() => memory.Max(comparer)));
                Assert.Equal(expected, Outcome(() => enumerable.Max(comparer)));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<int>)array).Max(comparer)));
                Assert.Equal(expected, Outcome(() => ((IEnumerable<int>)list).Max(comparer)));
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
                Assert.Equal(Outcome(() => Pulled(selected, comparer)), Outcome(() => selected.Max(comparer)));

                var filtered = array.Where(x => x % 5 != 4);
                Assert.Equal(Outcome(() => Pulled(filtered, comparer)), Outcome(() => filtered.Max(comparer)));

                var iterated = Iterate(array).Select(x => x + 1).Where(x => x % 2 == 0);
                Assert.Equal(Outcome(() => Pulled(iterated, comparer)), Outcome(() => iterated.Max(comparer)));

                var ranged = Enumerable.Range(5, length);
                Assert.Equal(Outcome(() => Pulled(ranged, comparer)), Outcome(() => ranged.Max(comparer)));

                var ordered = Scrambled(length).OrderBy(x => x);
                Assert.Equal(Outcome(() => Pulled(ordered, comparer)), Outcome(() => ordered.Max(comparer)));

                var concatenated = array.Select(x => x).Concat(Scrambled(length / 2).Select(x => x + 7));
                Assert.Equal(Outcome(() => Pulled(concatenated, comparer)), Outcome(() => concatenated.Max(comparer)));
            }
        }

        [Fact]
        public void TiesKeepTheFirstElement()
        {
            var items = new[] { 21, 43, 3, 13, 31, 33 };

            Assert.Equal(43, items.Max(ByLastDigit));
            Assert.Equal(43, Iterate(items).Max(ByLastDigit));
            Assert.Equal(43, items.Select(x => x).Max(ByLastDigit));
            Assert.Equal(43, new List<int>(items).Max(ByLastDigit));
            Assert.Equal(43, new[] { 21, 43, 3 }.Select(x => x).Concat(new[] { 13, 31, 33 }.Select(x => x)).Max(ByLastDigit));

            var allTie = Comparer<int>.Create((_, _) => 0);
            Assert.Equal(21, items.Max(allTie));
            Assert.Equal(21, Iterate(items).Max(allTie));
            Assert.Equal(21, new[] { 21, 43 }.Select(x => x).Concat(new[] { 3, 13 }.Select(x => x)).Max(allTie));
            Assert.Equal(3, Array.Empty<int>().Select(x => x).Concat(new[] { 3, 13 }.Select(x => x)).Max(allTie));
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

            (int, int)[] expected = [(2, 5), (8, 5), (1, 8), (9, 8)];

            Assert.Equal(9, items.Max(recording));
            Assert.Equal(expected, calls);

            calls.Clear();
            Assert.Equal(9, Iterate(items).Max(recording));
            Assert.Equal(expected, calls);

            calls.Clear();
            Assert.Equal(9, items.Select(x => x).Max(recording));
            Assert.Equal(expected, calls);

            calls.Clear();
            Assert.Equal(9, new[] { 5, 2 }.Select(x => x).Concat(new[] { 8, 1, 9 }.Select(x => x)).Max(recording));
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
            Assert.Equal("x", strings.Max(strict));
            Assert.Equal("x", Iterate(strings).Max(strict));
            Assert.Equal("x", strings.Select(s => s).Max(strict));
            Assert.Equal("x", new List<string>(strings).Max(strict));
            Assert.Null(new string[] { null, null }.Max(strict));
            Assert.Null(new string[] { null, null }.Select(s => s).Max(strict));
            Assert.Null(Array.Empty<string>().Max(strict));
            Assert.Null(Iterate(Array.Empty<string>()).Max(strict));

            var strictInt = Comparer<int?>.Create((x, y) =>
            {
                Assert.True(x.HasValue && y.HasValue);
                return x!.Value.CompareTo(y!.Value);
            });

            int?[] ints = [null, 4, null, 2, 7];
            Assert.Equal(7, ints.Max(strictInt));
            Assert.Equal(7, Iterate(ints).Max(strictInt));
            Assert.Equal(7, ints.Select(x => x).Max(strictInt));
            Assert.Null(new int?[] { null }.Max(strictInt));
            Assert.Null(Array.Empty<int?>().Max(strictInt));
            Assert.Null(Array.Empty<int?>().Select(x => x).Max(strictInt));
        }

        [Fact]
        public void EmptyNonNullableSourceThrows()
        {
            Assert.Throws<InvalidOperationException>(() => Array.Empty<int>().Max(Descending));
            Assert.Throws<InvalidOperationException>(() => new List<int>().Max(Descending));
            Assert.Throws<InvalidOperationException>(() => ReadOnlyMemory<int>.Empty.Max(Descending));
            Assert.Throws<InvalidOperationException>(() => Iterate(Array.Empty<int>()).Max(Descending));
            Assert.Throws<InvalidOperationException>(() => Enumerable.Range(0, 0).Max(Descending));
            Assert.Throws<InvalidOperationException>(() => Scrambled(10).Where(x => x < 0).Max(comparer: null));
        }

        [Fact]
        public void DefaultComparerOrdersNaNLowest()
        {
            double[] items = [double.NaN, 3.0, double.NaN, -1.0, double.PositiveInfinity];
            Assert.Equal(double.PositiveInfinity, items.Max(comparer: null));
            Assert.Equal(double.PositiveInfinity, Iterate(items).Max(Comparer<double>.Default));
            Assert.Equal(double.PositiveInfinity, items.Select(x => x).Max(comparer: null));
            Assert.Equal(-1.0, new[] { double.NaN, -1.0 }.Max(comparer: null));
            Assert.Equal(double.NaN, new[] { double.NaN, double.NaN }.Max(comparer: null));
            Assert.Equal(double.NaN, new[] { double.NaN, double.NaN }.Select(x => x).Max(comparer: null));
        }

        [Fact]
        public void NullSourceThrows()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null!).Max(Descending));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null!).Max(Descending));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).Max(comparer: null));
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

            Assert.Throws<DivideByZeroException>(() => tracked.Iterate(10).Max(throwing));
            Assert.Equal(1, tracked.Disposed);

            Assert.Throws<DivideByZeroException>(() => tracked.Iterate(10).Select(x => x).Max(throwing));
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal(0, tracked.Iterate(3).Max(throwing));
            Assert.Equal(3, tracked.Disposed);
        }
    }
}
