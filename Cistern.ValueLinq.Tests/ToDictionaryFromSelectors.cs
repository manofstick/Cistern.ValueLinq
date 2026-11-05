using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // ToDictionary with a key selector, and with a key and an element selector, over a Cistern chain.
    public class ToDictionaryFromSelectors
    {
        private static readonly IEqualityComparer<int> ByTens = EqualityComparer<int>.Create((x, y) => x % 10 == y % 10, x => x % 10);

        [Fact]
        public void KeySelector()
        {
            var dictionary = Enumerable.Range(0, 5).Select(x => x * 2).ToDictionary(x => x + 100);

            Assert.Equal(new Dictionary<int, int> { [100] = 0, [102] = 2, [104] = 4, [106] = 6, [108] = 8 }, dictionary);
        }

        [Fact]
        public void KeyAndElementSelectors()
        {
            var dictionary = Enumerable.Range(0, 3).Select(x => x * 2).ToDictionary(x => x, elementSelector: x => $"v{x}");

            Assert.Equal(new Dictionary<int, string> { [0] = "v0", [2] = "v2", [4] = "v4" }, dictionary);
        }

        [Fact]
        public void Comparer()
        {
            var dictionary = Enumerable.Range(0, 3).Select(x => x * 11).ToDictionary(x => x, comparer: ByTens);

            Assert.Same(ByTens, dictionary.Comparer);
            Assert.Equal(22, dictionary[2]);
            Assert.Throws<ArgumentException>(() => Enumerable.Range(0, 2).Select(x => x * 10).ToDictionary(x => x, ByTens));
        }

        [Fact]
        public void NullSelectors()
        {
            var source = Enumerable.Range(0, 3).Select(x => x);

            Assert.Equal("keySelector", Assert.Throws<ArgumentNullException>(() => source.ToDictionary((Func<int, int>)null!)).ParamName);
            Assert.Equal("elementSelector", Assert.Throws<ArgumentNullException>(() => source.ToDictionary(x => x, (Func<int, int>)null!)).ParamName);
        }
    }
}
