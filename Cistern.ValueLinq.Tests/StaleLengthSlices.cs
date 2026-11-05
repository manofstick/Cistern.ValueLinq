using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Concat records its parts' lengths when it is built, and a List<T> part can grow after that; a slice of the Concat
    // starting past the recorded length still holds the elements added since.
    public class StaleLengthSlices
    {
        private static (List<int>, List<int>) Parts() => (new List<int> { 1, 2 }, new List<int> { 3 });

        private static readonly int[] Expected = [5, 6];

        [Fact]
        public void SkipThenTake()
        {
            var (first, second) = Parts();
            var concat = first.Concat(second);
            second.AddRange([4, 5, 6, 7]);

            Assert.Equal(Expected, concat.Skip(4).Take(2).ToList());
        }

        [Fact]
        public void TakeThenSkip()
        {
            var (first, second) = Parts();
            var concat = first.Concat(second);
            second.AddRange([4, 5, 6, 7]);

            Assert.Equal(Expected, concat.Take(6).Skip(4).ToList());
        }

        [Fact]
        public void SkipThenTakeThroughIEnumerable()
        {
            var (first, second) = Parts();
            IEnumerable<int> concat = first.Concat(second);
            second.AddRange([4, 5, 6, 7]);

            Assert.Equal(Expected, concat.Skip(4).Take(2).ToList());
        }

        [Fact]
        public void Skip()
        {
            var (first, second) = Parts();
            var concat = first.Concat(second);
            second.AddRange([4, 5, 6]);

            Assert.Equal(Expected, concat.Skip(4).ToList());
            Assert.Equal(5, concat.Skip(4).First());
        }
    }
}
