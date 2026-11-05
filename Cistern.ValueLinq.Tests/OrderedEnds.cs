using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // First and Last of an ordering, directly or through a Reverse, are found by one selection pass rather than a sort:
    // the earliest element with the least key, or the latest with the greatest, with ThenBy keys breaking ties.
    public class OrderedEnds
    {
        private readonly record struct Item(int Key, int Minor, int Id);

        private sealed class CountingComparer : IComparer<int>
        {
            public int Calls { get; private set; }
            public int Compare(int x, int y) { ++Calls; return x.CompareTo(y); }
        }

        // keys 0..4 cycle, so every key has several elements, in source order by Id
        private static Item[] Items(int count)
        {
            var items = new Item[count];
            for (var i = 0; i < count; ++i)
                items[i] = new Item(i % 5, (i * 7) % 3, i);
            return items;
        }

        private static IEnumerable<Item> Iterate(Item[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        [Fact]
        public void First_IsTheEarliestLeastKey()
        {
            var source = Iterate(Items(20));

            Assert.Equal(0, source.OrderBy(i => i.Key).First().Id);
            Assert.Equal(0, source.OrderBy(i => i.Key).FirstOrDefault().Id);
            Assert.Equal(4, source.OrderByDescending(i => i.Key).First().Id);
        }

        [Fact]
        public void Last_IsTheLatestGreatestKey()
        {
            var source = Iterate(Items(20));

            Assert.Equal(19, source.OrderBy(i => i.Key).Last().Id);
            Assert.Equal(19, source.OrderBy(i => i.Key).LastOrDefault().Id);
            Assert.Equal(15, source.OrderByDescending(i => i.Key).Last().Id);
        }

        [Fact]
        public void Reverse_SwapsTheEnds()
        {
            var source = Iterate(Items(20));

            Assert.Equal(19, source.OrderBy(i => i.Key).Reverse().First().Id);
            Assert.Equal(0, source.OrderBy(i => i.Key).Reverse().Last().Id);
            Assert.Equal(15, source.OrderByDescending(i => i.Key).Reverse().First().Id);
            Assert.Equal(4, source.OrderByDescending(i => i.Key).Reverse().Last().Id);
        }

        [Fact]
        public void ThenBy_BreaksTies()
        {
            var source = Iterate(Items(20));

            // key 0 holds Ids 0, 5, 10, 15 with Minor 0, 2, 1, 0; key 4 holds Ids 4, 9, 14, 19 with Minor 1, 0, 2, 1
            Assert.Equal(0, source.OrderBy(i => i.Key).ThenBy(i => i.Minor).First().Id);
            Assert.Equal(5, source.OrderBy(i => i.Key).ThenByDescending(i => i.Minor).First().Id);
            Assert.Equal(14, source.OrderBy(i => i.Key).ThenBy(i => i.Minor).Last().Id);
            Assert.Equal(9, source.OrderBy(i => i.Key).ThenByDescending(i => i.Minor).Last().Id);
            Assert.Equal(14, source.OrderBy(i => i.Key).ThenBy(i => i.Minor).Reverse().First().Id);
        }

        [Fact]
        public void Ends_DoNotSort()
        {
            const int count = 100;
            var source = Iterate(Items(count));

            var comparer = new CountingComparer();
            source.OrderBy(i => i.Key, comparer).First();
            Assert.Equal(count - 1, comparer.Calls);

            comparer = new CountingComparer();
            source.OrderBy(i => i.Key, comparer).Reverse().Last();
            Assert.Equal(count - 1, comparer.Calls);
        }

        [Fact]
        public void SpanSource_EndsInEachDirection()
        {
            var items = Items(20);

            Assert.Equal(0, items.OrderBy(i => i.Key).First().Id);
            Assert.Equal(4, items.OrderByDescending(i => i.Key).First().Id);
            Assert.Equal(19, items.OrderBy(i => i.Key).Last().Id);
            Assert.Equal(15, items.OrderByDescending(i => i.Key).Last().Id);
            Assert.Equal(19, items.OrderBy(i => i.Key).Reverse().First().Id);
            Assert.Equal(14, items.OrderBy(i => i.Key).ThenBy(i => i.Minor).Last().Id);
        }

        [Fact]
        public void SpanSource_WithAComparer_ComparesOncePerLaterElement()
        {
            const int count = 100;
            var items = Items(count);

            var comparer = new CountingComparer();
            Assert.Equal(0, items.OrderBy(i => i.Key, comparer).First().Id);
            Assert.Equal(count - 1, comparer.Calls);

            comparer = new CountingComparer();
            Assert.Equal(count - 1, items.OrderBy(i => i.Key, comparer).Last().Id);
            Assert.Equal(count - 1, comparer.Calls);

            comparer = new CountingComparer();
            Assert.Equal(4, items.OrderByDescending(i => i.Key, comparer).First().Id);
            Assert.Equal(95, items.OrderByDescending(i => i.Key, comparer).Last().Id);
            Assert.Equal(2 * (count - 1), comparer.Calls);
        }

        [Fact]
        public void SpanSource_ComputesEachKeyOnce_AndNoneForOneElement()
        {
            var calls = 0;
            Func<Item, int> key = i => { ++calls; return i.Key; };

            Assert.Equal(7, new[] { new Item(3, 0, 7) }.OrderBy(key).Last().Id);
            Assert.Equal(0, calls);

            Assert.Equal(19, Items(20).OrderBy(key).Last().Id);
            Assert.Equal(20, calls);
        }

        // the least key 0 is held by Ids 0, 5, 10 before the split and 15 after it, the greatest 4 by Ids 4, 9 and 14, 19
        [Fact]
        public void SpanSource_TiesAcrossBatchesAndElements()
        {
            var items = Items(20);
            var (head, tail) = (items[..12], items[12..]);

            Assert.Equal(0, head.Concat(tail).OrderBy(i => i.Key).First().Id);
            Assert.Equal(19, head.Concat(tail).OrderBy(i => i.Key).Last().Id);
            Assert.Equal(19, head.Concat(Iterate(tail)).OrderBy(i => i.Key).Last().Id);
            Assert.Equal(19, Iterate(head).Concat(tail).OrderBy(i => i.Key).Last().Id);
            Assert.Equal(19, Iterate(items[..1]).Concat(items[1..]).OrderBy(i => i.Key).Last().Id);
            Assert.Equal(0, Iterate(items[..1]).Concat(items[1..]).OrderBy(i => i.Key).First().Id);
        }

        [Fact]
        public void Empty()
        {
            var source = Iterate([]);

            Assert.Throws<InvalidOperationException>(() => source.OrderBy(i => i.Key).First());
            Assert.Throws<InvalidOperationException>(() => source.OrderBy(i => i.Key).Last());
            Assert.Throws<InvalidOperationException>(() => source.OrderBy(i => i.Key).Reverse().First());
            Assert.Equal(default, source.OrderBy(i => i.Key).FirstOrDefault());
            Assert.Equal(default, source.OrderBy(i => i.Key).Reverse().LastOrDefault());
        }

        [Fact]
        public void Range_OfOneElement_HasThatElementAtBothEnds()
        {
            Assert.Equal(5, Enumerable.Range(5, 1).First());
            Assert.Equal(5, Enumerable.Range(5, 1).Last());
            Assert.Equal(5, Enumerable.Range(5, 1).LastOrDefault());
            Assert.Equal(5, Enumerable.Range(5, 1).Reverse().First());
        }

        [Fact]
        public void Range_EmptyAtIntMinValue_HasNoEnds()
        {
            Assert.Throws<InvalidOperationException>(() => Enumerable.Range(int.MinValue, 0).First());
            Assert.Throws<InvalidOperationException>(() => Enumerable.Range(int.MinValue, 0).Last());
            Assert.Equal(0, Enumerable.Range(int.MinValue, 0).FirstOrDefault());
            Assert.Equal(0, Enumerable.Range(int.MinValue, 0).LastOrDefault());
        }
    }
}
