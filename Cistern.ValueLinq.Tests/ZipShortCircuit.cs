using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Zip run by push stops pulling both sides as soon as the operator after it has what it needs.
    public class ZipShortCircuit
    {
        private sealed class Counting
        {
            public int Pulled { get; private set; }

            public IEnumerable<int> Iterate(int count)
            {
                for (var i = 0; i < count; ++i)
                {
                    ++Pulled;
                    yield return i;
                }
            }
        }

        [Fact]
        public void First_PullsOneElement()
        {
            var first = new Counting();

            Assert.Equal((0, 10), first.Iterate(100).Zip(new[] { 10, 11, 12, 13 }).First());
            Assert.Equal(1, first.Pulled);
        }

        [Fact]
        public void Any_PullsOneElement()
        {
            var first = new Counting();

            Assert.True(first.Iterate(100).Zip(new[] { 10, 11, 12, 13 }).Any());
            Assert.Equal(1, first.Pulled);
        }
    }
}
