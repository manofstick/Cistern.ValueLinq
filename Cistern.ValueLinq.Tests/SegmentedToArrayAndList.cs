using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // ToArray and ToList of a sequence of unknown length gather it in a scratch buffer of 8 and then pooled segments of
    // 16, 32, 64, ...: every length either side of a boundary comes out whole and in order, of values and of references.
    public class SegmentedToArrayAndList
    {
        private static IEnumerable<T> Iterate<T>(T[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        public static IEnumerable<object[]> Lengths()
        {
            foreach (var length in new[] { 0, 1, 7, 8, 9, 23, 24, 25, 55, 56, 57, 119, 120, 121, 1000, 100_000 })
                yield return [length];
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void Values(int length)
        {
            var values = new int[length];
            for (var i = 0; i < length; ++i)
                values[i] = i * 7;

            Assert.Equal(values, Iterate(values).ToArray());
            Assert.Equal(values, Iterate(values).ToList());
            Assert.Equal(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(values, x => x % 3 == 0)), values.Where(x => x % 3 == 0).Select(x => x).ToList().ToArray());
            Assert.Equal(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(values, x => x % 3 == 0)), Iterate(values).Where(x => x % 3 == 0).ToArray());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void References(int length)
        {
            var values = new string[length];
            for (var i = 0; i < length; ++i)
                values[i] = i.ToString();

            Assert.Equal(values, Iterate(values).ToArray());
            Assert.Equal(values, Iterate(values).ToList());
        }

        [Fact]
        public void AThrowingSource_Throws_AndTheNextRunIsWhole()
        {
            IEnumerable<int> throwsAt(int count)
            {
                for (var i = 0; i < count; ++i)
                    yield return i;
                throw new InvalidOperationException();
            }

            Assert.Throws<InvalidOperationException>(() => throwsAt(100).ToArray());
            Assert.Throws<InvalidOperationException>(() => throwsAt(100).ToList());
            Assert.Equal(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Range(0, 100)), Iterate(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Range(0, 100))).ToArray());
        }
    }
}
