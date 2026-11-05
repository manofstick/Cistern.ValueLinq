using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Take after a SelectMany keeps one count across every inner sequence: the chain below SelectMany is held once,
    // by value while the inner sequences expose spans and on the heap from the first one that does not, never
    // recreated per inner sequence.
    public class SelectManyThenTake
    {
        private static readonly int[][] Rows = [[1, 2, 3], [4, 5, 6], [7, 8, 9], [10, 11, 12]];

        private static IEnumerable<int> Iterate(int[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        private static IEnumerable<int>[] Inner(string shape) =>
            shape switch
            {
                "arrays" => Rows,
                "lists" => [new List<int>(Rows[0]), new List<int>(Rows[1]), new List<int>(Rows[2]), new List<int>(Rows[3])],
                "iterators" => [Iterate(Rows[0]), Iterate(Rows[1]), Iterate(Rows[2]), Iterate(Rows[3])],
                "mixed" => [Rows[0], Iterate(Rows[1]), new List<int>(Rows[2]), Iterate(Rows[3])],
                "arraysThenIterator" => [Rows[0], Rows[1], Iterate(Rows[2]), Rows[3]],
                _ => throw new ArgumentOutOfRangeException(nameof(shape)),
            };

        public static IEnumerable<object[]> Cases()
        {
            foreach (var shape in new[] { "arrays", "lists", "iterators", "mixed", "arraysThenIterator" })
                foreach (var count in new[] { 0, 2, 5, 7, 20 })
                    yield return [shape, count];
        }

        private static List<int> FirstOfAll(int count, Func<int, bool> predicate)
        {
            var result = new List<int>();
            foreach (var row in Rows)
                foreach (var item in row)
                    if (result.Count < count && predicate(item))
                        result.Add(item);
            return result;
        }

        private static int Total(List<int> items)
        {
            var total = 0;
            foreach (var item in items)
                total += item;
            return total;
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Aggregates(string shape, int count)
        {
            var expected = FirstOfAll(count, _ => true);

            Assert.Equal(Total(expected), Inner(shape).SelectMany(x => x).Take(count).Sum());
            Assert.Equal(expected, Inner(shape).SelectMany(x => x).Take(count).ToList());
            Assert.Equal(expected.Count, Inner(shape).SelectMany(x => x).Take(count).Count());
            Assert.Equal(expected.Count == 0 ? 0 : expected[^1], Inner(shape).SelectMany(x => x).Take(count).LastOrDefault());
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Foreach(string shape, int count)
        {
            var actual = new List<int>();
            foreach (var item in Inner(shape).SelectMany(x => x).Take(count))
                actual.Add(item);

            Assert.Equal(FirstOfAll(count, _ => true), actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void WhereThenTake(string shape, int count)
        {
            var expected = FirstOfAll(count, x => x % 2 == 1);

            Assert.Equal(Total(expected), Inner(shape).SelectMany(x => x).Where(x => x % 2 == 1).Take(count).Sum());
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void SelectThenTake(string shape, int count)
        {
            var expected = FirstOfAll(count, _ => true);

            Assert.Equal(Total(expected) * 10, Inner(shape).SelectMany(x => x).Select(x => x * 10).Take(count).Sum());
            Assert.Equal(Total(expected) * 10, Inner(shape).SelectMany(x => x).Take(count).Select(x => x * 10).Sum());
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void ResultSelectorThenTake(string shape, int count)
        {
            var expected = FirstOfAll(count, _ => true);

            Assert.Equal(expected, Inner(shape).SelectMany(x => x, (_, item) => item).Take(count).ToList());
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void SkipThenTake(string shape, int count)
        {
            var expected = FirstOfAll(count + 2, _ => true);
            expected.RemoveRange(0, Math.Min(2, expected.Count));

            Assert.Equal(expected, Inner(shape).SelectMany(x => x).Skip(2).Take(count).ToList());
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void NestedThenTake(string shape, int count)
        {
            var once = FirstOfAll(int.MaxValue, _ => true);
            var expected = new List<int>(once);
            expected.AddRange(once);
            if (count < expected.Count)
                expected.RemoveRange(count, expected.Count - count);

            Assert.Equal(expected, new[] { Inner(shape), Inner(shape) }.SelectMany(outer => outer.SelectMany(x => x)).Take(count).ToList());
        }
    }
}
