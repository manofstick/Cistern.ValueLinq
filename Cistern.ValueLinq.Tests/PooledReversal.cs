using System;
using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Reverse buffers its reversal in an array rented from its pool for one push or one enumeration: the array goes
    // back once, whether the run goes to the end, is stopped early or is thrown out of, and the elements are the
    // reversal's. Copies of an enumerator share the rental: it goes back once however many of them are disposed, and a
    // copy pulled after another was disposed ends rather than read the returned array.
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

        private const int Length = 1000;

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

        [Fact]
        public void Enumerated()
        {
            var pool = new CountingPool();
            var reversed = new List<int>();

            foreach (var x in Counted(pool, x => x))
                reversed.Add(x);

            Assert.Equal(System.Linq.Enumerable.Reverse(Values()), reversed);
            pool.AssertAllReturned();

            pool = new CountingPool();
            reversed.Clear();
            foreach (var x in Iterate().Select(x => x).Reverse(arrayPoolInfo: (pool, false)))
                reversed.Add(x);

            Assert.Equal(System.Linq.Enumerable.Reverse(Values()), reversed);
            pool.AssertAllReturned();
        }

        [Fact]
        public void EnumeratedAsIEnumerable()
        {
            var pool = new CountingPool();
            IEnumerable<int> reversal = Counted(pool, x => x);

            Assert.Equal(System.Linq.Enumerable.Reverse(Values()), System.Linq.Enumerable.ToList(reversal));
            pool.AssertAllReturned();

            pool = new CountingPool();
            reversal = Counted(pool, x => x);
            using (var enumerator = reversal.GetEnumerator())
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(Length - 1, enumerator.Current);
            }
            pool.AssertAllReturned();
        }

        [Fact]
        public void EnumerationStoppedEarly()
        {
            var pool = new CountingPool();

            foreach (var x in Counted(pool, x => x))
            {
                if (x == Length - 3)
                    break;
            }

            pool.AssertAllReturned();
        }

        [Fact]
        public void EnumerationThrownOutOf()
        {
            var pool = new CountingPool();

            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var x in Counted(pool, x => x))
                {
                    if (x == Length / 2)
                        throw new InvalidOperationException();
                }
            });

            pool.AssertAllReturned();
        }

        [Fact]
        public void CopiedEnumerators_ReturnTheRentalOnce()
        {
            var pool = new CountingPool();

            var enumerator = Counted(pool, x => x).GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.True(enumerator.MoveNext());
            var copy = enumerator;

            enumerator.Dispose();
            Assert.False(copy.MoveNext());
            copy.Dispose();

            pool.AssertAllReturned();
        }

        [Fact]
        public void CopiedOrderedEnumerator_EndsOnceAnotherIsDisposed()
        {
            var enumerator = Values().OrderBy(x => -x).GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(Length - 1, enumerator.Current);
            var copy = enumerator;

            Assert.True(copy.MoveNext());
            Assert.Equal(Length - 2, copy.Current);

            enumerator.Dispose();
            Assert.False(copy.MoveNext());
            copy.Dispose();
        }
    }
}
