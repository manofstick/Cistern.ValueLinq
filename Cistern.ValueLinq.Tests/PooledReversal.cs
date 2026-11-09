using System;
using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Reverse pushed buffers its reversal in an array rented from its pool for the push alone: the array goes back
    // once, whether the push runs to the end, is stopped early or is thrown out of, and the elements pushed are the
    // reversal's.
    public class PooledReversal
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

        private const int Length = 100;

        private static IEnumerable<int> Iterate()
        {
            for (var i = 0; i < Length; ++i)
                yield return i;
        }

        private static int[] Values()
        {
            var values = new int[Length];
            for (var i = 0; i < Length; ++i)
                values[i] = i;
            return values;
        }

        // a counted source is copied into the rented array, any other pushed into a builder and copied into it from there
        private static ValueEnumerable<int, Nodes.Reverse.Node<int, Nodes.Select.Node<int, int, Containers.SystemArray.Node<int>>>, Nodes.Reverse.Pull<int, Nodes.Select.Node<int, int, Containers.SystemArray.Node<int>>>> Counted(CountingPool pool, Func<int, int> selector) =>
            Values().Select(selector).Reverse(arrayPoolInfo: (pool, false));

        [Fact]
        public void RunToTheEnd_Counted()
        {
            var pool = new CountingPool();
            var reversed = new List<int>();

            Counted(pool, x => x).ForEach(reversed.Add);

            Assert.Equal(System.Linq.Enumerable.Reverse(Values()), reversed);
            pool.AssertAllReturned();
        }

        [Fact]
        public void RunToTheEnd_Pushed()
        {
            var pool = new CountingPool();
            var reversed = new List<int>();

            Iterate().Select(x => x).Reverse(arrayPoolInfo: (pool, false)).ForEach(reversed.Add);

            Assert.Equal(System.Linq.Enumerable.Reverse(Values()), reversed);
            pool.AssertAllReturned();
        }

        [Fact]
        public void StoppedEarly()
        {
            var pool = new CountingPool();

            Assert.True(Counted(pool, x => x).Any(x => x == Length - 3));
            pool.AssertAllReturned();

            pool = new CountingPool();
            Assert.True(Iterate().Select(x => x).Reverse(arrayPoolInfo: (pool, false)).Any(x => x == Length - 3));
            pool.AssertAllReturned();
        }

        [Fact]
        public void ThrownOutOf()
        {
            var pool = new CountingPool();

            Assert.Throws<InvalidOperationException>(() => Counted(pool, x => x).Select(x => x == Length / 2 ? throw new InvalidOperationException() : x).Sum());
            pool.AssertAllReturned();

            pool = new CountingPool();
            Assert.Throws<InvalidOperationException>(() => Iterate().Select(x => x).Reverse(arrayPoolInfo: (pool, false)).Select(x => x == Length / 2 ? throw new InvalidOperationException() : x).Sum());
            pool.AssertAllReturned();
        }
    }
}
