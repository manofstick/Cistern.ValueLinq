using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // An ICollection<T> pushed into a chain that reads every element is handed over cold: its elements are copied out by
    // its own CopyTo, once, and it is never enumerated. A chain that may stop early, or a collection past the copy limit
    // under a chain that reads through a span, enumerates it instead. A chain that copies into its own storage takes it
    // at any length, including a part copied whole into a ToArray or ToList builder after a short part. Either way the
    // elements are the enumerated ones, in order.
    public class ColdBatches
    {
        private sealed class Counted<T>(IList<T> items) : ICollection<T>
        {
            public int CopyTos;
            public int Enumerations;

            public int Count => items.Count;
            public bool IsReadOnly => true;

            public void CopyTo(T[] array, int arrayIndex)
            {
                ++CopyTos;
                items.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                ++Enumerations;
                return items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Contains(T item) => items.Contains(item);
            public void Add(T item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Remove(T item) => throw new NotSupportedException();
        }

        private static IEnumerable<int> Iterate(int start, int count)
        {
            for (var i = 0; i < count; ++i)
                yield return start + i;
        }

        private static int[] Values(int start, int count)
        {
            var values = new int[count];
            for (var i = 0; i < count; ++i)
                values[i] = start + i;
            return values;
        }

        public static IEnumerable<object[]> PartLengths()
        {
            foreach (var before in new[] { 0, 3, 8, 9, 20, 100 })
            {
                foreach (var length in new[] { 0, 1, 10, 1000 })
                    yield return [before, length];
            }
        }

        [Theory]
        [MemberData(nameof(PartLengths))]
        public void AConcatPartIsCopiedWholeIntoTheBuilder(int before, int length)
        {
            var expected = Values(0, before + length);

            var toArray = new Counted<int>(Values(before, length));
            Assert.Equal(expected, Iterate(0, before).Concat(toArray).ToArray());
            Assert.Equal(length == 0 ? 0 : 1, toArray.CopyTos);
            Assert.Equal(0, toArray.Enumerations);

            var toList = new Counted<int>(Values(before, length));
            Assert.Equal(expected, Iterate(0, before).Concat(toList).ToList());
            Assert.Equal(length == 0 ? 0 : 1, toList.CopyTos);
            Assert.Equal(0, toList.Enumerations);
        }

        [Fact]
        public void AChainReadingEveryElementCopiesTheCollectionOnce()
        {
            void AssertCopiedOnce<TResult>(TResult expected, Func<Counted<int>, TResult> query)
            {
                var collection = new Counted<int>(Values(1, 100));
                Assert.Equal(expected, query(collection));
                Assert.Equal(1, collection.CopyTos);
                Assert.Equal(0, collection.Enumerations);
            }

            AssertCopiedOnce(5050, c => c.Sum());
            AssertCopiedOnce(100, c => c.Max());
            AssertCopiedOnce(1, c => c.Min());
            AssertCopiedOnce(50.5, c => c.Average());
            AssertCopiedOnce(50, c => c.Count(x => x % 2 == 0));
            AssertCopiedOnce(10100, c => c.Select(x => x * 2).Sum());
            AssertCopiedOnce(2550, c => c.Where(x => x % 2 == 0).Sum());
            AssertCopiedOnce(100, c => c.MaxBy(x => x));
            AssertCopiedOnce(5050, c => Iterate(0, 0).Concat(c).Sum());
            AssertCopiedOnce((5050, 100), c => c.Fork(x => x.Sum(), x => x.Max()));
        }

        [Fact]
        public void AChainThatMayStopEarlyEnumeratesTheCollection()
        {
            void AssertEnumerated<TResult>(TResult expected, Func<Counted<int>, TResult> query)
            {
                var collection = new Counted<int>(Values(1, 100));
                Assert.Equal(expected, query(collection));
                Assert.Equal(0, collection.CopyTos);
                Assert.Equal(1, collection.Enumerations);
            }

            AssertEnumerated(50, c => c.First(x => x == 50));
            AssertEnumerated(true, c => c.Any(x => x == 50));
            AssertEnumerated(6, c => c.Take(3).Sum());
            AssertEnumerated(100, c => c.Select(x => x * 2).First(x => x == 100));
            AssertEnumerated(50, c => Iterate(0, 1).Concat(c).First(x => x == 50));
            AssertEnumerated((5050, 1), c => c.Fork(x => x.Sum(), x => x.First()));
        }

        [Fact]
        public void ACountReadsOnlyTheLength()
        {
            var collection = new Counted<int>(Values(1, 100));
            Assert.Equal(103L, Iterate(0, 3).Concat(collection).LongCount());
            Assert.Equal(0, collection.CopyTos);
            Assert.Equal(0, collection.Enumerations);
        }

        [Fact]
        public void PastTheCopyLimitASpanReaderEnumeratesTheCollection()
        {
            var length = BatchKindsExtensions.ColdSpanLimit + 1;
            var collection = new Counted<int>(Values(0, length));

            long expected = 0;
            for (var i = 0; i < length; ++i)
                expected += i;

            Assert.Equal(expected, collection.Sum(x => (long)x));
            Assert.Equal(0, collection.CopyTos);
            Assert.Equal(1, collection.Enumerations);
        }

        [Fact]
        public void CollectionsWithStructEnumeratorsGiveTheEnumeratedElements()
        {
            var ints = new HashSet<int>(Values(0, 100));
            var strings = new HashSet<string>();
            foreach (var value in Values(0, 100))
                strings.Add(value.ToString());
            var queue = new Queue<string>(strings);

            var expectedStrings = new List<string>(strings);
            var expectedQueue = new List<string>(queue);
            var longest = expectedStrings[0];
            foreach (var s in expectedStrings)
            {
                if (s.Length > longest.Length)
                    longest = s;
            }

            Assert.Equal(4950, ints.Sum());
            Assert.Equal(50, ints.Count(x => x % 2 == 0));
            Assert.Equal(90, strings.Count(s => s.Length == 2));
            Assert.Equal(longest, strings.MaxBy(s => s.Length));
            Assert.Equal(expectedStrings[50], strings.First(s => ReferenceEquals(s, expectedStrings[50])));
            Assert.Equal(longest, queue.MaxBy(s => s.Length));
            Assert.Equal(expectedQueue, queue.ToList());

            var concatenated = new List<string> { "a", "b" };
            concatenated.AddRange(expectedStrings);
            Assert.Equal(concatenated, Iterate(0, 2).Select(i => i == 0 ? "a" : "b").Concat(strings).ToArray());
        }
    }
}
