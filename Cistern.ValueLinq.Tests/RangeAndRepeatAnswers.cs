using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Range and Repeat answer Contains, ElementAt and their copies without enumerating: each answer matches the
    // enumerated sequence, at the bounds and either side of a vector's width.
    public class RangeAndRepeatAnswers
    {
        public static IEnumerable<object[]> Ranges()
        {
            foreach (var (start, count) in new[] { (0, 0), (5, 1), (-3, 7), (0, 8), (1, 9), (10, 17), (-100, 1000), (int.MaxValue - 9, 10), (int.MinValue, 0), (int.MinValue, 33) })
                yield return [start, count];
        }

        [Theory]
        [MemberData(nameof(Ranges))]
        public void Range_CopiesAndContains(int start, int count)
        {
            var expected = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Range(start, count));

            Assert.Equal(expected, Enumerable.Range(start, count).ToArray());
            Assert.Equal(expected, Enumerable.Range(start, count).ToList());

            foreach (var value in new[] { start - 1, start, start + count / 2, start + count - 1, start + count, int.MinValue, int.MaxValue })
                Assert.Equal(System.Linq.Enumerable.Contains(expected, value), Enumerable.Range(start, count).Contains(value));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void Repeat_ElementsAndContains(int count)
        {
            var repeat = Enumerable.Repeat(7, count);

            Assert.Equal(count > 0, repeat.Contains(7));
            Assert.False(repeat.Contains(8));

            if (count == 0)
            {
                Assert.Throws<InvalidOperationException>(() => Enumerable.Repeat(7, count).Last());
                Assert.Throws<ArgumentOutOfRangeException>(() => Enumerable.Repeat(7, count).ElementAt(0));
                Assert.Equal(0, Enumerable.Repeat(7, count).LastOrDefault());
            }
            else
            {
                Assert.Equal(7, Enumerable.Repeat(7, count).Last());
                Assert.Equal(7, Enumerable.Repeat(7, count).ElementAt(count - 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => Enumerable.Repeat(7, count).ElementAt(count));
            }
        }

        [Fact]
        public void Repeat_OfNull_CopiesNulls()
        {
            Assert.Equal(new string?[5], Enumerable.Repeat<string?>(null, 5).ToArray());
            Assert.Equal(new string?[5], Enumerable.Repeat<string?>(null, 5).ToList());
            Assert.Equal(new int?[5], Enumerable.Repeat<int?>(null, 5).ToArray());
            Assert.True(Enumerable.Repeat<string?>(null, 5).Contains(null));
        }

        // the element is compared once, value first, as the Contains aggregator compares
        [Fact]
        public void Repeat_Contains_ComparesOnce()
        {
            var calls = 0;
            var element = new Counted(() => ++calls);

            Assert.True(Enumerable.Repeat(element, 1000).Contains(element));
            Assert.Equal(1, calls);
        }

        private sealed class Counted(Action onEquals) : IEquatable<Counted>
        {
            public bool Equals(Counted? other)
            {
                onEquals();
                return ReferenceEquals(this, other);
            }

            public override bool Equals(object? obj) => Equals(obj as Counted);

            public override int GetHashCode() => 0;
        }
    }
}
