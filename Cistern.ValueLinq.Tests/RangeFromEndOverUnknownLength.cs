using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // TakeLast and SkipLast over a source whose length is not known keep the elements they hold in a buffer that grows
    // toward the count, in push and in pull. TakeLast disposes the source before it hands on its first element, as
    // System.Linq does. A source's maximum length only sizes the buffer, so a source that yields more than it reported
    // still gives the right elements.
    public class RangeFromEndOverUnknownLength
    {
        public static IEnumerable<object[]> Cases()
        {
            foreach (var length in new[] { 0, 1, 3, 16, 17, 40, 100 })
                foreach (var count in new[] { 0, 1, 3, 16, 17, 50, int.MaxValue })
                    yield return [length, count];
        }

        private static IEnumerable<int> Iterate(int length)
        {
            for (var i = 0; i < length; ++i)
                yield return i;
        }

        private static int[] Range(int start, int end)
        {
            var range = new int[Math.Max(0, end - start)];
            for (var i = 0; i < range.Length; ++i)
                range[i] = start + i;
            return range;
        }

        private static int[] Last(int length, int count) => Range(Math.Max(0, length - Math.Max(0, count)), length);
        private static int[] AllButLast(int length, int count) => Range(0, length - Math.Max(0, Math.Min(length, count)));

        private static List<int> Pull(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);
            return items;
        }

        private static long Total(int[] values)
        {
            var total = 0L;
            foreach (var value in values)
                total += value;
            return total;
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void TakeLast_Push(int length, int count)
        {
            var expected = Last(length, count);
            Assert.Equal(expected, Iterate(length).TakeLast(count).ToArray());
            Assert.Equal(expected, Iterate(length).TakeLast(count).ToList());
            Assert.Equal(Total(expected), Iterate(length).TakeLast(count).Select(x => (long)x).Sum());
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void TakeLast_Pull(int length, int count) =>
            Assert.Equal(Last(length, count), Pull(Iterate(length).TakeLast(count)));

        [Theory]
        [MemberData(nameof(Cases))]
        public void SkipLast_Push(int length, int count)
        {
            var expected = AllButLast(length, count);
            Assert.Equal(expected, Iterate(length).SkipLast(count).ToArray());
            Assert.Equal(expected, Iterate(length).SkipLast(count).ToList());
            Assert.Equal(Total(expected), Iterate(length).SkipLast(count).Select(x => (long)x).Sum());
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void SkipLast_Pull(int length, int count) =>
            Assert.Equal(AllButLast(length, count), Pull(Iterate(length).SkipLast(count)));

        // disposal is recorded only by Dispose itself: an iterator method would run its finally on reaching the end
        private sealed class DisposeTracked
        {
            public bool Disposed;

            public IEnumerable<int> Iterate(int length) => new Enumerable(this, length);

            private sealed class Enumerable(DisposeTracked owner, int length) : IEnumerable<int>
            {
                public IEnumerator<int> GetEnumerator() => new Enumerator(owner, length);
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            private sealed class Enumerator(DisposeTracked owner, int length) : IEnumerator<int>
            {
                private int _index = -1;

                public int Current => _index;
                object IEnumerator.Current => Current;
                public bool MoveNext() => ++_index < length;
                public void Reset() => throw new NotSupportedException();
                public void Dispose() => owner.Disposed = true;
            }
        }

        [Fact]
        public void TakeLast_Push_DisposesSourceBeforeHandingOn()
        {
            var source = new DisposeTracked();
            var sawUndisposed = false;
            var result = source.Iterate(10).TakeLast(3).Select(x => { sawUndisposed |= !source.Disposed; return x; }).ToArray();

            Assert.Equal([7, 8, 9], result);
            Assert.False(sawUndisposed);
        }

        [Fact]
        public void TakeLast_Pull_DisposesSourceBeforeHandingOn()
        {
            var source = new DisposeTracked();
            var sawUndisposed = false;
            var result = new List<int>();
            foreach (var item in source.Iterate(10).TakeLast(3))
            {
                sawUndisposed |= !source.Disposed;
                result.Add(item);
            }

            Assert.Equal([7, 8, 9], result);
            Assert.False(sawUndisposed);
        }

        // reports two elements and yields ten, so through a Where only a maximum length of two is known
        private sealed class UnderReported : IEnumerable<int>, ICollection
        {
            public int Count => 2;
            public bool IsSynchronized => false;
            public object SyncRoot => this;
            public void CopyTo(Array array, int index) => throw new NotSupportedException();

            public IEnumerator<int> GetEnumerator()
            {
                for (var i = 0; i < 10; ++i)
                    yield return i;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Fact]
        public void TakeLast_SourceLongerThanItsBound()
        {
            var source = new UnderReported();
            Assert.Equal([5, 6, 7, 8, 9], source.Where(x => true).TakeLast(5).ToArray());
            Assert.Equal([5, 6, 7, 8, 9], Pull(source.Where(x => true).TakeLast(5)));
        }

        [Fact]
        public void SkipLast_SourceLongerThanItsBound()
        {
            var source = new UnderReported();
            Assert.Equal([0, 1, 2, 3, 4, 5, 6], source.Where(x => true).SkipLast(3).ToArray());
            Assert.Equal([0, 1, 2, 3, 4, 5, 6], Pull(source.Where(x => true).SkipLast(3)));
        }
    }
}
