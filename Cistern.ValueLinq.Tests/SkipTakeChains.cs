using System;
using System.Collections;
using System.Collections.Generic;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Skip over a Skip and a Take over a Take compose into one node, a Skip meeting a Take stacks, the pushed source
    // stops at the last element taken, and a slice that yields nothing never creates its source.
    public class SkipTakeChains
    {
        // a counted collection whose length is known to an SystemIEnumerable.Node, which reads it off the non-generic ICollection
        private sealed class Counted : ICollection<int>, ICollection
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
            public bool IsSynchronized => false;
            public object SyncRoot => this;
            public void Add(int item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Contains(int item) => throw new NotSupportedException();
            public void CopyTo(int[] array, int arrayIndex) => throw new NotSupportedException();
            public void CopyTo(Array array, int index) => throw new NotSupportedException();
            public bool Remove(int item) => throw new NotSupportedException();
        }

        private static int[] Range(int length)
        {
            var data = new int[length];
            for (var i = 0; i < length; ++i)
                data[i] = i;
            return data;
        }

        // each compiles only for a chain of exactly the named nodes over the array: a Skip over a Skip or a Take over a
        // Take that had stacked instead of composing would not convert to the parameter type, and nor would a Skip
        // meeting a Take as anything but the two stacked
        private static int[] OneSkipOverArray<T>(in ValueEnumerable<T, Skip.Node<T, SystemArray.Node<T>>, Skip.Pull<T, SystemArray.Node<T>, SystemArray.Pull<T>>> chain, Func<int[]> toArray)
            => toArray();

        private static int[] OneTakeOverArray<T>(in ValueEnumerable<T, Take.Node<T, SystemArray.Node<T>>, Take.Pull<T, SystemArray.Node<T>, SystemArray.Pull<T>>> chain, Func<int[]> toArray)
            => toArray();

        private static int[] TakeOverSkipOverArray<T>(in ValueEnumerable<T, Take.Node<T, Skip.Node<T, SystemArray.Node<T>>>, Take.Pull<T, Skip.Node<T, SystemArray.Node<T>>, Skip.Pull<T, SystemArray.Node<T>, SystemArray.Pull<T>>>> chain, Func<int[]> toArray)
            => toArray();

        private static int[] SkipOverTakeOverArray<T>(in ValueEnumerable<T, Skip.Node<T, Take.Node<T, SystemArray.Node<T>>>, Skip.Pull<T, Take.Node<T, SystemArray.Node<T>>, Take.Pull<T, SystemArray.Node<T>, SystemArray.Pull<T>>>> chain, Func<int[]> toArray)
            => toArray();

        [Fact]
        public void SkipAndTakeComposeWithTheirOwnKind()
        {
            var data = Range(100);

            Assert.Equal(new[] { 2, 3 }, OneSkipOverArray(data.OfArray().Skip(2), () => data.OfArray().Skip(2).Take(2).ToArray()));
            Assert.Equal(new[] { 5, 6 }, OneSkipOverArray(data.OfArray().Skip(2).Skip(3), () => data.OfArray().Skip(2).Skip(3).Take(2).ToArray()));
            Assert.Empty(OneSkipOverArray(data.OfArray().Skip(int.MaxValue).Skip(1), () => data.OfArray().Skip(int.MaxValue).Skip(1).ToArray()));
            Assert.Equal(new[] { 0, 1, 2 }, OneTakeOverArray(data.OfArray().Take(7).Take(3), () => data.OfArray().Take(7).Take(3).ToArray()));
            Assert.Equal(new[] { 0, 1, 2 }, OneTakeOverArray(data.OfArray().Take(3).Take(7), () => data.OfArray().Take(3).Take(7).ToArray()));
            Assert.Equal(new[] { 2, 3, 4, 5, 6, 7, 8 }, TakeOverSkipOverArray(data.OfArray().Skip(2).Take(7), () => data.OfArray().Skip(2).Take(7).ToArray()));
            Assert.Equal(new[] { 2, 3, 4, 5, 6, 7, 8 }, SkipOverTakeOverArray(data.OfArray().Take(9).Skip(2), () => data.OfArray().Take(9).Skip(2).ToArray()));
            Assert.Equal(new[] { 2, 3, 4, 5, 6 }, TakeOverSkipOverArray(data.OfArray().Skip(2).Take(7).Take(5), () => data.OfArray().Skip(2).Take(7).Take(5).ToArray()));
            Assert.Equal(new[] { 5, 6, 7, 8 }, data.OfArray().Skip(2).Take(7).Skip(3).ToArray());
            Assert.Equal(new[] { 5, 6 }, data.OfArray().Skip(2).Take(7).Skip(3).Take(2).ToArray());
            Assert.Empty(data.OfArray().Skip(2).Take(3).Skip(3).ToArray());
            Assert.Empty(SkipOverTakeOverArray(data.OfArray().Take(3).Skip(5), () => data.OfArray().Take(3).Skip(5).ToArray()));
            Assert.Equal(new[] { 5, 6, 7, 8 }, new List<int>(data.OfArray().Skip(2).Take(7).Skip(3)));
        }

        [Fact]
        public void PushStopsAtTheLastElementTaken()
        {
            var data = Range(100);
            var produced = 0;
            Func<int, int> counting = x => { ++produced; return x; };

            Assert.Equal(0 + 1 + 2, data.OfArray().Select(counting).Take(3).Sum());
            Assert.Equal(3, produced);

            produced = 0;
            Assert.Equal(2 + 3 + 4, data.OfArray().Select(counting).Skip(2).Take(3).Sum());
            Assert.Equal(3, produced);

            produced = 0;
            Assert.Equal(2 + 3 + 4, data.OfArray().Select(counting).Skip(2).Take(3).ToList().Count == 3 ? 9 : -1);
            Assert.Equal(3, produced);

            produced = 0;
            var pulled = 0;
            foreach (var x in data.OfArray().Select(counting).Skip(2).Take(3))
                pulled += x;
            Assert.Equal(9, pulled);
            Assert.Equal(5, produced);
        }

        [Fact]
        public void ASliceYieldingNothingNeverCreatesTheSource()
        {
            var source = new Counted(1, 2, 3);
            IEnumerable<int> e = source;

            Assert.Empty(e.Take(0).ToArray());
            Assert.Equal(0, source.Enumerations);
            Assert.Empty(e.Skip(3).ToArray());
            Assert.Equal(0, source.Enumerations);
            Assert.Empty(e.Skip(1).Take(5).Skip(4).ToArray());
            Assert.Equal(0, source.Enumerations);
            Assert.Equal(0, e.Take(0).Count());
            Assert.Equal(0, source.Enumerations);
            foreach (var _ in e.Skip(5))
                Assert.Fail("yielded");
            Assert.Equal(0, source.Enumerations);

            Assert.Equal(new[] { 2, 3 }, e.Skip(1).Take(5).ToArray());
            Assert.Equal(1, source.Enumerations);
        }

        [Fact]
        public void ContainersAnswerTheSliceWithoutRunning()
        {
            var data = Range(100);
            var list = new List<int>(data);

            Assert.Equal(new[] { 10, 11, 12 }, data.Skip(10).Take(3).ToArray());
            Assert.Equal(new[] { 10, 11, 12 }, list.Skip(10).Take(3).ToArray());
            Assert.Equal(new[] { 10, 11, 12 }, new ReadOnlyMemory<int>(data).Skip(10).Take(3).ToArray());
            Assert.Equal(new[] { 89, 88, 87 }, data.OfArray().Reverse().Skip(10).Take(3).ToArray());
            Assert.Equal(new[] { 89, 88, 87 }, list.OfList().Reverse().Skip(10).Take(3).ToArray());
            Assert.Equal(new[] { 10, 11, 12 }, data.OfArray().OrderBy(x => x).Skip(10).Take(3).ToArray());
            Assert.Equal(new[] { 97, 98, 99 }, data.OfArray().Skip(97).Take(10).ToArray());
            Assert.Equal(new[] { 97, 98, 99 }, data.OfArray().Take(5..^0).Skip(92).ToArray());
            Assert.Equal(new[] { 0, 1, 2 }, data.OfArray().Append(100).Skip(0).Take(3).ToArray());
            Assert.Equal(new[] { 98, 99, 100 }, data.OfArray().Append(100).Skip(98).Take(5).ToArray());
            Assert.Equal(new[] { 98, 99 }, data.OfArray().Append(100).Skip(98).Take(2).ToArray());
        }
    }
}
