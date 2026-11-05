using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Skip over a Skip merges into one skip on an indexable source; the merged node is what a materialisation copies
    // from, so ToArray and ToList hold the elements past both skips. A range that resolves to a skip, as Take(..^n) and
    // SkipLast do over a source of known length, takes the same path.
    public class SkipOfSkip
    {
        private static readonly int[] Data = [1, 2, 3, 4, 5];

        public static IEnumerable<object[]> Skips() =>
        [
            [0, 0], [0, 1], [1, 0], [1, 1], [2, 1], [1, 3], [3, 3],
        ];

        private static int[] Expected(int first, int second) => Data[Math.Min(Data.Length, first + second)..];

        [Theory]
        [MemberData(nameof(Skips))]
        public void Array_ToArray(int first, int second) =>
            Assert.Equal(Expected(first, second), Data.Skip(first).Skip(second).ToArray());

        [Theory]
        [MemberData(nameof(Skips))]
        public void Array_ToList(int first, int second) =>
            Assert.Equal(Expected(first, second), Data.Skip(first).Skip(second).ToList());

        [Theory]
        [MemberData(nameof(Skips))]
        public void List_ToArray(int first, int second) =>
            Assert.Equal(Expected(first, second), new List<int>(Data).Skip(first).Skip(second).ToArray());

        [Theory]
        [MemberData(nameof(Skips))]
        public void Memory_ToArray(int first, int second) =>
            Assert.Equal(Expected(first, second), new ReadOnlyMemory<int>(Data).Skip(first).Skip(second).ToArray());

        [Fact]
        public void SkipThenTake_ToArray() =>
            Assert.Equal([3, 4], Data.Skip(1).Skip(1).Take(2).ToArray());

        [Fact]
        public void TakeFromEnd_OverSkip_ToArray() =>
            Assert.Equal([2, 3], Data.Skip(1).Take(..^2).ToArray());

        [Fact]
        public void SkipLast_OverSkip_ToArray() =>
            Assert.Equal([2, 3], new List<int>(Data).Skip(1).SkipLast(2).ToArray());
    }
}
