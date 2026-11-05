using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Distinct and Union answer First, Any and (with no comparer given) Contains from their source with no set built;
    // an Except, which excludes something, and a Distinct given a comparer, answer as the enumeration does.
    public class DistinctUnionAnswers
    {
        private sealed class Source(int[] values)
        {
            public int Reads { get; private set; }

            public IEnumerable<int> Iterate()
            {
                foreach (var value in values)
                {
                    ++Reads;
                    yield return value;
                }
            }
        }

        [Fact]
        public void First_ReadsOneElement()
        {
            var source = new Source([3, 3, 1, 2]);

            Assert.Equal(3, source.Iterate().Distinct().First());
            Assert.True(source.Iterate().Distinct().Any());
            Assert.Equal(2, source.Reads);

            Assert.Equal(3, source.Iterate().Union([9]).First());
            Assert.Equal(3, source.Reads);

            Assert.Equal(9, new int[0].Union(source.Iterate()).Concat([9]).Last());
            Assert.Equal(9, new int[0].Union([9, 8]).First());
            Assert.Throws<InvalidOperationException>(() => new int[0].Distinct().First());
            Assert.False(new int[0].Union(new int[0]).Any());
        }

        [Fact]
        public void Except_IsNotAnsweredFromItsSource()
        {
            Assert.Equal(1, new[] { 3, 3, 1, 2 }.Except([3]).First());
            Assert.False(new[] { 3, 3 }.Except([3]).Any());
            Assert.False(new[] { 3, 1 }.Except([3]).Contains(3));
        }

        [Fact]
        public void Contains_UsesTheSourcesLookup_OnlyWithNoComparer()
        {
            IEnumerable<string> ignoringCase = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "a", "b" };

            Assert.True(ignoringCase.Distinct().Contains("B"));
            Assert.True(new[] { "x" }.Union(ignoringCase).Contains("A"));
            Assert.Equal(
                System.Linq.Enumerable.Contains(System.Linq.Enumerable.Distinct(ignoringCase, StringComparer.Ordinal), "B"),
                ignoringCase.Distinct(StringComparer.Ordinal).Contains("B"));
            Assert.True(new[] { 1, 2 }.Union([3, 4]).Contains(4));
            Assert.False(new[] { 1, 2 }.Union([3, 4]).Contains(5));
        }

        private sealed class Lookups(params int[] values) : ICollection<int>
        {
            public int Asked { get; private set; }

            public bool Contains(int item)
            {
                ++Asked;
                return Array.IndexOf(values, item) >= 0;
            }

            public int Count => values.Length;
            public bool IsReadOnly => true;
            public void Add(int item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Remove(int item) => throw new NotSupportedException();
            public void CopyTo(int[] array, int arrayIndex) => throw new NotSupportedException();
            public IEnumerator<int> GetEnumerator() => throw new NotSupportedException();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Fact]
        public void Contains_OfCollections_AsksEachInTurn_WithNoEnumeration()
        {
            var first = new Lookups(1, 2);
            var second = new Lookups(3, 4);

            Assert.True(first.Union(second).Contains(1));
            Assert.Equal((1, 0), (first.Asked, second.Asked));

            Assert.True(first.Union(second).Contains(4));
            Assert.Equal((2, 1), (first.Asked, second.Asked));

            Assert.False(first.Union(second).Contains(5));
            Assert.Equal((3, 2), (first.Asked, second.Asked));
        }
    }
}
