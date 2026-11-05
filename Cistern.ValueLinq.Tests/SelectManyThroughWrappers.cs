using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A SelectMany chain handed out through a wrapper's GetEnumerator and consumed by push, with or without a result
    // selector and whether or not some of it was pulled before it was handed out, gives the elements still to come in
    // order: the wrapper's GetEnumerator is called once, an inner sequence is started only if the query reaches it,
    // every started inner source is disposed exactly once whether the query runs to the end, halts or throws, and the
    // downstream is disposed exactly once.
    public class SelectManyThroughWrappers
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

        // hands out the source's enumerator once, after pulling the given number of elements from it
        private sealed class HandedOut(IEnumerable<int> source, int pulledFirst)
            : IEnumerable<int>
        {
            public int GetEnumeratorCalls;

            public IEnumerator<int> GetEnumerator()
            {
                Assert.Equal(0, GetEnumeratorCalls++);
                var enumerator = source.GetEnumerator();
                for (var i = 0; i < pulledFirst; ++i)
                {
                    Assert.True(enumerator.MoveNext());
                    Assert.Equal(i, enumerator.Current);
                }
                return enumerator;
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

        private const int InnerLength = 100;
        private const int Inners = 4;
        private const int Total = Inners * InnerLength;
        private static readonly int[] Indices = [0, 1, 2, 3];

        private sealed class Built
        {
            public required Tracked[] Sources;
            public required HandedOut Wrapper;

            public void AssertStarted(int count)
            {
                for (var i = 0; i < Sources.Length; ++i)
                {
                    Assert.Equal(i < count ? 1 : 0, Sources[i].Started);
                    Assert.Equal(i < count ? 1 : 0, Sources[i].Disposed);
                }
                Assert.Equal(1, Wrapper.GetEnumeratorCalls);
            }
        }

        // the result selector hands back the inner element, after checking it was paired with its own outer element
        private static int Paired(int index, int item)
        {
            Assert.Equal(index, item / InnerLength);
            return item;
        }

        private static Built Build(string shape, bool resultSelector, int pulledFirst, int throwAt = -1)
        {
            var sources = new Tracked[Inners];
            for (var i = 0; i < Inners; ++i)
                sources[i] = new Tracked();

            IEnumerable<int> Inner(int i)
            {
                var iterator = sources[i].Iterate(i * InnerLength, InnerLength, throwAt);
                return shape switch
                {
                    "chain" => iterator.Select(x => x),
                    "iterator" => iterator,
                    _ => throw new ArgumentOutOfRangeException(nameof(shape)),
                };
            }

            IEnumerable<int> query = resultSelector ? Indices.SelectMany(Inner, Paired) : Indices.SelectMany(Inner);
            return new Built { Sources = sources, Wrapper = new HandedOut(query, pulledFirst) };
        }

        // with the second number, the second inner sequence is open when the enumerator is handed out
        public static IEnumerable<object[]> Cases()
        {
            foreach (var shape in new[] { "chain", "iterator" })
            {
                foreach (var resultSelector in new[] { false, true })
                {
                    foreach (var pulledFirst in new[] { 0, InnerLength + 10 })
                        yield return [shape, resultSelector, pulledFirst];
                }
            }
        }

        private static int[] Sequence(int start, int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = start + i;
            return items;
        }

        private static int StartedBy(int pulledFirst) => pulledFirst / InnerLength + 1;

        [Theory]
        [MemberData(nameof(Cases))]
        public void Sum(string shape, bool resultSelector, int pulledFirst)
        {
            var built = Build(shape, resultSelector, pulledFirst);
            var expected = 0;
            foreach (var item in Sequence(pulledFirst, Total - pulledFirst))
                expected += item;

            Assert.Equal(expected, built.Wrapper.Sum());
            built.AssertStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Count(string shape, bool resultSelector, int pulledFirst)
        {
            var built = Build(shape, resultSelector, pulledFirst);

            Assert.Equal(Total - pulledFirst, built.Wrapper.Count());
            built.AssertStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void ToList_DownstreamDisposedOnce(string shape, bool resultSelector, int pulledFirst)
        {
            var built = Build(shape, resultSelector, pulledFirst);
            var pool = new CountingPool();

            Assert.Equal(Sequence(pulledFirst, Total - pulledFirst), built.Wrapper.ToListUsePool(pool));
            built.AssertStarted(Inners);
            pool.AssertAllReturned();
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void HaltsInALaterInner_LaterInnersNotStarted(string shape, bool resultSelector, int pulledFirst)
        {
            var built = Build(shape, resultSelector, pulledFirst);
            var seen = new List<int>();
            var target = 2 * InnerLength + 5;

            var found = built.Wrapper.Select(x => { seen.Add(x); return x; }).First(x => x == target);

            Assert.Equal(target, found);
            Assert.Equal(Sequence(pulledFirst, target + 1 - pulledFirst), seen);
            built.AssertStarted(3);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void ThrowsInALaterInner_StartedSourcesAndDownstreamDisposed(string shape, bool resultSelector, int pulledFirst)
        {
            var built = Build(shape, resultSelector, pulledFirst, throwAt: 2 * InnerLength + 5);
            var pool = new CountingPool();

            Assert.Throws<InvalidOperationException>(() => built.Wrapper.ToListUsePool(pool));
            built.AssertStarted(3);
            pool.AssertAllReturned();
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void HaltsInTheOpenInner(string shape, bool resultSelector, int pulledFirst)
        {
            var built = Build(shape, resultSelector, pulledFirst);
            var target = pulledFirst + 3;

            Assert.Equal(target, built.Wrapper.First(x => x == target));
            built.AssertStarted(StartedBy(pulledFirst));
        }
    }
}
