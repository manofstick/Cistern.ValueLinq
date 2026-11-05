using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // SelectMany over inner sequences that are Cistern chains reached as IEnumerable<T> (directly, through a wrapper that
    // hands out the chain's own enumerator, or a Concat), fused or not and with or without a result selector, gives the
    // elements in order however it is consumed: an inner sequence is started only if the query reaches it, every started
    // inner source is disposed exactly once whether the query runs to the end, halts or throws, a wrapper's
    // GetEnumerator is called once, and the downstream is disposed exactly once.
    public class SelectManyInnerChains
    {
        private sealed class Tracked
        {
            public int Started;
            public int Disposed;

            public IEnumerable<int> Iterate(int start, int count, int throwAt)
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

        private sealed class Forwarding(IEnumerable<int> source)
            : IEnumerable<int>
        {
            public int GetEnumeratorCalls;

            public IEnumerator<int> GetEnumerator()
            {
                ++GetEnumeratorCalls;
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

        public enum Overload
        {
            Plain,
            Fused,
            ResultSelector,
            ResultSelectorFused,
        }

        private const int InnerLength = 100;
        private const int Half = InnerLength / 2;
        private const int Inners = 4;
        private static readonly int[] Indices = [0, 1, 2, 3];

        private sealed class Built
        {
            public required Tracked[] Sources;
            public required List<Forwarding> Wrappers;
            public required int PartsPerInner;
            public required Func<int, IEnumerable<int>> Inner;

            public void AssertStarted(int count)
            {
                for (var i = 0; i < Sources.Length; ++i)
                {
                    Assert.Equal(i < count ? PartsPerInner : 0, Sources[i].Started);
                    Assert.Equal(i < count ? PartsPerInner : 0, Sources[i].Disposed);
                }
                foreach (var wrapper in Wrappers)
                    Assert.Equal(1, wrapper.GetEnumeratorCalls);
            }
        }

        private static Built Build(string shape, int throwAt = -1)
        {
            var sources = new Tracked[Inners];
            for (var i = 0; i < Inners; ++i)
                sources[i] = new Tracked();
            var wrappers = new List<Forwarding>();

            IEnumerable<int> Inner(int i)
            {
                var start = i * InnerLength;
                switch (shape)
                {
                    case "chain":
                        return sources[i].Iterate(start, InnerLength, throwAt).Select(x => x);
                    case "forwarded":
                        var wrapper = new Forwarding(sources[i].Iterate(start, InnerLength, throwAt).Select(x => x));
                        wrappers.Add(wrapper);
                        return wrapper;
                    case "concat":
                        return sources[i].Iterate(start, Half, throwAt).Concat(sources[i].Iterate(start + Half, Half, throwAt));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shape));
                }
            }

            return new Built { Sources = sources, Wrappers = wrappers, PartsPerInner = shape == "concat" ? 2 : 1, Inner = Inner };
        }

        // the result selector hands back the inner element, after checking it was paired with its own outer element
        private static int Paired(int index, int item)
        {
            Assert.Equal(index, item / InnerLength);
            return item;
        }

        public static IEnumerable<object[]> Cases()
        {
            foreach (var shape in new[] { "chain", "forwarded", "concat" })
            {
                foreach (var overload in Enum.GetValues<Overload>())
                    yield return [shape, overload];
            }
        }

        private static int[] Sequence(int start, int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = start + i;
            return items;
        }

        private static List<int> Pull(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);
            return items;
        }

        private static int Sum(Built built, Overload overload) =>
            overload switch
            {
                Overload.Plain => Indices.SelectMany(built.Inner).Sum(),
                Overload.Fused => Indices.SelectMany(built.Inner).Fuse().Sum(),
                Overload.ResultSelector => Indices.SelectMany(built.Inner, Paired).Sum(),
                Overload.ResultSelectorFused => Indices.SelectMany(built.Inner, Paired).Fuse().Sum(),
                _ => throw new ArgumentOutOfRangeException(nameof(overload)),
            };

        private static int Count(Built built, Overload overload) =>
            overload switch
            {
                Overload.Plain => Indices.SelectMany(built.Inner).Count(),
                Overload.Fused => Indices.SelectMany(built.Inner).Fuse().Count(),
                Overload.ResultSelector => Indices.SelectMany(built.Inner, Paired).Count(),
                Overload.ResultSelectorFused => Indices.SelectMany(built.Inner, Paired).Fuse().Count(),
                _ => throw new ArgumentOutOfRangeException(nameof(overload)),
            };

        private static List<int> ToList(Built built, Overload overload, ArrayPool<int> pool) =>
            overload switch
            {
                Overload.Plain => Indices.SelectMany(built.Inner).ToListUsePool(pool),
                Overload.Fused => Indices.SelectMany(built.Inner).Fuse().ToListUsePool(pool),
                Overload.ResultSelector => Indices.SelectMany(built.Inner, Paired).ToListUsePool(pool),
                Overload.ResultSelectorFused => Indices.SelectMany(built.Inner, Paired).Fuse().ToListUsePool(pool),
                _ => throw new ArgumentOutOfRangeException(nameof(overload)),
            };

        private static List<int> Pull(Built built, Overload overload) =>
            overload switch
            {
                Overload.Plain => Pull(Indices.SelectMany(built.Inner)),
                Overload.Fused => Pull(Indices.SelectMany(built.Inner).Fuse()),
                Overload.ResultSelector => Pull(Indices.SelectMany(built.Inner, Paired)),
                Overload.ResultSelectorFused => Pull(Indices.SelectMany(built.Inner, Paired).Fuse()),
                _ => throw new ArgumentOutOfRangeException(nameof(overload)),
            };

        private static int FirstSeen(Built built, Overload overload, List<int> seen, Func<int, bool> predicate)
        {
            int Record(int x)
            {
                seen.Add(x);
                return x;
            }

            return overload switch
            {
                Overload.Plain => Indices.SelectMany(built.Inner).Select(Record).First(predicate),
                Overload.Fused => Indices.SelectMany(built.Inner).Fuse().Select(Record).First(predicate),
                Overload.ResultSelector => Indices.SelectMany(built.Inner, Paired).Select(Record).First(predicate),
                Overload.ResultSelectorFused => Indices.SelectMany(built.Inner, Paired).Fuse().Select(Record).First(predicate),
                _ => throw new ArgumentOutOfRangeException(nameof(overload)),
            };
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void SumInOrder(string shape, Overload overload)
        {
            var built = Build(shape);
            var expected = 0;
            foreach (var item in Sequence(0, Inners * InnerLength))
                expected += item;

            Assert.Equal(expected, Sum(built, overload));
            built.AssertStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void CountAll(string shape, Overload overload)
        {
            var built = Build(shape);

            Assert.Equal(Inners * InnerLength, Count(built, overload));
            built.AssertStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void ToList_DownstreamDisposedOnce(string shape, Overload overload)
        {
            var built = Build(shape);
            var pool = new CountingPool();

            Assert.Equal(Sequence(0, Inners * InnerLength), ToList(built, overload, pool));
            built.AssertStarted(Inners);
            pool.AssertAllReturned();
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Pull_SameElements(string shape, Overload overload)
        {
            var built = Build(shape);

            Assert.Equal(Sequence(0, Inners * InnerLength), Pull(built, overload));
            built.AssertStarted(Inners);
        }

        // the halt is in the second half of the inner sequence, so a concat inner has started both its parts
        [Theory]
        [MemberData(nameof(Cases))]
        public void HaltsInAMiddleInner_LaterInnersNotStarted(string shape, Overload overload)
        {
            var built = Build(shape);
            var seen = new List<int>();
            var target = InnerLength + Half + 5;

            Assert.Equal(target, FirstSeen(built, overload, seen, x => x == target));
            Assert.Equal(Sequence(0, target + 1), seen);
            built.AssertStarted(2);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void ThrowsInAMiddleInner_StartedSourcesAndDownstreamDisposed(string shape, Overload overload)
        {
            var built = Build(shape, throwAt: 2 * InnerLength + Half + 5);
            var pool = new CountingPool();

            Assert.Throws<InvalidOperationException>(() => ToList(built, overload, pool));
            built.AssertStarted(3);
            pool.AssertAllReturned();
        }
    }
}
