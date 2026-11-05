using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Shuffle buffers its source into an array of its own and permutes that: the caller's container is never
    // reordered, a count is answered by the source without a permutation being made, and a Take over it is a uniform
    // selection taken from one permutation.
    public class ShuffleChains
    {
        private sealed class Counted : ICollection<int>
        {
            private readonly int[] _items;

            public Counted(params int[] items) => _items = items;

            public int Enumerations { get; private set; }

            public IEnumerator<int> GetEnumerator()
            {
                ++Enumerations;
                return ((IEnumerable<int>)_items).GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => _items.Length;
            public bool IsReadOnly => true;
            public void Add(int item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Contains(int item) => throw new NotSupportedException();
            public void CopyTo(int[] array, int arrayIndex) => throw new NotSupportedException();
            public bool Remove(int item) => throw new NotSupportedException();
        }

        private static int[] Range(int length)
        {
            var data = new int[length];
            for (var i = 0; i < length; ++i)
                data[i] = i;
            return data;
        }

        [Fact]
        public void SourceArrayIsNotReordered()
        {
            var data = Range(1000);
            var expected = (int[])data.Clone();

            var toArray = data.Shuffle().ToArray();
            var toList = data.Shuffle().ToList();
            var pulled = new List<int>();
            foreach (var x in data.Shuffle())
                pulled.Add(x);
            var taken = data.Shuffle().Take(10).ToArray();
            var skipped = data.Shuffle().Skip(990).ToArray();

            Assert.Equal(expected, data);
            Assert.NotEqual(expected, toArray);
            Assert.NotEqual(expected, toList);
            Assert.NotEqual(expected, pulled);
            Assert.Equal(10, taken.Length);
            Assert.Equal(10, skipped.Length);
        }

        [Fact]
        public void SourceListIsNotReordered()
        {
            var data = new List<int>(Range(1000));
            var expected = data.ToArray();

            var toArray = data.Shuffle().ToArray();
            foreach (var x in data.Shuffle())
                _ = x;

            Assert.Equal(expected, data);
            Assert.NotEqual(expected, toArray);
        }

        [Fact]
        public void CountComesFromTheSource()
        {
            var source = new Counted(3, 1, 2);
            IEnumerable<int> e = source;

            Assert.Equal(3, e.Shuffle().Count());
            Assert.Equal(0, source.Enumerations);

            Assert.Equal(2, e.Shuffle().Take(2).Take(5).Count());
        }

        [Fact]
        public void TakeAndSkipPartitionOnePermutation()
        {
            var data = Range(100);

            var taken = data.Shuffle().Take(30).ToArray();
            Assert.Equal(30, taken.Length);
            Assert.Equal(30, new HashSet<int>(taken).Count);
            Assert.All(taken, x => Assert.InRange(x, 0, 99));

            var skipped = data.Shuffle().Skip(70).ToArray();
            Assert.Equal(30, skipped.Length);
            Assert.Equal(30, new HashSet<int>(skipped).Count);

            var chained = data.OfArray().Where(x => x % 2 == 0).Shuffle().Select(x => x + 1).ToArray();
            Array.Sort(chained);
            Assert.Equal(50, chained.Length);
            Assert.All(chained, x => Assert.Equal(1, x % 2));
        }

        [Fact]
        public void EachEnumerationIsAFreshPermutation()
        {
            var shuffled = Range(1000).Shuffle();

            var first = shuffled.ToArray();
            var second = shuffled.ToArray();
            var third = new List<int>();
            foreach (var x in shuffled)
                third.Add(x);

            Assert.NotEqual(first, second);
            Assert.NotEqual(first, third);
            Array.Sort(first);
            Array.Sort(second);
            third.Sort();
            Assert.Equal(Range(1000), first);
            Assert.Equal(Range(1000), second);
            Assert.Equal(Range(1000), third);
        }
    }
}
