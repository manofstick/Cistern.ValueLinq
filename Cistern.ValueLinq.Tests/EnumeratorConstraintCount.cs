using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A query over a collection reached through its own enumerator is deferred: a collection changed between building
    // the query and running it is counted, and sized for, as it is when the query runs.
    public class EnumeratorConstraintCount : EnumerableTests
    {
        // implements no count interface, so the library learns its size only from what the query is given
        private sealed class Bag : IEnumerable<int>
        {
            private readonly List<int> _items = new();

            public Bag(params int[] items) => _items.AddRange(items);

            public int Count => _items.Count;
            public void Add(int item) => _items.Add(item);

            public Enumerator GetEnumerator() => new(_items.GetEnumerator());
            IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator : IEnumerator<int>
            {
                private List<int>.Enumerator _inner;

                public Enumerator(List<int>.Enumerator inner) => _inner = inner;

                public int Current => _inner.Current;
                object IEnumerator.Current => Current;
                public bool MoveNext() => _inner.MoveNext();
                public void Reset() => throw new NotSupportedException();
                public void Dispose() => _inner.Dispose();
            }
        }

        [Fact]
        public void SuppliedCount_IsReadWhenTheQueryRuns()
        {
            var source = new Bag(1, 2, 3);
            var query = source.OfEnumeratorConstraint<int, Bag, Bag.Enumerator>(s => s.GetEnumerator(), s => s.Count);

            source.Add(4);

            Assert.Equal(4, query.Count());
            Assert.Equal(new[] { 1, 2, 3, 4 }, query.ToArray());
            Assert.Equal(new[] { 1, 2, 3, 4 }, query.ToList());
        }

        [Fact]
        public void CollectionCount_IsReadWhenTheQueryRuns()
        {
            var source = new Queue<int>(new[] { 1, 2, 3 });
            var query = source.OfEnumeratorConstraint<int, Queue<int>, Queue<int>.Enumerator>(s => s.GetEnumerator());

            source.Enqueue(4);

            Assert.Equal(4, query.Count());
            Assert.Equal(new[] { 1, 2, 3, 4 }, query.ToArray());
        }

        [Fact]
        public void NoCount_IsWalked()
        {
            var source = new Bag(1, 2, 3);
            var query = source.OfEnumeratorConstraint<int, Bag, Bag.Enumerator>(s => s.GetEnumerator());

            source.Add(4);

            Assert.Equal(4, query.Count());
            Assert.Equal(new[] { 1, 2, 3, 4 }, query.ToArray());
        }
    }
}
