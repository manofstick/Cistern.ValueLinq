using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Ordering and reversing do not change how many elements there are, so Count over them is the source's count:
    // nothing is sorted or reversed, no key is computed, and a source that knows its length is not enumerated.
    public class OrderByCount
    {
        private sealed class Source : ICollection<int>
        {
            private readonly int[] _items;
            private readonly bool _exposeCount;

            public Source(bool exposeCount, params int[] items) => (_items, _exposeCount) = (items, exposeCount);

            public int Enumerations { get; private set; }

            public IEnumerator<int> GetEnumerator()
            {
                ++Enumerations;
                return ((IEnumerable<int>)_items).GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IEnumerable<int> AsEnumerable() => _exposeCount ? this : Iterate();

            private IEnumerable<int> Iterate()
            {
                using var e = GetEnumerator();
                while (e.MoveNext())
                    yield return e.Current;
            }

            public int Count => _items.Length;
            public bool IsReadOnly => true;
            public void Add(int item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Contains(int item) => throw new NotSupportedException();
            public void CopyTo(int[] array, int arrayIndex) => throw new NotSupportedException();
            public bool Remove(int item) => throw new NotSupportedException();
        }

        public static IEnumerable<object[]> Cases()
        {
            foreach (var exposeCount in new[] { true, false })
                foreach (var shape in new[] { "OrderBy", "OrderByDescending", "ThenBy", "OrderBy.Reverse", "ThenBy.Reverse" })
                    yield return [exposeCount, shape];
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Count_NeitherSortsNorReverses(bool exposeCount, string shape)
        {
            var source = new Source(exposeCount, 3, 1, 2);
            var keys = 0;
            Func<int, int> key = i => { ++keys; return i; };
            var e = source.AsEnumerable();

            var count = shape switch
            {
                "OrderBy"           => e.OrderBy(key).Count(),
                "OrderByDescending" => e.OrderByDescending(key).Count(),
                "ThenBy"            => e.OrderBy(key).ThenBy(key).Count(),
                "OrderBy.Reverse"   => e.OrderBy(key).Reverse().Count(),
                "ThenBy.Reverse"    => e.OrderBy(key).ThenBy(key).Reverse().Count(),
                _                   => throw new ArgumentOutOfRangeException(nameof(shape)),
            };

            Assert.Equal(3, count);
            Assert.Equal(0, keys);
            Assert.Equal(exposeCount ? 0 : 1, source.Enumerations);
        }
    }
}
