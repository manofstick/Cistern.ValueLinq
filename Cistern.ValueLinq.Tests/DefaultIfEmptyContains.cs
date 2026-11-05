using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Contains over a DefaultIfEmpty: a value other than the default, or the default over a source with a count it can
    // read, is asked of the source in its place, so a set answers by its own lookup and comparer, as System.Linq's does;
    // any other is pushed, one pass over the source.
    public class DefaultIfEmptyContains
    {
        private static IEnumerable<T> Iterate<T>(T[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        [Fact]
        public void AnEmptySource_HoldsTheDefaultAlone()
        {
            Assert.True(new List<int>().DefaultIfEmpty(-1).Contains(-1));
            Assert.False(new List<int>().DefaultIfEmpty(-1).Contains(0));
            Assert.True(new HashSet<int>().DefaultIfEmpty().Contains(0));
            Assert.True(Iterate(new int[0]).DefaultIfEmpty(-1).Contains(-1));
            Assert.False(Iterate(new int[0]).DefaultIfEmpty(-1).Contains(0));
        }

        [Fact]
        public void ASourceWithElements_IsAskedInItsPlace()
        {
            Assert.True(new List<int> { 1, 2, 3 }.DefaultIfEmpty(-1).Contains(3));
            Assert.False(new List<int> { 1, 2, 3 }.DefaultIfEmpty(-1).Contains(-1));
            Assert.True(Iterate(new[] { 1, 2, 3 }).DefaultIfEmpty(-1).Contains(2));
            Assert.False(Iterate(new[] { 1, 2, 3 }).DefaultIfEmpty(-1).Contains(-1));
            Assert.True(Iterate(new[] { 1, -1, 3 }).DefaultIfEmpty(-1).Contains(-1));

            IEnumerable<string> ignoringCase = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "a", "b" };
            Assert.Equal(System.Linq.Enumerable.Contains(System.Linq.Enumerable.DefaultIfEmpty(ignoringCase, "x"), "B"), ignoringCase.DefaultIfEmpty("x").Contains("B"));
            Assert.Equal(System.Linq.Enumerable.Contains(System.Linq.Enumerable.DefaultIfEmpty(ignoringCase, "x"), "x"), ignoringCase.DefaultIfEmpty("x").Contains("x"));
        }
    }
}
