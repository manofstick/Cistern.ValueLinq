using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Shuffle answers an element by one uniform draw (checked only against the length), a leading Take by a reservoir
    // sample, and Any and Contains from its source; draws are checked loosely for uniformity, with bounds generous enough
    // that a correct draw fails with negligible probability.
    public class ShuffleAnswers
    {
        private static IEnumerable<int> Iterate(int[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        private static int[] Values(int length)
        {
            var values = new int[length];
            for (var i = 0; i < length; ++i)
                values[i] = i;
            return values;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(7)]
        public void Elements_AreMembers_AndIndexesAreBounded(int length)
        {
            var values = Values(length);
            var list = new List<int>(values);

            for (var i = 0; i < length; ++i)
            {
                Assert.InRange(list.Shuffle().ElementAt(i), 0, length - 1);
                Assert.InRange(Iterate(values).Shuffle().ElementAt(^(i + 1)), 0, length - 1);
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => list.Shuffle().ElementAt(length));
            Assert.Throws<ArgumentOutOfRangeException>(() => Iterate(values).Shuffle().ElementAt(length));
            if (length == 0)
            {
                Assert.Throws<InvalidOperationException>(() => list.Shuffle().First());
                Assert.Throws<InvalidOperationException>(() => Iterate(values).Shuffle().Last());
                Assert.False(Iterate(values).Shuffle().Any());
            }
            else
            {
                Assert.True(Iterate(values).Shuffle().Any());
            }
        }

        [Fact]
        public void Draws_AreUniform()
        {
            const int Length = 10;
            const int Draws = 20_000;
            var values = Values(Length);
            var list = new List<int>(values);

            var fromList = new int[Length];
            var fromIterator = new int[Length];
            for (var i = 0; i < Draws; ++i)
            {
                ++fromList[list.Shuffle().First()];
                ++fromIterator[Iterate(values).Shuffle().Last()];
            }

            // each slot expects 2000; a correct draw falls outside 1700..2300 with probability far below one in a million
            foreach (var count in fromList)
                Assert.InRange(count, 1700, 2300);
            foreach (var count in fromIterator)
                Assert.InRange(count, 1700, 2300);
        }

        [Fact]
        public void LeadingTake_IsAUniformSampleOfDistinctElements()
        {
            const int Length = 20;
            const int Take = 5;
            const int Samples = 20_000;
            var values = Values(Length);

            var counts = new int[Length];
            for (var i = 0; i < Samples; ++i)
            {
                var sample = Iterate(values).Shuffle().Take(Take).ToArray();
                Assert.Equal(Take, sample.Length);
                Assert.Equal(Take, new HashSet<int>(sample).Count);
                foreach (var value in sample)
                    ++counts[value];
            }

            // each value expects 5000
            foreach (var count in counts)
                Assert.InRange(count, 4600, 5400);

            var all = Iterate(values).Shuffle().Take(Length + 5).ToList();
            all.Sort();
            Assert.Equal(values, all);
        }

        // a sample's Contains is a random boolean, as System.Linq's: certain when nothing or everything can be missed, and
        // otherwise true at the rate a uniform sample of that size holds a match
        [Fact]
        public void SampleContains_IsTheChanceOfAMatchBeingDrawn()
        {
            var values = Values(20);
            var withTwoOnes = (int[])values.Clone();
            withTwoOnes[7] = 1;

            for (var i = 0; i < 100; ++i)
            {
                Assert.False(values.Shuffle().Take(5).Contains(-1));
                Assert.True(Iterate(values).Shuffle().Take(20).Contains(3));
                Assert.True(new int[10].Shuffle().Take(3).Contains(0));
                Assert.True(withTwoOnes.Shuffle().Take(19).Contains(1));
            }

            // P(no 1 among 5 drawn from 20 holding two) = (18 * 17 * 16 * 15 * 14) / (20 * 19 * 18 * 17 * 16) = 210 / 380,
            // so 20000 trials expect 8947 true, with a standard deviation near 70
            var found = 0;
            for (var i = 0; i < 20_000; ++i)
            {
                if (withTwoOnes.Shuffle().Take(5).Contains(1))
                    ++found;
            }
            Assert.InRange(found, 8500, 9400);
        }

        [Fact]
        public void Contains_UsesTheSourcesLookup()
        {
            IEnumerable<string> ignoringCase = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "a", "b" };

            Assert.True(ignoringCase.Shuffle().Contains("B"));
            Assert.False(ignoringCase.Shuffle().Contains("x"));
        }
    }
}
