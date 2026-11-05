using System;
using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // The result-selector SelectMany over inner sequences that expose a span (arrays, lists), alone or mixed with ones
    // that do not, gives the elements in order, each paired with its own outer element, however it is consumed: a halt
    // part way through an inner sequence starts no later one, an exception from the result selector part way through
    // returns every pooled array, and every started inner source is disposed exactly once.
    public class SelectManyResultSelectorSpans
    {
        private sealed class Tracked
        {
            public int Started;
            public int Disposed;

            public IEnumerable<int> Iterate(int start, int count)
            {
                ++Started;
                try
                {
                    for (var i = 0; i < count; ++i)
                        yield return start + i;
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

        private const int InnerLength = 100;
        private const int Inners = 4;
        private const int Total = Inners * InnerLength;
        private static readonly int[] Indices = [0, 1, 2, 3];

        private sealed class Built
        {
            public required Tracked[] Iterators;
            public required Func<int, IEnumerable<int>> Inner;
            public int CollectionSelectorCalls;
            public int ThrowAt = -1;

            // the inner element, after checking it was paired with its own outer element
            public int Paired(int index, int item)
            {
                Assert.Equal(index, item / InnerLength);
                if (item == ThrowAt)
                    throw new InvalidOperationException("result selector");
                return item;
            }

            public void AssertInnersStarted(int count)
            {
                Assert.Equal(count, CollectionSelectorCalls);
                for (var i = 0; i < Iterators.Length; ++i)
                    Assert.Equal(Iterators[i].Started, Iterators[i].Disposed);
            }
        }

        private static int[] Sequence(int start, int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = start + i;
            return items;
        }

        private static Built Build(string shape)
        {
            var iterators = new Tracked[Inners];
            for (var i = 0; i < Inners; ++i)
                iterators[i] = new Tracked();

            Built? built = null;
            IEnumerable<int> Inner(int i)
            {
                ++built!.CollectionSelectorCalls;
                var start = i * InnerLength;
                var iterate = shape switch
                {
                    "arrays" or "lists" => false,
                    "arraysThenIterator" => i == 1,
                    "iteratorThenArrays" => i == 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(shape)),
                };

                if (iterate)
                    return iterators[i].Iterate(start, InnerLength);
                if (shape == "lists")
                    return new List<int>(Sequence(start, InnerLength));
                return Sequence(start, InnerLength);
            }

            built = new Built { Iterators = iterators, Inner = Inner };
            return built;
        }

        public static IEnumerable<object[]> Shapes()
        {
            foreach (var shape in new[] { "arrays", "lists", "arraysThenIterator", "iteratorThenArrays" })
                yield return [shape];
        }

        private static List<int> Pull(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);
            return items;
        }

        [Theory]
        [MemberData(nameof(Shapes))]
        public void Sum(string shape)
        {
            var built = Build(shape);
            var expected = 0;
            foreach (var item in Sequence(0, Total))
                expected += item;

            Assert.Equal(expected, Indices.SelectMany(built.Inner, built.Paired).Sum());
            built.AssertInnersStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Shapes))]
        public void Count(string shape)
        {
            var built = Build(shape);

            Assert.Equal(Total, Indices.SelectMany(built.Inner, built.Paired).Count());
            built.AssertInnersStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Shapes))]
        public void ToList_DownstreamDisposedOnce(string shape)
        {
            var built = Build(shape);
            var pool = new CountingPool();

            Assert.Equal(Sequence(0, Total), Indices.SelectMany(built.Inner, built.Paired).ToListUsePool(pool));
            built.AssertInnersStarted(Inners);
            pool.AssertAllReturned();
        }

        [Theory]
        [MemberData(nameof(Shapes))]
        public void Pull_SameElements(string shape)
        {
            var built = Build(shape);

            Assert.Equal(Sequence(0, Total), Pull(Indices.SelectMany(built.Inner, built.Paired)));
            built.AssertInnersStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Shapes))]
        public void HaltsPartWayThroughAnInner_LaterInnersNotStarted(string shape)
        {
            var built = Build(shape);
            var seen = new List<int>();
            var target = InnerLength + 5;

            var found = Indices.SelectMany(built.Inner, built.Paired).Select(x => { seen.Add(x); return x; }).First(x => x == target);

            Assert.Equal(target, found);
            Assert.Equal(Sequence(0, target + 1), seen);
            built.AssertInnersStarted(2);
        }

        [Theory]
        [MemberData(nameof(Shapes))]
        public void ResultSelectorThrowsPartWayThroughAnInner_PooledArraysReturned(string shape)
        {
            var built = Build(shape);
            built.ThrowAt = 2 * InnerLength + 5;
            var pool = new CountingPool();

            Assert.Throws<InvalidOperationException>(() => Indices.SelectMany(built.Inner, built.Paired).ToListUsePool(pool));
            built.AssertInnersStarted(3);
            pool.AssertAllReturned();
        }
    }
}
