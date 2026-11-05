using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // SequenceEqual of two sequences neither of which is a Cistern chain is System.Linq's, so collections of different
    // counts are unequal without a comparison; with a chain on either side Cistern compares them itself.
    public class SequenceEqualDispatch
    {
        private sealed class CountingComparer : IEqualityComparer<int>
        {
            public int Calls { get; private set; }

            public bool Equals(int x, int y)
            {
                ++Calls;
                return x == y;
            }

            public int GetHashCode(int x) => x;
        }

        [Fact]
        public void CollectionsOfDifferentCounts_AreUnequalWithoutAComparison()
        {
            var comparer = new CountingComparer();
            IEnumerable<int> first = new HashSet<int> { 1, 2, 3 };
            IEnumerable<int> second = new HashSet<int> { 1, 2 };

            Assert.False(first.SequenceEqual(second, comparer));
            Assert.False(new List<int> { 1, 2, 3 }.SequenceEqual(new Collection<int> { 1, 2 }, comparer));
            Assert.Equal(0, comparer.Calls);
        }

        [Fact]
        public void PlainSequences_CompareAsSystemLinq()
        {
            IEnumerable<int> first = new Collection<int> { 1, 2, 3 };

            Assert.True(first.SequenceEqual(new Collection<int> { 1, 2, 3 }));
            Assert.False(first.SequenceEqual(new Collection<int> { 1, 2, 4 }));
            Assert.True(new List<int> { 1, 2, 3 }.SequenceEqual(first));
        }

        [Fact]
        public void AChainOnEitherSide_IsComparedByCistern()
        {
            var calls = 0;
            IEnumerable<int> chain = new[] { 1, 2, 3 }.Select(x => { ++calls; return x; });
            IEnumerable<int> collection = new Collection<int> { 1, 2, 3 };

            Assert.True(chain.SequenceEqual(collection));
            Assert.True(collection.SequenceEqual(chain));
            Assert.True(new List<int> { 1, 2, 3 }.SequenceEqual(chain));
            Assert.False(chain.SequenceEqual(new Collection<int> { 1, 2 }));
            Assert.True(calls > 0);
        }
    }
}
