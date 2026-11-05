using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Count with a predicate on a ValueEnumerable counts the matching elements, as the IEnumerable<T>, array and List<T>
    // overloads do.
    public class CountPredicate
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(10, 5)]
        [InlineData(1001, 500)]
        public void CountsMatches(int length, int expected)
        {
            var source = new int[length];
            for (var i = 0; i < length; ++i)
                source[i] = i;

            int count = source.Select(x => x + 1).Count(x => x % 2 == 0);

            Assert.Equal(expected, count);
        }
    }
}
