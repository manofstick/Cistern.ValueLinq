using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Skip or Take over Repeat is answered as a shorter Repeat, so the skipped elements are never pushed.
    public class RepeatSlices
    {
        public static IEnumerable<object[]> Slices() =>
        [
            [5, 0, 3], [5, 2, 2], [5, 2, 10], [5, 5, 1], [5, 9, 1], [5, 1, 0], [0, 0, 1],
        ];

        private static int[] Expected(int count, int skip, int take)
        {
            var length = System.Math.Clamp(count - skip, 0, take);
            var expected = new int[length];
            System.Array.Fill(expected, 7);

            return expected;
        }

        [Theory]
        [MemberData(nameof(Slices))]
        public void SkipThenTake(int count, int skip, int take)
        {
            var slice = Enumerable.Repeat(7, count).Skip(skip).Take(take);

            Assert.Equal(Expected(count, skip, take), slice.ToArray());
            Assert.Equal(Expected(count, skip, take), slice.ToList());
            Assert.Equal(Expected(count, skip, take).Length, slice.Count());
            Assert.Equal(Expected(count, skip, take), Foreach(slice));
        }

        [Theory]
        [MemberData(nameof(Slices))]
        public void TakeThenSkip(int count, int skip, int take)
        {
            var slice = Enumerable.Repeat(7, count).Take(skip + take).Skip(skip);

            Assert.Equal(Expected(count, skip, take), slice.ToArray());
            Assert.Equal(Expected(count, skip, take), slice.ToList());
        }

        [Fact]
        public void SkipNearTheEndOfALongRepeat()
        {
            var slice = Enumerable.Repeat(7, int.MaxValue).Skip(int.MaxValue - 2);

            Assert.Equal([7, 7], slice.ToArray());
            Assert.Equal(7, slice.Last());
        }

        private static List<int> Foreach(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }
    }
}
