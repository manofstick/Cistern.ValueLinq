using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Pulled, an operator that can yield nothing whatever its source holds never creates the source's enumerator: a
    // take of nothing, a range empty at any length, and a skip past a source known to be no longer (so an OrderBy below
    // is never sorted).
    public class PullDeclinedSources
    {
        private sealed class NeverEnumerated : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator() => throw new InvalidOperationException();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private static List<int> Pull<TNode, TEnumerator>(ValueEnumerable<int, TNode, TEnumerator> source)
            where TNode : INode<int>
            where TEnumerator : struct, IPullEnumerator<int>, INodeEnumerator<TNode, TEnumerator>
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);
            return items;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void TakeNothing_DoesNotEnumerateSource(int count)
        {
            Assert.Empty(Pull(new NeverEnumerated().Take(count)));
        }

        [Fact]
        public void RangeEmptyAtAnyLength_DoesNotEnumerateSource()
        {
            Assert.Empty(Pull(new NeverEnumerated().Take(^0..)));
            Assert.Empty(Pull(new NeverEnumerated().Take(^2..^3)));
        }

        [Fact]
        public void SkipPastOrderedSource_DoesNotSort()
        {
            var calls = 0;
            int[] data = [5, 3, 1, 4, 2];

            Assert.Empty(Pull(data.OrderBy(x => { ++calls; return x; }).Skip(5)));
            Assert.Equal(0, calls);

            Assert.Equal([5], Pull(data.OrderBy(x => { ++calls; return x; }).Skip(4)));
            Assert.NotEqual(0, calls);
        }
    }
}
