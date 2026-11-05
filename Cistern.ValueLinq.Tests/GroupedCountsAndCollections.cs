using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A GroupBy with a result selector counts its groupings without running the selector, as System.Linq's does; a
    // lookup is a read-only collection of its groupings, so it is counted and copied as one; and a grouping, a list,
    // is indexed in place by ElementAt.
    public class GroupedCountsAndCollections
    {
        private static IEnumerable<T> Iterate<T>(T[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        [Fact]
        public void GroupByWithAResultSelector_CountsWithoutRunningIt()
        {
            int[] items = [5, 1, 7, 2, 2, 9, 4, 1];
            var calls = 0;

            Assert.Equal(4, items.GroupBy(x => x % 4, (key, group) => { ++calls; return key; }).Count());
            Assert.Equal(4, Iterate(items).GroupBy(x => x % 4, x => x * 2, (key, group) => { ++calls; return key; }).Count());
            Assert.Equal(0, Iterate(new int[0]).GroupBy(x => x % 4, (key, group) => { ++calls; return key; }).Count());
            Assert.Equal(0, calls);

            Assert.Equal(4, items.GroupBy(x => x % 4, (key, group) => { ++calls; return key; }).ToList().Count);
            Assert.Equal(4, calls);
        }

        [Fact]
        public void ALookup_IsCountedAndCopiedAsACollection()
        {
            int[] items = [5, 1, 7, 2, 2, 9, 4, 1];
            IEnumerable<System.Linq.IGrouping<int, int>> lookup = items.ToLookup(x => x % 4);
            var expected = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(System.Linq.Enumerable.ToLookup(items, x => x % 4), g => g.Key));

            Assert.Equal(expected.Count, lookup.Count());
            Assert.Equal(expected, lookup.ToList().Select(g => g.Key).ToList());
            Assert.Equal(expected, lookup.ToArray().Select(g => g.Key).ToList());
            Assert.Equal(0, ((IEnumerable<System.Linq.IGrouping<int, int>>)new int[0].ToLookup(x => x)).ToList().Count);
        }

        [Fact]
        public void AGrouping_IsIndexedInPlace()
        {
            int[] items = [5, 1, 7, 2, 2, 9, 4, 1];
            IEnumerable<int> grouping = items.ToLookup(x => x % 4)[1];

            Assert.Equal(5, grouping.ElementAt(0));
            Assert.Equal(9, grouping.ElementAt(2));
            Assert.Equal(1, grouping.ElementAt(3));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => grouping.ElementAt(4));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => grouping.ElementAt(-1));
            Assert.Equal([5, 1, 9, 1], grouping.ToList());
        }
    }
}
