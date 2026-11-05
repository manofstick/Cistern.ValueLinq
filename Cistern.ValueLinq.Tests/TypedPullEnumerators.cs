using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.ValueEnumerable;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A source's GetEnumerator hands foreach the container's own enumerator (each local below states the type it must
    // be), which yields the source's elements, leaves Current at default once the end is reached, and keeps the
    // collection's own checks (a List changed mid-enumeration throws).
    public class TypedPullEnumerators
    {
        private static readonly int[] Data = [3, 1, 4, 1, 5];

        private static List<T> Pull<T, TEnumerator>(ValueEnumerator<T, TEnumerator> enumerator)
            where TEnumerator : struct, IPullEnumerator<T>
        {
            var items = new List<T>();
            using (enumerator)
            {
                while (enumerator.MoveNext())
                    items.Add(enumerator.Current);
                Assert.Equal(default, enumerator.Current);
                Assert.False(enumerator.MoveNext());
            }
            return items;
        }

        [Fact]
        public void Array()
        {
            ValueEnumerator<int, SystemArray.Pull<int>> e = Data.OfArray().GetEnumerator();
            Assert.Equal(Data, Pull(e));
        }

        [Fact]
        public void EmptyArray()
        {
            ValueEnumerator<int, SystemArray.Pull<int>> e = System.Array.Empty<int>().OfArray().GetEnumerator();
            Assert.Empty(Pull(e));
        }

        [Fact]
        public void Memory()
        {
            ValueEnumerator<int, SystemMemory.Pull<int>> e = new ReadOnlyMemory<int>(Data, 1, 3).OfMemory().GetEnumerator();
            Assert.Equal([1, 4, 1], Pull(e));
        }

        [Fact]
        public void Span()
        {
            ValueEnumerator<int, SystemSpan.Pull<int[], int>> e = Enumerable.FromSpan(Data, static a => a.AsSpan(2)).GetEnumerator();
            Assert.Equal([4, 1, 5], Pull(e));
        }

        [Fact]
        public void List()
        {
            ValueEnumerator<int, SystemList.Pull<int>> e = new List<int>(Data).OfList().GetEnumerator();
            Assert.Equal(Data, Pull(e));
        }

        [Fact]
        public void ListByIndex()
        {
            ValueEnumerator<int, SystemListSkipTake.Pull<int>> e = new List<int>(Data).OfListByIndex().GetEnumerator();
            Assert.Equal(Data, Pull(e));
        }

        [Fact]
        public void ListByEnumerator()
        {
            ValueEnumerator<int, SystemEnumerableGeneric.NodeEnumerator<int, List<int>, List<int>.Enumerator>> e = new List<int>(Data).OfListByEnumerator().GetEnumerator();
            Assert.Equal(Data, Pull(e));
        }

        [Fact]
        public void Range()
        {
            ValueEnumerator<int, SystemRange.Pull> e = Enumerable.Range(5, 4).GetEnumerator();
            Assert.Equal([5, 6, 7, 8], Pull(e));
        }

        [Fact]
        public void Repeat()
        {
            ValueEnumerator<string, CisternRepeat.Pull<string>> e = Enumerable.Repeat("x", 3).GetEnumerator();
            Assert.Equal(["x", "x", "x"], Pull(e));
        }

        [Fact]
        public void Return()
        {
            ValueEnumerator<string, CisternReturn.Pull<string>> e = Enumerable.Return("x").GetEnumerator();
            Assert.Equal(["x"], Pull(e));
        }

        [Fact]
        public void Empty()
        {
            ValueEnumerator<string, CisternEmpty.Pull<string>> e = Enumerable.Empty<string>().GetEnumerator();
            Assert.Empty(Pull(e));
        }

        [Fact]
        public void List_ChangedDuringEnumeration_Throws()
        {
            var source = new List<int>(Data);
            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var item in source.OfList())
                    source.Add(item);
            });
        }

        [Fact]
        public void ListByEnumerator_ChangedDuringEnumeration_Throws()
        {
            var source = new List<int>(Data);
            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var item in source.OfListByEnumerator())
                    source.Add(item);
            });
        }
    }
}
