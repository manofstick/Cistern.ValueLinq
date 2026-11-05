using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // ToDictionary over a source whose count is known without enumerating sizes the dictionary to it, as System.Linq
    // sizes one from an ICollection<T>.
    public class DictionaryCapacity
    {
        private const int Count = 1000;

        private static int CapacityFor(int count) =>
            new Dictionary<int, int>(count).EnsureCapacity(0);

        [Fact]
        public void Collection()
        {
            IEnumerable<int> values = new HashSet<int>(Enumerable.Range(0, Count).ToArray());

            Assert.Equal(CapacityFor(Count), values.ToDictionary(x => x).EnsureCapacity(0));
            Assert.Equal(CapacityFor(Count), values.ToDictionary(x => x, x => -x).EnsureCapacity(0));
        }

        [Fact]
        public void ArrayAndChain()
        {
            var values = Enumerable.Range(0, Count).ToArray();
            var pairs = new List<KeyValuePair<int, int>>();
            foreach (var value in values)
                pairs.Add(new KeyValuePair<int, int>(value, -value));

            Assert.Equal(CapacityFor(Count), values.ToDictionary(x => x).EnsureCapacity(0));
            Assert.Equal(CapacityFor(Count), values.Select(x => x + 1).ToDictionary(x => x).EnsureCapacity(0));
            Assert.Equal(CapacityFor(Count), pairs.Select(x => x).ToDictionary().EnsureCapacity(0));
        }

        [Fact]
        public void UnknownCount()
        {
            static IEnumerable<int> iterate()
            {
                for (var i = 0; i < Count; ++i)
                    yield return i;
            }

            var dictionary = iterate().ToDictionary(x => x);

            Assert.Equal(Count, dictionary.Count);
            Assert.Equal(Count, iterate().Where(x => true).ToDictionary(x => x).Count);
        }

        [Fact]
        public void EmptyCollection()
        {
            IEnumerable<int> values = new List<int>();
            var comparer = EqualityComparer<int>.Create((x, y) => x % 10 == y % 10, x => x % 10);

            var keyed = values.ToDictionary(x => x, comparer);
            var elements = values.ToDictionary(x => x, x => -x);

            Assert.Empty(keyed);
            Assert.Same(comparer, keyed.Comparer);
            Assert.Empty(elements);
            Assert.Throws<ArgumentNullException>("keySelector", () => values.ToDictionary((Func<int, int>)null!));
            Assert.Throws<ArgumentNullException>("elementSelector", () => values.ToDictionary(x => x, (Func<int, int>)null!));
        }

        [Fact]
        public void DuplicateKeyStillThrows()
        {
            IEnumerable<int> values = new List<int> { 1, 2, 1 };

            Assert.Throws<ArgumentException>(() => values.ToDictionary(x => x));
        }
    }
}
