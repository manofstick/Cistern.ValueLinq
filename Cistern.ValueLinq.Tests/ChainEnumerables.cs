using System;
using System.Collections.Generic;
using Cistern.ValueLinq.Containers;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A chain handed out by ToEnumerable yields the chain's elements whatever its options; it is an ICollection<T> only
    // where CopyTo's dry run answers, counting without running user code and copying the chain in, and it is enumerated
    // over the chain rebuilt once over the container its IEnumerable<T> source holds.
    public class ChainEnumerables
    {
        private static readonly int[] Data = [3, 1, 4, 1, 5, 9, 2, 6];

        [Theory]
        [InlineData(EnumerableOptions.None)]
        [InlineData(EnumerableOptions.Fuse)]
        public void YieldsTheChainsElements(EnumerableOptions options)
        {
            IEnumerable<int> source = Data;

            Assert.Equal([9, 1, 16, 1, 25, 81, 4, 36], new List<int>(source.Select(x => x * x).ToEnumerable(options)));
            Assert.Equal([3, 4, 5, 9, 6], new List<int>(source.Where(x => x > 2).ToEnumerable(options)));
        }

        [Fact]
        public void OverACountedChain_IsACollection()
        {
            var collection = Assert.IsAssignableFrom<ICollection<short>>(Enumerable.Sequence<short>(42, 51, 1).ToEnumerable());

            Assert.Equal(10, collection.Count);
            Assert.True(collection.IsReadOnly);
            Assert.True(collection.Contains(50));
            Assert.False(collection.Contains(52));

            var array = new short[12];
            collection.CopyTo(array, 2);
            Assert.Equal([0, 0, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51], array);

            Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(new short[11], 2));
            Assert.Throws<NotSupportedException>(() => collection.Add(1));
            Assert.Throws<NotSupportedException>(() => collection.Clear());
            Assert.Throws<NotSupportedException>(() => collection.Remove(42));

            var list = new List<short> { 1 };
            list.AddRange(collection);
            Assert.Equal([1, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51], list);
        }

        [Fact]
        public void OverAChainThatMustBeEnumeratedToCount_IsNotACollection()
        {
            IEnumerable<int> source = Data;

            Assert.False(source.Where(x => x > 2).ToEnumerable() is ICollection<int>);
            Assert.False(Iterate(Data).Select(x => x).ToEnumerable() is ICollection<int>);
        }

        [Fact]
        public void ACollection_CountsWithoutRunningTheSelector_AndCopiesRunningItOncePerElement()
        {
            var calls = 0;
            var collection = Assert.IsAssignableFrom<ICollection<int>>(new List<int>(Data).Select(x => { ++calls; return x * 2; }).ToEnumerable());

            Assert.Equal(Data.Length, collection.Count);
            Assert.Equal(0, calls);

            var copied = new int[Data.Length];
            collection.CopyTo(copied, 0);
            Assert.Equal([6, 2, 8, 2, 10, 18, 4, 12], copied);
            Assert.Equal(Data.Length, calls);
        }

        [Fact]
        public void OverAnArray_IsEnumeratedOverTheArray()
        {
            IEnumerable<int> source = Data;

            using var enumerator = source.Select(x => x + 1).ToEnumerable().GetEnumerator();

            Assert.Contains("SystemArray+Pull", enumerator.GetType().GetGenericArguments()[0].ToString());
        }

        private static IEnumerable<int> Iterate(int[] values)
        {
            foreach (var value in values)
                yield return value;
        }
    }
}
