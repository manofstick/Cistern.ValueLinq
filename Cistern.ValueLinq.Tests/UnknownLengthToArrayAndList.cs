using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // ToArray and ToList of a sequence whose length is not known gather it on the push path in doubling segments, then
    // copy it into one exactly sized result. Lengths either side of the segment boundaries are taken element by element
    // (an iterator), as a forwarder's chunks (a Where over an array) and as spans that straddle the boundaries (a
    // SelectMany over three-element arrays).
    public class UnknownLengthToArrayAndList
    {
        public static IEnumerable<object[]> Lengths() =>
            [[0], [1], [15], [16], [17], [31], [32], [33], [47], [48], [49], [64], [100], [1000]];

        private static int[] Expected(int length)
        {
            var expected = new int[length];
            for (var i = 0; i < length; ++i)
                expected[i] = i;
            return expected;
        }

        private static IEnumerable<int> Iterate(int length)
        {
            for (var i = 0; i < length; ++i)
                yield return i;
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void Iterator(int length)
        {
            Assert.Equal(Expected(length), Iterate(length).Select(x => x).ToArray());
            Assert.Equal(Expected(length), Iterate(length).Select(x => x).ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void WhereOverArray(int length)
        {
            var source = Expected(length);
            Assert.Equal(Expected(length), source.Where(x => x >= 0).ToArray());
            Assert.Equal(Expected(length), source.Where(x => x >= 0).ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void SelectManyOfArrays(int length)
        {
            var expected = Expected(length * 3);
            var outer = Expected(length);
            Assert.Equal(expected, outer.SelectMany(i => new[] { i * 3, i * 3 + 1, i * 3 + 2 }).ToArray());
            Assert.Equal(expected, outer.SelectMany(i => new[] { i * 3, i * 3 + 1, i * 3 + 2 }).ToList());
        }
    }
}
