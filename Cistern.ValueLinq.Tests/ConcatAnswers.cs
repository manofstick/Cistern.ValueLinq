using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Concat answers ElementAt, First, Last and Contains from its parts: a counted part is passed over whole, Last reads
    // an earlier part only when the later ones are empty, and Contains uses each part's own lookup.
    public class ConcatAnswers
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

        private static int[] Values(int start, int count)
        {
            var values = new int[count];
            for (var i = 0; i < count; ++i)
                values[i] = start + i;
            return values;
        }

        public static IEnumerable<object[]> Shapes() =>
        [
            [0, 0], [0, 3], [3, 0], [1, 1], [4, 5], [10, 2],
        ];

        [Theory]
        [MemberData(nameof(Shapes))]
        public void ElementAt_MatchesTheEnumeration(int first, int second)
        {
            // no element is 0, so 0 is the default of an index past the end
            var a = Values(1, first);
            var b = Values(100, second);
            var expected = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(a, b));
            var expectedThree = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(expected, a));

            for (var i = 0; i <= expectedThree.Length; ++i)
            {
                var element = i < expected.Length ? expected[i] : 0;
                Assert.Equal(element, new List<int>(a).Concat(new List<int>(b)).ElementAtOrDefault(i));
                Assert.Equal(element, new Source(a).Iterate().Concat(new List<int>(b)).ElementAtOrDefault(i));
                Assert.Equal(element, a.Concat(new Source(b).Iterate()).ElementAtOrDefault(i));
                Assert.Equal(i < expectedThree.Length ? expectedThree[i] : 0, a.Concat(b).Concat(a).ElementAtOrDefault(i));
            }

            if (expected.Length == 0)
            {
                Assert.Throws<InvalidOperationException>(() => a.Concat(b).First());
                Assert.Throws<InvalidOperationException>(() => new Source(a).Iterate().Concat(b).Last());
            }
            else
            {
                Assert.Equal(expected[0], new Source(a).Iterate().Concat(b).First());
                Assert.Equal(expected[^1], new Source(a).Iterate().Concat(new Source(b).Iterate()).Last());
                Assert.Equal(expected[^1], new List<int>(a).Concat(new List<int>(b)).Last());
            }
        }

        [Fact]
        public void Last_ReadsTheEarlierPartOnlyWhenTheLaterIsEmpty()
        {
            var start = new Source(Values(0, 10));
            Assert.Equal(104, start.Iterate().Concat(new List<int>(Values(100, 5))).Last());
            Assert.Equal(0, start.Reads);

            Assert.Equal(9, start.Iterate().Concat(new List<int>()).Last());
            Assert.Equal(10, start.Reads);
        }

        [Fact]
        public void ElementAt_PassesOverACountedPart()
        {
            var finish = new Source(Values(100, 5));
            Assert.Equal(102, new List<int>(Values(0, 10)).Concat(finish.Iterate()).ElementAt(12));
            Assert.Equal(3, finish.Reads);
        }

        [Fact]
        public void Contains_UsesEachPartsLookup()
        {
            var ignoringCase = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "a", "b" };
            IEnumerable<string> asEnumerable = ignoringCase;

            Assert.True(new[] { "x" }.Concat(asEnumerable).Contains("B"));
            Assert.True(asEnumerable.Concat(new[] { "x" }).Contains("A"));
            Assert.False(asEnumerable.Concat(new[] { "x" }).Contains("X"));
            Assert.True(asEnumerable.Concat(new[] { "x" }).Concat(new[] { "y" }).Contains("y"));
        }

        [Fact]
        public void Contains_TestsTheLastPartFirst()
        {
            var a = new Source([1, 2, 3]);
            var b = new Source([4, 5, 6]);
            var c = new Source([7, 8, 9]);

            Assert.True(a.Iterate().Concat(b.Iterate()).Concat(c.Iterate()).Contains(8));
            Assert.Equal((0, 0, 2), (a.Reads, b.Reads, c.Reads));

            Assert.True(a.Iterate().Concat(b.Iterate()).Concat(c.Iterate()).Contains(5));
            Assert.Equal((3, 2, 5), (a.Reads, b.Reads, c.Reads));
        }
    }
}
