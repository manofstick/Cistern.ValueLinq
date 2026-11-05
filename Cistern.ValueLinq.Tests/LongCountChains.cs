using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class LongCountChains
    {
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

        private static long Pulled<T>(IEnumerable<T> source)
        {
            long count = 0;
            foreach (var _ in source)
                ++count;

            return count;
        }

        private static bool IsEven(int x) => x % 2 == 0;

        public static IEnumerable<object[]> Lengths() => [[0], [1], [10], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeCountsItsElements(int length)
        {
            var array = Sequence(length);
            var list = new List<int>(array);
            ReadOnlyMemory<int> memory = array;
            IEnumerable<int> enumerable = Iterate(length);

            Assert.Equal(length, array.LongCount());
            Assert.Equal(length, list.LongCount());
            Assert.Equal(length, memory.LongCount());
            Assert.Equal(length, enumerable.LongCount());
            Assert.Equal(length, ((IEnumerable<int>)array).LongCount());
            Assert.Equal(length, ((IEnumerable<int>)list).LongCount());

            var evens = (length + 1) / 2;
            Assert.Equal(evens, array.LongCount(IsEven));
            Assert.Equal(evens, list.LongCount(IsEven));
            Assert.Equal(evens, memory.LongCount(IsEven));
            Assert.Equal(evens, enumerable.LongCount(IsEven));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ValueEnumerablePushAgreesWithPull(int length)
        {
            var array = Sequence(length);

            var selected = array.Select(x => x * 3);
            Assert.Equal(Pulled(selected), selected.LongCount());
            Assert.Equal(Pulled(selected.Where(IsEven)), selected.LongCount(IsEven));

            var filtered = array.Where(x => x % 3 != 0);
            Assert.Equal(Pulled(filtered), filtered.LongCount());
            Assert.Equal(Pulled(filtered.Where(IsEven)), filtered.LongCount(IsEven));

            var iterated = Iterate(length).Select(x => x + 1).Where(IsEven);
            Assert.Equal(Pulled(iterated), iterated.LongCount());

            var range = Enumerable.Range(5, length);
            Assert.Equal(Pulled(range), range.LongCount());
            Assert.Equal(Pulled(range.Where(IsEven)), range.LongCount(IsEven));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void LongCountAgreesWithCount(int length)
        {
            var array = Sequence(length);

            Assert.Equal(array.Select(x => x - 1).Count(), array.Select(x => x - 1).LongCount());
            Assert.Equal(array.Where(IsEven).Count(), array.Where(IsEven).LongCount());
            Assert.Equal(Iterate(length).Count(x => x > 3), Iterate(length).LongCount(x => x > 3));
        }

        [Fact]
        public void SelectorRunsUnlessSideEffectsAreIgnored()
        {
            var array = Sequence(100);
            var calls = 0;
            var selected = array.Select(x => { ++calls; return x; });

            Assert.Equal(100L, selected.LongCount());
            Assert.Equal(100, calls);

            calls = 0;
            Assert.Equal(100L, selected.LongCount(ignorePotentialSideEffects: true));
            Assert.Equal(0, calls);
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ConcatAnswersMatchRunningIt(int length)
        {
            var array = Sequence(length);

            var bounded = array.Where(IsEven).Concat(array.Where(x => x % 3 == 0));
            Assert.Equal(Pulled(bounded), bounded.LongCount());

            var unbounded = Iterate(length).Concat(Iterate(length + 1));
            Assert.Equal(Pulled(unbounded), unbounded.LongCount());

            var appended = array.Where(IsEven).Append(-1).Prepend(-2);
            Assert.Equal(Pulled(appended), appended.LongCount());
        }

        [Fact]
        public void LengthBeyondIntIsCounted()
        {
            var repeated = Enumerable.Repeat(7, int.MaxValue).Concat(Enumerable.Repeat(7, int.MaxValue));

            Assert.Equal(2L * int.MaxValue, repeated.LongCount());
        }

        [Fact]
        public void NullArgumentsThrow()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null!).LongCount());
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null!).LongCount());
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).LongCount());
            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).LongCount(null!));
            Assert.Throws<ArgumentNullException>("predicate", () => new List<int>(Sequence(3)).LongCount(null!));
            Assert.Throws<ArgumentNullException>("predicate", () => Iterate(3).LongCount(null!));
            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).Select(x => x).LongCount(null!));
            Assert.Throws<ArgumentNullException>("predicate", () => Array.Empty<int>().LongCount(null!));
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
        public void SourceIsDisposedWhenThePredicateThrows()
        {
            var tracked = new Tracked();

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).LongCount(x => x == 4 ? throw new InvalidOperationException() : true));
            Assert.Equal(1, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Select(x => x == 4 ? throw new InvalidOperationException() : x).LongCount());
            Assert.Equal(2, tracked.Disposed);
        }

        [Fact]
        public void SourceIsDisposedWhenCounted()
        {
            var tracked = new Tracked();

            Assert.Equal(10L, tracked.Iterate(10).LongCount());
            Assert.Equal(1, tracked.Disposed);

            Assert.Equal(5L, tracked.Iterate(10).LongCount(IsEven));
            Assert.Equal(2, tracked.Disposed);
        }
    }
}
