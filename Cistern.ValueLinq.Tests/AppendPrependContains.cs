using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Contains over an Append or Prepend compares the added elements first, reading nothing of the source when one
    // matches, and otherwise asks the source, whose own lookup (a HashSet's comparer) answers.
    public class AppendPrependContains
    {
        private sealed class Source(int[] values)
        {
            public int Reads { get; private set; }

            public IEnumerable<int> Iterate()
            {
                foreach (var value in values)
                {
                    ++Reads;
                    yield return value;
                }
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(9)]
        public void AnAddedElement_IsFoundWithoutReadingTheSource(int added)
        {
            var source = new Source([1, 2, 3]);
            for (var i = 0; i < added; ++i)
            {
                var appended = source.Iterate().Prepend(-1).Append(100);
                for (var j = 0; j < added; ++j)
                    appended = appended.Append(100 + j).Prepend(-1 - j);

                Assert.True(appended.Contains(100 + i));
                Assert.True(appended.Contains(-1 - i));
            }
            Assert.Equal(0, source.Reads);

            Assert.True(source.Iterate().Append(100).Contains(2));
            Assert.False(source.Iterate().Prepend(-1).Append(100).Contains(7));
            Assert.Equal(5, source.Reads);
        }

        [Fact]
        public void TheSourcesLookup_Answers()
        {
            IEnumerable<string> ignoringCase = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "a", "b" };

            Assert.True(ignoringCase.Append("x").Contains("B"));
            Assert.False(ignoringCase.Append("x").Contains("X"));
        }
    }
}
