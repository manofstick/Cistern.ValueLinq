using System;
using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A push chain is disposed exactly once when the query ends, including the part of it that a SelectMany or a
    // Concat of three or more sequences shares across its inner pushes: every array a pooled ToList or ToArray rents
    // goes back to the pool, and none goes back twice.
    public class PushChainDisposal
    {
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

        private static IEnumerable<int> Iterate(int start, int count)
        {
            for (var i = 0; i < count; ++i)
                yield return start + i;
        }

        private static int[] Sequence(int start, int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = start + i;
            return items;
        }

        private static IEnumerable<int>[] Inner(string shape) =>
            shape switch
            {
                "arrays" => [Sequence(0, 300), Sequence(300, 300), Sequence(600, 300)],
                "iterators" => [Iterate(0, 300), Iterate(300, 300), Iterate(600, 300)],
                "arraysThenIterator" => [Sequence(0, 300), Iterate(300, 300), Sequence(600, 300)],
                _ => throw new ArgumentOutOfRangeException(nameof(shape)),
            };

        [Theory]
        [InlineData("arrays")]
        [InlineData("iterators")]
        [InlineData("arraysThenIterator")]
        public void SelectManyToList(string shape)
        {
            var pool = new CountingPool();

            var result = Inner(shape).SelectMany(x => x).ToListUsePool(pool);

            Assert.Equal(Sequence(0, 900), result);
            pool.AssertAllReturned();
        }

        [Theory]
        [InlineData("arrays")]
        [InlineData("iterators")]
        [InlineData("arraysThenIterator")]
        public void SelectManyToArray(string shape)
        {
            var pool = new CountingPool();

            var result = Inner(shape).SelectMany(x => x).ToArrayUsePool(pool);

            Assert.Equal(Sequence(0, 900), result);
            pool.AssertAllReturned();
        }

        [Theory]
        [InlineData("arrays")]
        [InlineData("iterators")]
        [InlineData("arraysThenIterator")]
        public void SelectManyResultSelectorToList(string shape)
        {
            var pool = new CountingPool();

            var result = Inner(shape).SelectMany(x => x, (_, item) => item).ToListUsePool(pool);

            Assert.Equal(Sequence(0, 900), result);
            pool.AssertAllReturned();
        }

        [Fact]
        public void SelectManyResultSelectorToArrayDefaultPool()
        {
            int?[][] source = [[1, 2, 3, 4], [5, 6], [], [8, 9], [-10, 100]];
            string[] expected = ["1", "2", "3", "4", "5", "6", "8", "9", "-10", "100"];

            Assert.Equal(expected, source.SelectMany(e => e, (e, f) => f.ToString()).ToArrayUsePool());
        }

        [Fact]
        public void ConcatOfTwoToList()
        {
            var pool = new CountingPool();

            var result = Iterate(0, 450).Concat(Iterate(450, 450)).ToListUsePool(pool);

            Assert.Equal(Sequence(0, 900), result);
            pool.AssertAllReturned();
        }

        [Fact]
        public void ConcatOfThreeToList()
        {
            var pool = new CountingPool();

            var result = Iterate(0, 300).Concat(Iterate(300, 300)).Concat(Iterate(600, 300)).ToListUsePool(pool);

            Assert.Equal(Sequence(0, 900), result);
            pool.AssertAllReturned();
        }

        // a chain pushed into across an IEnumerable<T> boundary is disposed by whichever side owns it, fused or not
        [Theory]
        [InlineData(false, "arrays")]
        [InlineData(false, "iterators")]
        [InlineData(true, "arrays")]
        [InlineData(true, "iterators")]
        public void AcrossABoundaryToList(bool fuse, string shape)
        {
            var pool = new CountingPool();
            IEnumerable<int> inner = (shape == "arrays" ? Sequence(0, 900) : Iterate(0, 900)).Select(x => x);

            var result = fuse ? inner.Where(x => x >= 0).Fuse().ToListUsePool(pool) : inner.Where(x => x >= 0).ToListUsePool(pool);

            Assert.Equal(Sequence(0, 900), result);
            pool.AssertAllReturned();
        }
    }
}
