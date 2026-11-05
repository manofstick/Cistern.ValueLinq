using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // ElementAt and ElementAtOrDefault of an index from the end, over a source whose count is read without enumerating
    // it and which answers no element request (a HashSet<T>), turn the index into one from the start, as System.Linq
    // does, and reach the element by walking the source to it.
    public class ElementAtFromEndCounted
    {
        // counted, and enumerable only up to the element asked for
        private sealed class Counted : ICollection<int>
        {
            private readonly int _count;
            private readonly int _readable;

            public Counted(int count, int readable)
            {
                _count = count;
                _readable = readable;
            }

            public int Count => _count;
            public bool IsReadOnly => true;
            public void Add(int item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Contains(int item) => throw new NotSupportedException();
            public void CopyTo(int[] array, int arrayIndex) => throw new NotSupportedException();
            public bool Remove(int item) => throw new NotSupportedException();

            public IEnumerator<int> GetEnumerator()
            {
                for (var i = 0; i < _count; ++i)
                {
                    if (i >= _readable)
                        throw new InvalidOperationException("read past the element asked for");
                    yield return i;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Fact]
        public void HashSet()
        {
            IEnumerable<int> values = new HashSet<int> { 5, 6, 7, 8, 9 };

            Assert.Equal(System.Linq.Enumerable.ElementAt(values, ^1), values.ElementAt(^1));
            Assert.Equal(System.Linq.Enumerable.ElementAt(values, ^5), values.ElementAt(^5));
            Assert.Throws<ArgumentOutOfRangeException>(() => values.ElementAt(^6));
            Assert.Throws<ArgumentOutOfRangeException>(() => values.ElementAt(^0));
            Assert.Equal(System.Linq.Enumerable.ElementAt(values, ^2), values.ElementAtOrDefault(^2));
            Assert.Equal(0, values.ElementAtOrDefault(^6));
            Assert.Equal(0, values.ElementAtOrDefault(^0));
        }

        [Fact]
        public void ReadsOnlyToTheElement()
        {
            IEnumerable<int> values = new Counted(100, 91);

            Assert.Equal(90, values.ElementAt(^10));
            Assert.Equal(90, values.ElementAtOrDefault(^10));
        }
    }
}
