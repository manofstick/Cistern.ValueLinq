using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A concat built one part at a time, each step applied to a wrapper that hands out the concat below it through
    // GetEnumerator (so no level can see the concat it extends), gives the same results however it is consumed: each
    // wrapper's GetEnumerator is called once, a part is started only if the query reaches it, every started part's
    // source is disposed exactly once whether the query runs to the end, halts or throws, and the downstream is disposed
    // exactly once. The same holds for a wrapper around a chain that is not a concat, and for a query that stops early
    // in the last part of a concat of three or more parts.
    public class ConcatThroughWrappers
    {
        private sealed class Tracked
        {
            public int Started;
            public int Disposed;

            public IEnumerable<int> Iterate(int start, int count, int throwAt = -1)
            {
                ++Started;
                try
                {
                    for (var i = 0; i < count; ++i)
                    {
                        if (start + i == throwAt)
                            throw new InvalidOperationException("source");
                        yield return start + i;
                    }
                }
                finally
                {
                    ++Disposed;
                }
            }
        }

        private sealed class RunOnce(IEnumerable<int> source)
            : IEnumerable<int>
        {
            public int GetEnumeratorCalls;

            public IEnumerator<int> GetEnumerator()
            {
                Assert.Equal(0, GetEnumeratorCalls++);
                return source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class CountingPool : ArrayPool<int>
        {
            private readonly HashSet<int[]> _outstanding = new(ReferenceEqualityComparer.Instance);

            public int Rented { get; private set; }

            public override int[] Rent(int minimumLength)
            {
                var array = new int[Math.Max(minimumLength, 16)];
                _outstanding.Add(array);
                ++Rented;
                return array;
            }

            public override void Return(int[] array, bool clearArray = false) =>
                Assert.True(_outstanding.Remove(array), "an array was returned that is not out on loan");

            public void AssertAllReturned()
            {
                Assert.True(Rented > 0, "nothing was rented, so the query did not exercise the pool");
                Assert.Empty(_outstanding);
            }
        }

        private const int PartLength = 100;
        private const int Parts = 4;

        private sealed class Built
        {
            public required IEnumerable<int> Concat;
            public required Tracked[] Sources;
            public required List<RunOnce> Wrappers;

            public void AssertStarted(int count)
            {
                for (var i = 0; i < Sources.Length; ++i)
                {
                    Assert.Equal(i < count ? 1 : 0, Sources[i].Started);
                    Assert.Equal(i < count ? 1 : 0, Sources[i].Disposed);
                }
                foreach (var wrapper in Wrappers)
                    Assert.Equal(1, wrapper.GetEnumeratorCalls);
            }
        }

        private static int[] Sequence(int start, int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = start + i;
            return items;
        }

        private static Built Build(int throwAt = -1)
        {
            var sources = new Tracked[Parts];
            var wrappers = new List<RunOnce>();
            IEnumerable<int> concat = Array.Empty<int>();
            for (var i = 0; i < Parts; ++i)
            {
                sources[i] = new Tracked();
                var wrapper = new RunOnce(concat);
                wrappers.Add(wrapper);
                concat = wrapper.Concat(sources[i].Iterate(i * PartLength, PartLength, throwAt));
            }
            return new Built { Concat = concat, Sources = sources, Wrappers = wrappers };
        }

        private static List<int> Pull(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);
            return items;
        }

        [Fact]
        public void Sum()
        {
            var built = Build();
            var expected = 0;
            foreach (var item in Sequence(0, Parts * PartLength))
                expected += item;

            Assert.Equal(expected, built.Concat.Sum());
            built.AssertStarted(Parts);
        }

        [Fact]
        public void Count()
        {
            var built = Build();

            Assert.Equal(Parts * PartLength, built.Concat.Count());
            built.AssertStarted(Parts);
        }

        [Fact]
        public void ToList_DownstreamDisposedOnce()
        {
            var built = Build();
            var pool = new CountingPool();

            Assert.Equal(Sequence(0, Parts * PartLength), built.Concat.ToListUsePool(pool));
            built.AssertStarted(Parts);
            pool.AssertAllReturned();
        }

        [Fact]
        public void Pull_SameElements()
        {
            var built = Build();

            Assert.Equal(Sequence(0, Parts * PartLength), Pull(built.Concat));
            built.AssertStarted(Parts);
        }

        [Fact]
        public void HaltsInAMiddlePart_LaterPartsNotStarted()
        {
            var built = Build();
            var seen = new List<int>();

            var found = built.Concat.Select(x => { seen.Add(x); return x; }).First(x => x == PartLength + 5);

            Assert.Equal(PartLength + 5, found);
            Assert.Equal(Sequence(0, PartLength + 6), seen);
            built.AssertStarted(2);
        }

        [Fact]
        public void ThrowsInAMiddlePart_StartedSourcesAndDownstreamDisposed()
        {
            var built = Build(throwAt: 2 * PartLength + 5);
            var pool = new CountingPool();

            Assert.Throws<InvalidOperationException>(() => built.Concat.ToListUsePool(pool));

            built.AssertStarted(3);
            pool.AssertAllReturned();
        }

        [Fact]
        public void WrapperAroundAChainThatIsNotAConcat()
        {
            var sum = new Tracked();
            Assert.Equal(2 * 4950, new RunOnce(sum.Iterate(0, 100).Select(x => x * 2)).Select(x => x).Sum());
            Assert.Equal((1, 1), (sum.Started, sum.Disposed));

            var count = new Tracked();
            Assert.Equal(100, new RunOnce(count.Iterate(0, 100).Select(x => x * 2)).Count());
            Assert.Equal((1, 1), (count.Started, count.Disposed));

            var first = new Tracked();
            Assert.Equal(12, new RunOnce(first.Iterate(0, 100).Select(x => x * 2)).First(x => x > 10));
            Assert.Equal((1, 1), (first.Started, first.Disposed));
        }

        [Fact]
        public void PullStopsEarlyInTheLastOfThreeParts_DisposesIt()
        {
            var sources = new[] { new Tracked(), new Tracked(), new Tracked() };
            var concat = sources[0].Iterate(0, 3).Concat(sources[1].Iterate(3, 3)).Concat(sources[2].Iterate(6, 3));

            Assert.Equal([0, 1, 2, 3, 4, 5, 6, 7], Pull(concat.Take(8)));
            foreach (var source in sources)
                Assert.Equal((1, 1), (source.Started, source.Disposed));
        }
    }
}
