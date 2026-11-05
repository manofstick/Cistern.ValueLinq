using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class FirstOrDefaultValue
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
            foreach (var item in source)
                return item;

            return defaultValue;
        }

        private static T Pulled<T>(IEnumerable<T> source, Func<T, bool> predicate, T defaultValue)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                    return item;
            }

            return defaultValue;
        }

        private static bool OverFive(int x) => x > 5;

        public static IEnumerable<object[]> Lengths() => [[0], [1], [6], [10], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeAnswersItsFirstElement(int length)
        {
            var array = Sequence(length);
            var list = new List<int>(array);
            ReadOnlyMemory<int> memory = array;
            IEnumerable<int> enumerable = Iterate(length);

            var first = length == 0 ? Missing : 0;
            Assert.Equal(first, array.FirstOrDefault(Missing));
            Assert.Equal(first, list.FirstOrDefault(Missing));
            Assert.Equal(first, memory.FirstOrDefault(Missing));
            Assert.Equal(first, enumerable.FirstOrDefault(Missing));
            Assert.Equal(first, ((IEnumerable<int>)array).FirstOrDefault(Missing));
            Assert.Equal(first, ((IEnumerable<int>)list).FirstOrDefault(Missing));

            var match = length > 6 ? 6 : Missing;
            Assert.Equal(match, array.FirstOrDefault(OverFive, Missing));
            Assert.Equal(match, list.FirstOrDefault(OverFive, Missing));
            Assert.Equal(match, memory.FirstOrDefault(OverFive, Missing));
            Assert.Equal(match, enumerable.FirstOrDefault(OverFive, Missing));
            Assert.Equal(match, ((IEnumerable<int>)array).FirstOrDefault(OverFive, Missing));
            Assert.Equal(match, ((IEnumerable<int>)list).FirstOrDefault(OverFive, Missing));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ValueEnumerablePushAgreesWithPull(int length)
        {
            var array = Sequence(length);

            var selected = array.Select(x => x * 3);
            Assert.Equal(Pulled(selected, Missing), selected.FirstOrDefault(Missing));
            Assert.Equal(Pulled(selected, OverFive, Missing), selected.FirstOrDefault(OverFive, Missing));

            var filtered = array.Where(x => x % 3 == 2);
            Assert.Equal(Pulled(filtered, Missing), filtered.FirstOrDefault(Missing));
            Assert.Equal(Pulled(filtered, OverFive, Missing), filtered.FirstOrDefault(OverFive, Missing));

            var iterated = Iterate(length).Select(x => x + 1).Where(x => x % 2 == 0);
            Assert.Equal(Pulled(iterated, Missing), iterated.FirstOrDefault(Missing));
            Assert.Equal(Pulled(iterated, OverFive, Missing), iterated.FirstOrDefault(OverFive, Missing));

            var concatenatedAfterEmptyPart = Array.Empty<int>().Select(x => x).Concat(array.Select(x => x + 10));
            Assert.Equal(Pulled(concatenatedAfterEmptyPart, Missing), concatenatedAfterEmptyPart.FirstOrDefault(Missing));
            Assert.Equal(Pulled(concatenatedAfterEmptyPart, OverFive, Missing), concatenatedAfterEmptyPart.FirstOrDefault(OverFive, Missing));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void TryElementAtAnswersMatchRunningTheSequence(int length)
        {
            var range = Enumerable.Range(5, length);
            Assert.Equal(Pulled(range, Missing), range.FirstOrDefault(Missing));

            var skipped = Sequence(length).Select(x => x * 2).Skip(3);
            Assert.Equal(Pulled(skipped, Missing), skipped.FirstOrDefault(Missing));

            var ordered = Sequence(length).OrderByDescending(x => x);
            Assert.Equal(Pulled(ordered, Missing), ordered.FirstOrDefault(Missing));
            Assert.Equal(Pulled(ordered, OverFive, Missing), ordered.FirstOrDefault(OverFive, Missing));

            var reversed = Sequence(length).Reverse();
            Assert.Equal(Pulled(reversed, Missing), reversed.FirstOrDefault(Missing));
            Assert.Equal(Pulled(reversed, OverFive, Missing), reversed.FirstOrDefault(OverFive, Missing));
        }

        [Fact]
        public void ReferenceDefaultValueIsReturnedAsGiven()
        {
            var fallback = "fallback";
            Assert.Same(fallback, Array.Empty<string>().FirstOrDefault(fallback));
            Assert.Same(fallback, new[] { "a", "b" }.FirstOrDefault(s => s.Length > 1, fallback));
            Assert.Same(fallback, new List<string>().Select(s => s).FirstOrDefault(fallback));
            Assert.Null(new[] { "a" }.Where(s => s.Length > 1).FirstOrDefault((string?)null));
            Assert.Equal("b", new[] { "a", "b" }.Select(s => s).FirstOrDefault(s => s == "b", fallback));
        }

        [Fact]
        public void NullArgumentsThrow()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null!).FirstOrDefault(Missing));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).FirstOrDefault(Missing));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).FirstOrDefault(OverFive, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).FirstOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => new List<int>(Sequence(3)).FirstOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Iterate(3).FirstOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).Select(x => x).FirstOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Array.Empty<int>().FirstOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Enumerable.Range(0, 0).FirstOrDefault(null!, Missing));

            Func<int, bool> noPredicate = null!;

            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).FirstOrDefault(noPredicate));
            Assert.Throws<ArgumentNullException>("predicate", () => new List<int>(Sequence(3)).FirstOrDefault(noPredicate));
            Assert.Throws<ArgumentNullException>("predicate", () => Iterate(3).FirstOrDefault(noPredicate));
            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).Select(x => x).FirstOrDefault(noPredicate));
            Assert.Throws<ArgumentNullException>("predicate", () => Array.Empty<int>().FirstOrDefault(noPredicate));
            Assert.Throws<ArgumentNullException>("predicate", () => Enumerable.Range(0, 0).FirstOrDefault(noPredicate));
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
        public void SourceIsDisposedWhenTheFirstElementStopsThePush()
        {
            var tracked = new Tracked();

            Assert.Equal(0, tracked.Iterate(10).FirstOrDefault(Missing));
            Assert.Equal(1, tracked.Disposed);
            Assert.Equal(1, tracked.Yielded);

            Assert.Equal(6, tracked.Iterate(10).FirstOrDefault(OverFive, Missing));
            Assert.Equal(2, tracked.Disposed);
            Assert.Equal(1 + 7, tracked.Yielded);

            Assert.Equal(6, tracked.Iterate(10).Select(x => x * 2).FirstOrDefault(OverFive, Missing));
            Assert.Equal(3, tracked.Disposed);

            Assert.Equal(Missing, tracked.Iterate(3).FirstOrDefault(OverFive, Missing));
            Assert.Equal(4, tracked.Disposed);
        }

        [Fact]
        public void SourceIsDisposedWhenThePredicateThrows()
        {
            var tracked = new Tracked();

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).FirstOrDefault(x => x == 4 ? throw new InvalidOperationException() : false, Missing));
            Assert.Equal(1, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Select(x => x).FirstOrDefault(x => x == 4 ? throw new InvalidOperationException() : false, Missing));
            Assert.Equal(2, tracked.Disposed);
        }
    }
}
