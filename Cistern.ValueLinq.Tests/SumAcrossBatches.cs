using System;
using Xunit;

using Cistern.ValueLinq.Maths;

namespace Cistern.ValueLinq.Tests
{
    // A Sum fed more than one span (a SelectMany's inner arrays, a Where's chunks) carries its running total across
    // them once; every span here is long enough to take the vector path.
    public class SumAcrossBatches : EnumerableTests
    {
        private static int[] Sequence(int start, int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = start + i;
            return items;
        }

        [Theory]
        [InlineData(SIMDOptions.OnlyIfSame)]
        [InlineData(SIMDOptions.WithOverflowHandling)]
        [InlineData(SIMDOptions.Fastest)]
        public void WhereThenSum_Int(SIMDOptions options)
        {
            var source = Sequence(0, 200);

            var sum = source.Where(x => x >= 0).Sum(options);

            Assert.Equal(19900, sum);
        }

        [Theory]
        [InlineData(SIMDOptions.OnlyIfSame)]
        [InlineData(SIMDOptions.WithOverflowHandling)]
        [InlineData(SIMDOptions.Fastest)]
        public void WhereThenSum_Long(SIMDOptions options)
        {
            var source = new long[200];
            for (var i = 0; i < source.Length; ++i)
                source[i] = i;

            var sum = source.Where(x => x >= 0).Sum(options);

            Assert.Equal(19900L, sum);
        }

        [Theory]
        [InlineData(SIMDOptions.OnlyIfSame)]
        [InlineData(SIMDOptions.WithOverflowHandling)]
        [InlineData(SIMDOptions.Fastest)]
        public void SelectManyThenSum_Int(SIMDOptions options)
        {
            var rows = new[] { Sequence(0, 64), Sequence(64, 64), Sequence(128, 64), Sequence(192, 8) };

            var sum = rows.SelectMany(row => row).Sum(options);

            Assert.Equal(19900, sum);
        }
    }
}
