using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // The array and List<T> overloads written by hand check their source as System.Linq does, and Count checks its
    // predicate whatever the source holds.
    public class NullArguments
    {
        private static void ThrowsFor(string name, Action action) =>
            Assert.Equal(name, Assert.Throws<ArgumentNullException>(action).ParamName);

        [Fact]
        public void ArraySource()
        {
            int[] source = null!;

            ThrowsFor("source", () => source.Count());
            ThrowsFor("source", () => source.Any());
        }

        [Fact]
        public void ListSource()
        {
            List<int> source = null!;

            ThrowsFor("source", () => source.Count());
            ThrowsFor("source", () => source.Any());
            ThrowsFor("source", () => source.Last());
            ThrowsFor("source", () => source.LastOrDefault());
            ThrowsFor("source", () => source.ElementAt(0));
            ThrowsFor("source", () => source.ElementAtOrDefault(0));
        }

        [Fact]
        public void LastOfAnEmptyList() =>
            Assert.Throws<InvalidOperationException>(() => new List<int>().Last());

        [Fact]
        public void CountPredicate()
        {
            Func<int, bool> predicate = null!;

            ThrowsFor("predicate", () => new[] { 1, 2 }.Count(predicate));
            ThrowsFor("predicate", () => System.Array.Empty<int>().Count(predicate));
            ThrowsFor("predicate", () => new List<int> { 1 }.Count(predicate));
            ThrowsFor("predicate", () => Enumerable.Range(0, 3).Count(predicate));
            ThrowsFor("predicate", () => Iterate().Count(predicate));
        }

        // with both null, the source is named first, as System.Linq names it
        [Fact]
        public void SourceBeforePredicate()
        {
            Func<int, bool> predicate = null!;
            Func<int, int> key = null!;

            IEnumerable<int> enumerable = null!;
            ThrowsFor("source", () => enumerable.Count(predicate));
            ThrowsFor("source", () => enumerable.LongCount(predicate));
            ThrowsFor("source", () => enumerable.FirstOrDefault(predicate));
            ThrowsFor("source", () => enumerable.FirstOrDefault(predicate, 0));
            ThrowsFor("source", () => enumerable.SingleOrDefault(predicate));
            ThrowsFor("source", () => enumerable.SingleOrDefault(predicate, 0));
            ThrowsFor("source", () => enumerable.MinBy(key));
            ThrowsFor("source", () => enumerable.MaxBy(key));

            int[] array = null!;
            ThrowsFor("source", () => array.Count(predicate));
            ThrowsFor("source", () => array.LongCount(predicate));
            ThrowsFor("source", () => array.FirstOrDefault(predicate));
            ThrowsFor("source", () => array.SingleOrDefault(predicate));
            ThrowsFor("source", () => array.MinBy(key));
            ThrowsFor("source", () => array.MaxBy(key));

            List<int> list = null!;
            ThrowsFor("source", () => list.Count(predicate));
            ThrowsFor("source", () => list.LongCount(predicate));
            ThrowsFor("source", () => list.FirstOrDefault(predicate));
            ThrowsFor("source", () => list.SingleOrDefault(predicate));
            ThrowsFor("source", () => list.MinBy(key));
            ThrowsFor("source", () => list.MaxBy(key));
        }

        private static IEnumerable<int> Iterate()
        {
            yield return 1;
        }
    }
}
