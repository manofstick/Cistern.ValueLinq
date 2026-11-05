using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Reverse over a source that declines the mirrored index runs the source only to that element (its Last reads one
    // element), and answers Any and Contains from the source (a HashSet's own lookup), nothing reversed.
    public class ReverseAnswers
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

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void Elements_MatchTheReversal(int length)
        {
            var values = new int[length];
            for (var i = 0; i < length; ++i)
                values[i] = i * 3 + 1;
            var reversed = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Reverse(values));

            for (var i = 0; i <= length; ++i)
            {
                Assert.Equal(i < length ? reversed[i] : 0, new Source(values).Iterate().Reverse().ElementAtOrDefault(i));
                Assert.Equal(i < length ? reversed[^(i + 1)] : 0, new Source(values).Iterate().Reverse().ElementAtOrDefault(^(i + 1)));
            }

            if (length == 0)
            {
                Assert.Throws<InvalidOperationException>(() => new Source(values).Iterate().Reverse().Last());
                Assert.Throws<InvalidOperationException>(() => new Source(values).Iterate().Reverse().First());
                Assert.False(new Source(values).Iterate().Reverse().Any());
            }
            else
            {
                Assert.Equal(reversed[^1], new Source(values).Iterate().Reverse().Last());
                Assert.Equal(reversed[0], new Source(values).Iterate().Reverse().First());
                Assert.True(new Source(values).Iterate().Reverse().Any());
            }
        }

        [Fact]
        public void Last_ReadsOneElement()
        {
            var source = new Source([5, 6, 7, 8]);

            Assert.Equal(5, source.Iterate().Reverse().Last());
            Assert.Equal(1, source.Reads);

            Assert.Equal(7, source.Iterate().Reverse().ElementAt(^3));
            Assert.Equal(4, source.Reads);
        }

        // a window of the reversal is the source's mirrored slice, so each value is found exactly when the window holds it
        [Fact]
        public void WindowContains_MatchesTheWindow()
        {
            var values = new int[12];
            for (var i = 0; i < values.Length; ++i)
                values[i] = i;
            var list = new List<int>(values);

            foreach (var skip in new[] { 0, 1, 5, 11, 12, 20 })
            {
                foreach (var take in new[] { 1, 3, 12, 30 })
                {
                    var window = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(System.Linq.Enumerable.Skip(System.Linq.Enumerable.Reverse(values), skip), take));
                    foreach (var value in values)
                    {
                        var expected = Array.IndexOf(window, value) >= 0;
                        Assert.Equal(expected, values.Reverse().Skip(skip).Take(take).Contains(value));
                        Assert.Equal(expected, Cistern.ValueLinq.Enumerable.Reverse(list).Skip(skip).Take(take).Contains(value));
                        Assert.Equal(expected, new Source(values).Iterate().Reverse().Skip(skip).Take(take).Contains(value));
                    }
                }
            }
        }

        [Fact]
        public void Contains_UsesTheSourcesLookup()
        {
            IEnumerable<string> ignoringCase = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "a", "b" };

            Assert.True(ignoringCase.Reverse().Contains("B"));
            Assert.False(ignoringCase.Reverse().Contains("x"));
        }
    }
}
