using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // An element asked for by index, from the start or the end, is passed through Reverse, Skip and Take with its index
    // translated, and answered by the source without running the sequence; a from-end index with no known length is
    // found by buffering. A negative index throws, or gives the default, without starting the source.
    public class ElementAtRequests
    {
        private static readonly int[] Data = [10, 11, 12, 13, 14, 15, 16, 17, 18, 19];

        private static IEnumerable<int> Iterate()
        {
            foreach (var item in Data)
                yield return item;
        }

        [Fact]
        public void Reverse_MirrorsTheIndex()
        {
            Assert.Equal(19, Data.Reverse().ElementAt(0));
            Assert.Equal(17, Data.Reverse().ElementAt(2));
            Assert.Equal(10, Data.Reverse().ElementAt(^1));
            Assert.Equal(12, Data.Reverse().ElementAt(^3));
            Assert.Throws<ArgumentOutOfRangeException>(() => Data.Reverse().ElementAt(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => Data.Reverse().ElementAt(^0));
            Assert.Equal(0, Data.Reverse().ElementAtOrDefault(^11));
        }

        [Fact]
        public void SkipAndTake_TranslateTheIndex()
        {
            var window = Data.Skip(2).Take(5); // 12..16

            Assert.Equal(12, window.First());
            Assert.Equal(16, window.Last());
            Assert.Equal(14, window.ElementAt(2));
            Assert.Equal(15, window.ElementAt(^2));
            Assert.Throws<ArgumentOutOfRangeException>(() => window.ElementAt(5));
            Assert.Equal(0, window.ElementAtOrDefault(^6));
        }

        [Fact]
        public void NegativeIndex_StartsNoSource()
        {
            var started = 0;
            IEnumerable<int> Counted()
            {
                ++started;
                foreach (var item in Data)
                    yield return item;
            }

            Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => Counted().ElementAt(-1)).ParamName);
            Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => Counted().Select(x => x).ElementAt(-1)).ParamName);
            Assert.Equal(0, Counted().ElementAtOrDefault(-1));
            Assert.Equal(0, Counted().Select(x => x).ElementAtOrDefault(-1));
            Assert.Equal(0, started);
        }

        [Fact]
        public void FromEnd_WithNoKnownLength_IsBuffered()
        {
            Assert.Equal(19, Iterate().ElementAt(^1));
            Assert.Equal(13, Iterate().ElementAt(^7));
            Assert.Throws<ArgumentOutOfRangeException>(() => Iterate().ElementAt(^11));
            Assert.Throws<ArgumentOutOfRangeException>(() => Iterate().ElementAt(^0));
            Assert.Equal(0, Iterate().ElementAtOrDefault(^11));
            Assert.Equal(17, Iterate().Skip(2).ElementAt(^3));
        }

        [Fact]
        public void ListOutOfRange_IsDefault()
        {
            var list = new List<int>(Data);

            Assert.Equal(0, list.ElementAtOrDefault(-1));
            Assert.Equal(0, list.ElementAtOrDefault(10));
            Assert.Equal(19, list.ElementAtOrDefault(9));
        }
    }
}
