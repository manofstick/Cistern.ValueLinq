using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // An IList<T>, an IReadOnlyList<T>, a range, a repeat and a single element answer Reverse with themselves read from
    // the end, so a Reverse over one buffers nothing and reads only the elements its result is made of.
    public class IndexedReversals
    {
        private sealed class ReadOnlyList(int[] values) : IReadOnlyList<int>
        {
            public int Reads { get; private set; }

            public int this[int index] { get { ++Reads; return values[index]; } }

            public int Count => values.Length;

            public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)values).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private static int[] Values(int length)
        {
            var values = new int[length];
            for (var i = 0; i < length; ++i)
                values[i] = i * 3 + 1;
            return values;
        }

        private static int[] Reversed(IEnumerable<int> values) => System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Reverse(values));

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [10]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void IList_IsTheReversal(int length) => Check(Reversed(Values(length)), () => new Collection<int>(Values(length)));

        [Theory]
        [MemberData(nameof(Lengths))]
        public void IReadOnlyList_IsTheReversal(int length) => Check(Reversed(Values(length)), () => new ReadOnlyList(Values(length)));

        private static void Check(int[] expected, Func<IEnumerable<int>> source)
        {
            var n = expected.Length;

            Assert.Equal(expected, source().Reverse().ToArray());
            Assert.Equal(expected, source().Reverse().ToList());
            Assert.Equal(expected, source().Reverse().Select(x => x).ToArray());
            Assert.Equal(System.Linq.Enumerable.Sum(expected), source().Reverse().Sum());
            Assert.Equal(n, source().Reverse().Count());
            Assert.Equal(Reversed(expected), source().Reverse().Reverse().Select(x => x).ToArray());
            Assert.Equal(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(System.Linq.Enumerable.Skip(expected, 1), 3)), source().Reverse().Skip(1).Take(3).Select(x => x).ToArray());

            for (var i = 0; i < n; ++i)
            {
                Assert.Equal(expected[i], source().Reverse().ElementAt(i));
                Assert.Equal(expected[^(i + 1)], source().Reverse().ElementAt(^(i + 1)));
            }

            var pulled = new List<int>();
            foreach (var x in source().Reverse())
                pulled.Add(x);
            Assert.Equal(expected, pulled);
        }

        [Fact]
        public void IReadOnlyList_ReadsOnlyTheWindow()
        {
            var list = new ReadOnlyList(Values(10));

            Assert.Equal(new[] { 25, 22 }, ((IEnumerable<int>)list).Reverse().Skip(1).Take(2).Select(x => x).ToArray());
            Assert.Equal(2, list.Reads);
        }

        [Fact]
        public void IList_LastMatchScansFromTheEnd()
        {
            IEnumerable<int> list = new Collection<int>(Values(10));
            var called = new List<int>();

            Assert.Equal(19, list.Last(x => { called.Add(x); return x < 20; }));
            Assert.Equal(new[] { 28, 25, 22, 19 }, called);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(5, 1)]
        [InlineData(-3, 7)]
        [InlineData(int.MaxValue, 1)]
        [InlineData(int.MinValue, 3)]
        [InlineData(int.MaxValue - 2, 3)]
        public void Range_IsTheReversal(int start, int count)
        {
            var expected = Reversed(System.Linq.Enumerable.Range(start, count));

            Assert.Equal(expected, Enumerable.Range(start, count).Reverse().ToArray());
            Assert.Equal(expected, Enumerable.Range(start, count).Reverse().ToList());
            Assert.Equal(expected, Enumerable.Range(start, count).Reverse().Select(x => x).ToArray());
            Assert.Equal(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Skip(expected, 1)), Enumerable.Range(start, count).Reverse().Skip(1).Select(x => x).ToArray());
            Assert.Equal(count, Enumerable.Range(start, count).Reverse().Count());
            if (count > 0)
            {
                Assert.Equal(expected[0], Enumerable.Range(start, count).Reverse().First());
                Assert.Equal(expected[^1], Enumerable.Range(start, count).Reverse().Last());
            }
        }

        [Fact]
        public void RepeatAndReturn_AreTheirOwnReversal()
        {
            Assert.Equal(new[] { 7, 7, 7 }, Enumerable.Repeat(7, 3).Reverse().Select(x => x).ToArray());
            Assert.Empty(Enumerable.Repeat(7, 0).Reverse().Select(x => x).ToArray());
            Assert.Equal(new[] { 7, 7 }, Enumerable.Repeat(7, 3).Reverse().Skip(1).Select(x => x).ToArray());
            Assert.Equal(new[] { 4 }, Enumerable.Return(4).Reverse().Select(x => x).ToArray());
            Assert.Equal(4, Enumerable.Return(4).Reverse().Last(x => x > 0));
        }
    }
}
