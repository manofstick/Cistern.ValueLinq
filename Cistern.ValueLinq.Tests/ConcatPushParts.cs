using System;
using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Concat run by push gives the same elements in order when a part is itself a Concat, whether that Concat is seen
    // by its type, met across an IEnumerable<T> boundary or fused with it: a downstream that halts in one part starts
    // none of the parts after it, every part that was started has its source disposed exactly once whether the query
    // runs to the end, halts or throws, and the downstream is disposed exactly once.
    public class ConcatPushParts
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

        private static int[] Sequence(int start, int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = start + i;
            return items;
        }

        private static (IEnumerable<int> Concat, Tracked[] Sources) Build(string shape, int throwAt = -1)
        {
            var sources = new Tracked[Parts];
            var parts = new IEnumerable<int>[Parts];
            for (var i = 0; i < Parts; ++i)
            {
                sources[i] = new Tracked();
                parts[i] = sources[i].Iterate(i * PartLength, PartLength, throwAt);
            }

            IEnumerable<int> concat = shape switch
            {
                // a Concat over the IEnumerable<T> overloads collects the parts it can see from its first part down, so
                // concats nested in the later part are each met across the boundary
                "boundary" => parts[0].Concat(parts[1].Concat(parts[2].Concat(parts[3]))),
                "typed" => parts[0].OfEnumerable().Concat(parts[1].OfEnumerable().Concat(parts[2].OfEnumerable().Concat(parts[3].OfEnumerable()))),
                "fused" => parts[0].Concat(parts[1].Concat(parts[2].Concat(parts[3]))).Fuse(),
                "collected" => parts[0].Concat(parts[1]).Concat(parts[2]).Concat(parts[3]),
                _ => throw new ArgumentOutOfRangeException(nameof(shape)),
            };
            return (concat, sources);
        }

        private static void AssertStarted(Tracked[] sources, int count)
        {
            for (var i = 0; i < sources.Length; ++i)
            {
                Assert.Equal(i < count ? 1 : 0, sources[i].Started);
                Assert.Equal(i < count ? 1 : 0, sources[i].Disposed);
            }
        }

        [Theory]
        [InlineData("boundary")]
        [InlineData("typed")]
        [InlineData("fused")]
        [InlineData("collected")]
        public void RunsToTheEnd(string shape)
        {
            var expected = Sequence(0, Parts * PartLength);
            var expectedSum = 0;
            foreach (var item in expected)
                expectedSum += item;

            var (toList, toListSources) = Build(shape);
            Assert.Equal(expected, toList.ToList());
            AssertStarted(toListSources, Parts);

            var (sum, sumSources) = Build(shape);
            Assert.Equal(expectedSum, sum.Sum());
            AssertStarted(sumSources, Parts);

            // Last is asked of the last part first, as System.Linq's Concat asks it, so only that part is started
            var (last, lastSources) = Build(shape);
            Assert.Equal(expected[^1], last.Last());
            for (var i = 0; i < lastSources.Length; ++i)
            {
                Assert.Equal(i == Parts - 1 ? 1 : 0, lastSources[i].Started);
                Assert.Equal(i == Parts - 1 ? 1 : 0, lastSources[i].Disposed);
            }
        }

        [Theory]
        [InlineData("boundary")]
        [InlineData("typed")]
        [InlineData("fused")]
        [InlineData("collected")]
        public void HaltsInAMiddlePart_LaterPartsNotStarted(string shape)
        {
            var (concat, sources) = Build(shape);
            var seen = new List<int>();

            var found = concat.Select(x => { seen.Add(x); return x; }).First(x => x == PartLength + 5);

            Assert.Equal(PartLength + 5, found);
            Assert.Equal(Sequence(0, PartLength + 6), seen);
            AssertStarted(sources, 2);
        }

        [Theory]
        [InlineData("boundary")]
        [InlineData("typed")]
        [InlineData("fused")]
        [InlineData("collected")]
        public void ThrowsInAMiddlePart_StartedSourcesAndDownstreamDisposed(string shape)
        {
            var (concat, sources) = Build(shape, throwAt: 2 * PartLength + 5);
            var pool = new CountingPool();

            Assert.Throws<InvalidOperationException>(() => concat.ToListUsePool(pool));

            AssertStarted(sources, 3);
            pool.AssertAllReturned();
        }

        [Theory]
        [InlineData("boundary")]
        [InlineData("typed")]
        [InlineData("fused")]
        [InlineData("collected")]
        public void DownstreamDisposedOnce(string shape)
        {
            var (concat, _) = Build(shape);
            var pool = new CountingPool();

            var result = concat.ToListUsePool(pool);

            Assert.Equal(Sequence(0, Parts * PartLength), result);
            pool.AssertAllReturned();
        }

        // parts that hand the downstream whole spans, one of which it halts in
        [Fact]
        public void HaltsInASpanOfANestedPart()
        {
            IEnumerable<int> nested = Sequence(0, 3).Concat(Sequence(3, 3).Concat(Sequence(6, 3)));

            Assert.Equal(4, nested.ElementAt(4));
            Assert.Equal(7, nested.ElementAt(7));
            Assert.Equal(0, nested.First());
            Assert.Equal(Sequence(0, 9), nested.ToArray());
        }
    }
}
