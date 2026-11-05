using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Skip or Take over an ordering orders only the window's elements; the window holds what a full stable sort puts
    // there, ties included, whichever keys, directions and comparers order it.
    public class OrderedWindows
    {
        private readonly record struct Item(int Key, int Minor, int Id);

        // keys with many ties, in no order, Ids in source order
        private static Item[] Items(int count)
        {
            var random = new Random(17);
            var items = new Item[count];
            for (var i = 0; i < count; ++i)
                items[i] = new Item(random.Next(20), random.Next(3), i);
            return items;
        }

        private static IEnumerable<Item> Iterate(Item[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        private sealed class ReversedComparer : IComparer<int>
        {
            public int Compare(int x, int y) => y.CompareTo(x);
        }

        public static IEnumerable<object[]> Windows()
        {
            foreach (var count in new[] { 1, 2, 7, 300, 1000 })
            {
                foreach (var (skip, take) in new[] { (0, 1), (0, 10), (1, 1), (3, 5), (100, 50), (count - 1, 1), (count - 3, 10), (count / 2, count), (count + 5, 3) })
                    yield return [count, Math.Max(0, skip), take];
            }
        }

        [Theory]
        [MemberData(nameof(Windows))]
        public void SkipTake_HoldsTheFullSortsElements(int count, int skip, int take)
        {
            var items = Items(count);

            Assert.Equal(
                System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(System.Linq.Enumerable.Skip(System.Linq.Enumerable.OrderBy(items, i => i.Key), skip), take)),
                items.OrderBy(i => i.Key).Skip(skip).Take(take).ToArray());
            Assert.Equal(
                System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(System.Linq.Enumerable.Skip(System.Linq.Enumerable.OrderByDescending(items, i => i.Key), skip), take)),
                Iterate(items).OrderByDescending(i => i.Key).Skip(skip).Take(take).ToArray());
            Assert.Equal(
                System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(System.Linq.Enumerable.Skip(System.Linq.Enumerable.ThenByDescending(System.Linq.Enumerable.OrderBy(items, i => i.Key), i => i.Minor), skip), take)),
                items.OrderBy(i => i.Key).ThenByDescending(i => i.Minor).Skip(skip).Take(take).ToList().ToArray());
            Assert.Equal(
                System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(System.Linq.Enumerable.Skip(System.Linq.Enumerable.OrderBy(items, i => i.Key, new ReversedComparer()), skip), take)),
                items.OrderBy(i => i.Key, new ReversedComparer()).Skip(skip).Take(take).ToArray());
        }

        [Theory]
        [MemberData(nameof(Windows))]
        public void TakeRange_HoldsTheFullSortsElements(int count, int skip, int take)
        {
            var items = Items(count);
            var range = skip..(skip + take);

            Assert.Equal(
                System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(System.Linq.Enumerable.OrderBy(items, i => i.Key), range)),
                items.OrderBy(i => i.Key).Take(range).ToArray());
        }

        [Fact]
        public void SkipTake_Enumerated_HoldsTheFullSortsElements()
        {
            var items = Items(1000);
            var window = new List<Item>();
            foreach (var item in items.OrderBy(i => i.Key).Skip(400).Take(25))
                window.Add(item);

            Assert.Equal(
                System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(System.Linq.Enumerable.Skip(System.Linq.Enumerable.OrderBy(items, i => i.Key), 400), 25)),
                window.ToArray());
        }
    }
}
