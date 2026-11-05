using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Concat run by push gives a downstream that has halted none of the parts after the one it halted in, whether it
    // halted on an element or on a span; both the two-part concat and a chain collected from concats over
    // IEnumerable<T> are covered.
    public class ConcatHalt
    {
        private static IEnumerable<int> Chain(params int[][] parts)
        {
            IEnumerable<int> chain = parts[0];
            for (var i = 1; i < parts.Length; ++i)
                chain = chain.Concat(parts[i]);
            return chain;
        }

        [Fact]
        public void TwoParts_HaltOnAnElement()
        {
            var seen = new List<int>();

            var first = new[] { 1, 2 }.OfArray().Concat(new[] { 3, 4 }.OfArray()).Select(x => { seen.Add(x); return x; }).First();

            Assert.Equal(1, first);
            Assert.Equal([1], seen);
        }

        [Fact]
        public void TwoParts_HaltOnASpan()
        {
            Assert.Equal(2, new[] { 1, 2 }.OfArray().Concat(new[] { 3, 4 }.OfArray()).ElementAt(1));
            Assert.Equal(1, new[] { 1, 2 }.OfArray().Concat(new[] { 3, 4 }.OfArray()).First());
        }

        [Fact]
        public void Chain_HaltOnAnElementInTheFirstPart()
        {
            var seen = new List<int>();

            var first = Chain([1, 2], [3, 4], [5, 6]).Select(x => { seen.Add(x); return x; }).First();

            Assert.Equal(1, first);
            Assert.Equal([1], seen);
        }

        [Fact]
        public void Chain_HaltOnAnElementInALaterPart()
        {
            var seen = new List<int>();

            var found = Chain([1, 2], [3, 4], [5, 6]).Select(x => { seen.Add(x); return x; }).First(x => x == 3);

            Assert.Equal(3, found);
            Assert.Equal([1, 2, 3], seen);
        }

        [Fact]
        public void Chain_HaltOnASpan()
        {
            Assert.Equal(2, Chain([1, 2], [3, 4], [5, 6]).ElementAt(1));
            Assert.Equal(4, Chain([1, 2], [3, 4], [5, 6]).ElementAt(3));
            Assert.Equal(1, Chain([1, 2], [3, 4], [5, 6]).First());
        }
    }
}
