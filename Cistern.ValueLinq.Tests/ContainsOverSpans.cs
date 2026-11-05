using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Contains over an array, a list or a window of one searches the span whole, with the default comparer or the one
    // given, and gives the answer the element loop gives over an iterator.
    public class ContainsOverSpans
    {
        private static IEnumerable<T> Iterate<T>(T[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        private sealed class ModuloComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y) => x % 10 == y % 10;
            public int GetHashCode(int obj) => obj % 10;
        }

        [Fact]
        public void Values()
        {
            var values = new int[1000];
            for (var i = 0; i < values.Length; ++i)
                values[i] = i * 3;

            foreach (var probe in new[] { 0, 3, 2997, 1500, -1, 2998 })
            {
                var expected = probe >= 0 && probe % 3 == 0 && probe <= 2997;
                Assert.Equal(expected, values.Contains(probe));
                Assert.Equal(expected, new List<int>(values).Contains(probe));
                Assert.Equal(expected, Iterate(values).Contains(probe));
                Assert.Equal(expected, values.Contains(probe, null));
            }

            Assert.False(new List<int>(values).Skip(100).Take(100).Contains(3));
            Assert.True(new List<int>(values).Skip(100).Take(100).Contains(300));
            Assert.False(new List<int>(values).Skip(100).Take(100).Contains(600));
        }

        [Fact]
        public void WithAComparer()
        {
            var values = new[] { 11, 22, 33 };

            Assert.True(values.Contains(42, new ModuloComparer()));
            Assert.False(values.Contains(44, new ModuloComparer()));
            Assert.True(new List<int>(values).Contains(13, new ModuloComparer()));
        }

        [Fact]
        public void References()
        {
            var values = new[] { "a", null, "c" };

            Assert.True(values.Contains(null));
            Assert.True(values.Contains("c"));
            Assert.False(values.Contains("d"));
            Assert.True(values.Contains("C", StringComparer.OrdinalIgnoreCase));
        }
    }
}
