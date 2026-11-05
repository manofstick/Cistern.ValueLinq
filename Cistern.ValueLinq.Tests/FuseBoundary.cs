using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Cistern chain reached as IEnumerable<T> is met through its own enumerator, or pushes into a sink holding the
    // chain applied to it, unless Fuse() above it asks for the two to be fused or the inner chain was handed out with
    // ToEnumerable(EnumerableOptions.Fuse); either way the elements and results are the same, a query that stops early disposes the inner
    // chain's source exactly once, and an exception part way through still disposes it. Fuse reaches Concat parts,
    // Zip's second sequence and SelectMany's inner sequences.
    public class FuseBoundary
    {
        private sealed class Tracked
        {
            public int Started;
            public int Disposed;

            public IEnumerable<int> Iterate(int count, int throwAt = -1)
            {
                ++Started;
                try
                {
                    for (var i = 0; i < count; ++i)
                    {
                        if (i == throwAt)
                            throw new InvalidOperationException("source");
                        yield return i;
                    }
                }
                finally
                {
                    ++Disposed;
                }
            }

            public void AssertOneRun()
            {
                Assert.Equal(1, Started);
                Assert.Equal(1, Disposed);
            }
        }

        private static int[] Sequence(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = i;
            return items;
        }

        private static List<int> Pull(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);
            return items;
        }

        // the inner chain, held as IEnumerable<int> so the outer chain meets it as a boundary
        private static IEnumerable<int> InnerOverArray(int count) => Sequence(count).Select(x => x * 2);
        private static IEnumerable<int> InnerOverIterator(Tracked tracked, int count, int throwAt = -1) => tracked.Iterate(count, throwAt).Select(x => x * 2);

        private static List<int> Expected(int count, Func<int, bool> predicate)
        {
            var items = new List<int>();
            for (var i = 0; i < count; ++i)
                if (predicate(i * 2))
                    items.Add(i * 2);
            return items;
        }

        [Theory]
        [InlineData(false, 0)]
        [InlineData(false, 1000)]
        [InlineData(true, 0)]
        [InlineData(true, 1000)]
        public void Pull_SameElements(bool fuse, int count)
        {
            var inner = InnerOverArray(count);

            var items = fuse ? Pull(inner.Where(x => x % 3 == 0).Fuse()) : Pull(inner.Where(x => x % 3 == 0));

            Assert.Equal(Expected(count, x => x % 3 == 0), items);
        }

        [Theory]
        [InlineData(false, 0)]
        [InlineData(false, 1000)]
        [InlineData(true, 0)]
        [InlineData(true, 1000)]
        public void Push_SameResults(bool fuse, int count)
        {
            var inner = InnerOverArray(count);
            var expected = Expected(count, x => x % 3 == 0);
            var expectedSum = 0;
            foreach (var item in expected)
                expectedSum += item;

            var chain = inner.Where(x => x % 3 == 0);
            if (fuse)
            {
                Assert.Equal(expectedSum, chain.Fuse().Sum());
                Assert.Equal(expected, chain.Fuse().ToList());
                Assert.Equal(expected.Count == 0 ? -1 : expected[^1], chain.Fuse().LastOrDefault(-1));
            }
            else
            {
                Assert.Equal(expectedSum, chain.Sum());
                Assert.Equal(expected, chain.ToList());
                Assert.Equal(expected.Count == 0 ? -1 : expected[^1], chain.LastOrDefault(-1));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Pull_StopsEarly_DisposesInnerSourceOnce(bool fuse)
        {
            var tracked = new Tracked();
            var inner = InnerOverIterator(tracked, 100);

            var items = fuse ? Pull(inner.Take(3).Fuse()) : Pull(inner.Take(3));

            Assert.Equal([0, 2, 4], items);
            tracked.AssertOneRun();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Push_StopsEarly_DisposesInnerSourceOnce(bool fuse)
        {
            var tracked = new Tracked();
            var inner = InnerOverIterator(tracked, 100);

            var first = fuse ? inner.Where(x => x > 6).Fuse().First() : inner.Where(x => x > 6).First();

            Assert.Equal(8, first);
            tracked.AssertOneRun();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Push_ExceptionPartWay_DisposesInnerSource(bool fuse)
        {
            var tracked = new Tracked();
            var inner = InnerOverIterator(tracked, 100, throwAt: 50);

            Assert.Throws<InvalidOperationException>(() => fuse ? inner.Where(x => x > 0).Fuse().ToList() : inner.Where(x => x > 0).ToList());
            tracked.AssertOneRun();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SelectMany_InnerSequences(bool fuse)
        {
            int[] outer = [1, 2, 3];
            IEnumerable<int> Inner(int n) => Sequence(n).Select(x => x + 10 * n);
            List<int> expected = [10, 20, 21, 30, 31, 32];

            if (fuse)
            {
                Assert.Equal(expected, Pull(outer.SelectMany(Inner).Fuse()));
                Assert.Equal(expected, outer.SelectMany(Inner).Fuse().ToList());
                Assert.Equal(expected, Pull(outer.SelectMany(n => Inner(n).Fuse())));
                Assert.Equal(expected, outer.SelectMany(n => Inner(n).Fuse()).ToList());
            }
            else
            {
                Assert.Equal(expected, Pull(outer.SelectMany(Inner)));
                Assert.Equal(expected, outer.SelectMany(Inner).ToList());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Concat_Parts(bool fuse)
        {
            var first = InnerOverArray(3);
            var second = InnerOverArray(2);
            var third = InnerOverArray(1);
            List<int> expected = [0, 2, 4, 0, 2, 0];

            if (fuse)
            {
                Assert.Equal(expected, Pull(first.Concat(second).Concat(third).Fuse()));
                Assert.Equal(expected, first.Concat(second).Concat(third).Fuse().ToList());
            }
            else
            {
                Assert.Equal(expected, Pull(first.Concat(second).Concat(third)));
                Assert.Equal(expected, first.Concat(second).Concat(third).ToList());
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1000)]
        public void FusedEnumerable_SameElementsAndResults(int count)
        {
            var inner = Sequence(count).Select(x => x * 2).ToEnumerable(EnumerableOptions.Fuse);
            var expected = Expected(count, x => x % 3 == 0);
            var expectedSum = 0;
            foreach (var item in expected)
                expectedSum += item;

            Assert.Equal(Expected(count, _ => true), Pull(inner));
            Assert.Equal(expected, Pull(inner.Where(x => x % 3 == 0)));
            Assert.Equal(expected, inner.Where(x => x % 3 == 0).ToList());
            Assert.Equal(expectedSum, inner.Where(x => x % 3 == 0).Sum());
        }

        [Fact]
        public void FusedEnumerable_StopsEarly_DisposesInnerSourceOnce()
        {
            var pulled = new Tracked();
            Assert.Equal([0, 2, 4], Pull(pulled.Iterate(100).Select(x => x * 2).ToEnumerable(EnumerableOptions.Fuse).Take(3)));
            pulled.AssertOneRun();

            var pushed = new Tracked();
            Assert.Equal(8, pushed.Iterate(100).Select(x => x * 2).ToEnumerable(EnumerableOptions.Fuse).Where(x => x > 6).First());
            pushed.AssertOneRun();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Zip_SecondSequence(bool fuse)
        {
            var tracked = new Tracked();
            var first = InnerOverArray(5);
            var second = InnerOverIterator(tracked, 3);
            List<(int, int)> expected = [(0, 0), (2, 2), (4, 4)];

            var items = new List<(int, int)>();
            if (fuse)
            {
                foreach (var item in first.Zip(second).Fuse())
                    items.Add(item);
            }
            else
            {
                foreach (var item in first.Zip(second))
                    items.Add(item);
            }

            Assert.Equal(expected, items);
            tracked.AssertOneRun();
        }
    }
}
