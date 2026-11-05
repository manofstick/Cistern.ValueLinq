using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // An element of a tuple Zip whose first source answers by running user code (an OrderBy's selection) and whose
    // second does not is found by pulling the second to it, so the first's user code runs once.
    public class ZipElementAtUserCode
    {
        private sealed class Source
        {
            public int Enumerations { get; private set; }
            public int Reads { get; private set; }

            public IEnumerable<int> Iterate(int count)
            {
                ++Enumerations;
                for (var i = 0; i < count; ++i)
                {
                    ++Reads;
                    yield return i * 10;
                }
            }
        }

        private static readonly int[] Data = [5, 3, 9, 1, 7];

        private static readonly int[] Sorted = [1, 3, 5, 7, 9];

        public static IEnumerable<object[]> Indexes() =>
        [
            [0, true], [2, true], [3, true], [4, false], [9, false],
        ];

        [Theory]
        [MemberData(nameof(Indexes))]
        public void ElementAtOrDefault(int index, bool present)
        {
            var keys = 0;
            Func<int, int> key = x => { ++keys; return x; };
            var second = new Source();

            var element = Data.OrderBy(key).Zip(second.Iterate(4)).ElementAtOrDefault(index);

            // an index past the ordering's end is answered from its buffer's length, as System.Linq's OrderBy answers
            // it, with no key computed and the second sequence never read
            var withinFirst = index < Data.Length;
            Assert.Equal(present ? (Sorted[index], index * 10) : default, element);
            Assert.Equal(withinFirst ? Data.Length : 0, keys);
            Assert.Equal(withinFirst ? 1 : 0, second.Enumerations);
        }

        [Fact]
        public void ElementAt()
        {
            var keys = 0;
            Func<int, int> key = x => { ++keys; return x; };
            var second = new Source();

            var element = Data.OrderBy(key).Zip(second.Iterate(4)).ElementAt(2);

            Assert.Equal((5, 20), element);
            Assert.Equal(Data.Length, keys);
            Assert.Equal(3, second.Reads);
            Assert.Throws<ArgumentOutOfRangeException>(() => Data.OrderBy(key).Zip(second.Iterate(4)).ElementAt(4));
        }

        [Fact]
        public void First()
        {
            var keys = 0;
            Func<int, int> key = x => { ++keys; return x; };

            Assert.Equal((1, 0), Data.OrderBy(key).Zip(new Source().Iterate(4)).First());
            Assert.Equal(Data.Length, keys);
        }
    }
}
