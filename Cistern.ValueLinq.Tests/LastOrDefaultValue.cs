using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class LastOrDefaultValue
    {
        private const int Missing = -1;

        private static int[] Sequence(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = i;

            return items;
        }

        private static IEnumerable<int> Iterate(int count)
        {
            for (var i = 0; i < count; ++i)
                yield return i;
        }

        private static T Pulled<T>(IEnumerable<T> source, T defaultValue)
        {
            var last = defaultValue;
            foreach (var item in source)
                last = item;

            return last;
        }

        private static T Pulled<T>(IEnumerable<T> source, Func<T, bool> predicate, T defaultValue)
        {
            var last = defaultValue;
            foreach (var item in source)
            {
                if (predicate(item))
                    last = item;
            }

            return last;
        }

        private static bool UnderFive(int x) => x < 5;

        public static IEnumerable<object[]> Lengths() => [[0], [1], [6], [10], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeAnswersItsLastElement(int length)
        {
            var array = Sequence(length);
            var list = new List<int>(array);
            ReadOnlyMemory<int> memory = array;
            IEnumerable<int> enumerable = Iterate(length);

            var last = length == 0 ? Missing : length - 1;
            Assert.Equal(last, array.LastOrDefault(Missing));
            Assert.Equal(last, list.LastOrDefault(Missing));
            Assert.Equal(last, memory.LastOrDefault(Missing));
            Assert.Equal(last, enumerable.LastOrDefault(Missing));
            Assert.Equal(last, ((IEnumerable<int>)array).LastOrDefault(Missing));
            Assert.Equal(last, ((IEnumerable<int>)list).LastOrDefault(Missing));

            var match = length == 0 ? Missing : Math.Min(length - 1, 4);
            Assert.Equal(match, array.LastOrDefault(UnderFive, Missing));
            Assert.Equal(match, list.LastOrDefault(UnderFive, Missing));
            Assert.Equal(match, memory.LastOrDefault(UnderFive, Missing));
            Assert.Equal(match, enumerable.LastOrDefault(UnderFive, Missing));
            Assert.Equal(match, ((IEnumerable<int>)array).LastOrDefault(UnderFive, Missing));
            Assert.Equal(match, ((IEnumerable<int>)list).LastOrDefault(UnderFive, Missing));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ValueEnumerablePushAgreesWithPull(int length)
        {
            var array = Sequence(length);

            var selected = array.Select(x => x * 3);
            Assert.Equal(Pulled(selected, Missing), selected.LastOrDefault(Missing));
            Assert.Equal(Pulled(selected, UnderFive, Missing), selected.LastOrDefault(UnderFive, Missing));

            var filtered = array.Where(x => x % 3 == 2);
            Assert.Equal(Pulled(filtered, Missing), filtered.LastOrDefault(Missing));
            Assert.Equal(Pulled(filtered, UnderFive, Missing), filtered.LastOrDefault(UnderFive, Missing));

            var filteredSelected = array.Where(x => x % 3 == 2).Select(x => x - 1);
            Assert.Equal(Pulled(filteredSelected, Missing), filteredSelected.LastOrDefault(Missing));
            Assert.Equal(Pulled(filteredSelected, UnderFive, Missing), filteredSelected.LastOrDefault(UnderFive, Missing));

            var iterated = Iterate(length).Select(x => x + 1).Where(x => x % 2 == 0);
            Assert.Equal(Pulled(iterated, Missing), iterated.LastOrDefault(Missing));
            Assert.Equal(Pulled(iterated, UnderFive, Missing), iterated.LastOrDefault(UnderFive, Missing));

            var concatenatedBeforeEmptyPart = array.Select(x => x + 10).Concat(Array.Empty<int>().Select(x => x));
            Assert.Equal(Pulled(concatenatedBeforeEmptyPart, Missing), concatenatedBeforeEmptyPart.LastOrDefault(Missing));
            Assert.Equal(Pulled(concatenatedBeforeEmptyPart, x => x < 15, Missing), concatenatedBeforeEmptyPart.LastOrDefault(x => x < 15, Missing));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestAnswersMatchRunningTheSequence(int length)
        {
            var range = Enumerable.Range(5, length);
            Assert.Equal(Pulled(range, Missing), range.LastOrDefault(Missing));
            Assert.Equal(Pulled(range, x => x < 8, Missing), range.LastOrDefault(x => x < 8, Missing));

            var skipped = Sequence(length).Select(x => x * 2).Skip(3);
            Assert.Equal(Pulled(skipped, Missing), skipped.LastOrDefault(Missing));
            Assert.Equal(Pulled(skipped, UnderFive, Missing), skipped.LastOrDefault(UnderFive, Missing));

            var ordered = Sequence(length).OrderByDescending(x => x);
            Assert.Equal(Pulled(ordered, Missing), ordered.LastOrDefault(Missing));
            Assert.Equal(Pulled(ordered, UnderFive, Missing), ordered.LastOrDefault(UnderFive, Missing));

            var reversed = Sequence(length).Reverse();
            Assert.Equal(Pulled(reversed, Missing), reversed.LastOrDefault(Missing));
            Assert.Equal(Pulled(reversed, UnderFive, Missing), reversed.LastOrDefault(UnderFive, Missing));

            var appended = Sequence(length).Append(3);
            Assert.Equal(Pulled(appended, Missing), appended.LastOrDefault(Missing));
            Assert.Equal(Pulled(appended, x => x > 3, Missing), appended.LastOrDefault(x => x > 3, Missing));
        }

        [Fact]
        public void PredicateRunsFromTheEndOnlyWhereTheSourceReverses()
        {
            var seen = new List<int>();
            bool Recording(int x)
            {
                seen.Add(x);
                return x < 5;
            }

            Assert.Equal(4, Sequence(10).LastOrDefault(Recording, Missing));
            Assert.Equal([9, 8, 7, 6, 5, 4], seen);

            seen.Clear();
            Assert.Equal(4, new List<int>(Sequence(10)).LastOrDefault(Recording, Missing));
            Assert.Equal([9, 8, 7, 6, 5, 4], seen);

            seen.Clear();
            Assert.Equal(Missing, Sequence(10).Select(x => x + 100).LastOrDefault(Recording, Missing));
            Assert.Equal(Sequence(10).Select(x => x + 100).ToArray(), seen.ToArray());

            seen.Clear();
            Assert.Equal(4, Sequence(10).Where(x => x % 2 == 0).LastOrDefault(Recording, Missing));
            Assert.Equal([0, 2, 4, 6, 8], seen);
        }

        [Fact]
        public void ReferenceDefaultValueIsReturnedAsGiven()
        {
            var fallback = "fallback";
            Assert.Same(fallback, Array.Empty<string>().LastOrDefault(fallback));
            Assert.Same(fallback, new List<string>().LastOrDefault(fallback));
            Assert.Same(fallback, new[] { "a", "b" }.LastOrDefault(s => s.Length > 1, fallback));
            Assert.Same(fallback, new List<string>().Select(s => s).LastOrDefault(fallback));
            Assert.Null(new[] { "a" }.Where(s => s.Length > 1).LastOrDefault((string)null));
            Assert.Equal("a", new[] { "a", "b" }.Select(s => s).LastOrDefault(s => s == "a", fallback));
        }

        [Fact]
        public void NullArgumentsThrow()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null!).LastOrDefault(Missing));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null!).LastOrDefault(Missing));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).LastOrDefault(Missing));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).LastOrDefault(UnderFive, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).LastOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => new List<int>(Sequence(3)).LastOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Iterate(3).LastOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).Select(x => x).LastOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Array.Empty<int>().LastOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Enumerable.Range(0, 0).LastOrDefault(null!, Missing));
        }

        private sealed class Tracked
        {
            public int Disposed;
            public int Yielded;

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
        public void SourceIsDisposedWhenThePushEnds()
        {
            var tracked = new Tracked();

            Assert.Equal(9, tracked.Iterate(10).LastOrDefault(Missing));
            Assert.Equal(1, tracked.Disposed);
            Assert.Equal(10, tracked.Yielded);

            Assert.Equal(4, tracked.Iterate(10).LastOrDefault(UnderFive, Missing));
            Assert.Equal(2, tracked.Disposed);
            Assert.Equal(20, tracked.Yielded);

            Assert.Equal(4, tracked.Iterate(10).Select(x => x * 2).LastOrDefault(UnderFive, Missing));
            Assert.Equal(3, tracked.Disposed);

            Assert.Equal(Missing, tracked.Iterate(0).LastOrDefault(Missing));
            Assert.Equal(4, tracked.Disposed);
        }

        [Fact]
        public void SourceIsDisposedWhenThePredicateThrows()
        {
            var tracked = new Tracked();

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).LastOrDefault(x => x == 4 ? throw new InvalidOperationException() : false, Missing));
            Assert.Equal(1, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Select(x => x).LastOrDefault(x => x == 4 ? throw new InvalidOperationException() : false, Missing));
            Assert.Equal(2, tracked.Disposed);
        }
    }
}
