using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // First, Last and ElementAt of a Select find the element in the source and project only it, as System.Linq's
    // Select iterators do; over an ordering the element is selected rather than sorted.
    public class SelectedElements
    {
        private static IEnumerable<int> Iterate(int[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        [Fact]
        public void OverAnArray_ProjectsOnlyTheElement()
        {
            var values = new[] { 5, 3, 8, 1, 9 };
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(50, values.Select(selector).First());
            Assert.Equal(90, values.Select(selector).Last());
            Assert.Equal(80, values.Select(selector).ElementAt(2));
            Assert.Equal(10, values.Select(selector).ElementAtOrDefault(^2));
            Assert.Equal(0, values.Select(selector).ElementAtOrDefault(5));
            Assert.Equal(4, calls);
        }

        [Fact]
        public void OverAnOrdering_SelectsAndProjectsOnlyTheElement()
        {
            var values = new[] { 5, 3, 8, 1, 9, 1 };
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(10, Iterate(values).OrderBy(x => x).Select(selector).First());
            Assert.Equal(90, Iterate(values).OrderBy(x => x).Select(selector).Last());
            Assert.Equal(2, calls);
        }

        [Fact]
        public void EmptySource_ProjectsNothing()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x; };

            Assert.Equal(0, Array.Empty<int>().Select(selector).FirstOrDefault());
            Assert.Equal(0, Iterate([]).Select(selector).LastOrDefault());
            Assert.Equal(0, calls);
        }
    }
}
