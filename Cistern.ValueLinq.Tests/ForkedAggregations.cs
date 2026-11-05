using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Fork gives each of its aggregations the value it has alone, whether the first is pushed or answered by a request
    // (Last of a Where-Select over an array, found by a backward scan), with two aggregations or three.
    public class ForkedAggregations
    {
        private static readonly int[] Data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        private static IEnumerable<int> Iterate(int[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        [Fact]
        public void Two_OverAnArrayAndAnIterator()
        {
            Func<int, bool> isEven = x => x % 2 == 0;
            Func<int, long> widen = x => x;

            Assert.Equal((10L, 5), Data.Where(isEven).Select(widen).Fork(x => x.Last(), x => x.Count()));
            Assert.Equal((10L, 5), Iterate(Data).Where(isEven).Select(widen).Fork(x => x.Last(), x => x.Count()));
            Assert.Equal((5, 10L), Data.Where(isEven).Select(widen).Fork(x => x.Count(), x => x.Last()));
        }

        [Fact]
        public void Three_OverAnArrayAndAnIterator()
        {
            Func<int, bool> isEven = x => x % 2 == 0;
            Func<int, long> widen = x => x;

            Assert.Equal((10L, 5, 6.0), Data.Where(isEven).Select(widen).Fork(x => x.Last(), x => x.Count(), x => x.Average()));
            Assert.Equal((10L, 5, 6.0), Iterate(Data).Where(isEven).Select(widen).Fork(x => x.Last(), x => x.Count(), x => x.Average()));
            Assert.Equal((5, 10L, 2L), Data.Where(isEven).Select(widen).Fork(x => x.Count(), x => x.Last(), x => x.First()));
        }
    }
}
