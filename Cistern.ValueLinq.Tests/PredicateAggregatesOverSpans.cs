using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // First and Single with a predicate, and Aggregate with or without a seed, run an array's or list's span whole, and
    // a Where's surviving chunks one after another, with the answers and predicate calls of the element loop over an
    // iterator.
    public class PredicateAggregatesOverSpans
    {
        private static IEnumerable<T> Iterate<T>(T[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        private static int[] Values(int count)
        {
            var values = new int[count];
            for (var i = 0; i < count; ++i)
                values[i] = i;
            return values;
        }

        [Fact]
        public void First()
        {
            var values = Values(1000);
            var calls = 0;
            Func<int, bool> at = x => { ++calls; return x == 700; };

            Assert.Equal(700, values.First(at));
            Assert.Equal(701, calls);
            Assert.Equal(700, new List<int>(values).First(at));
            Assert.Equal(700, values.Where(x => x % 2 == 0).First(at));
            Assert.Throws<InvalidOperationException>(() => values.First(x => x < 0));
            Assert.Throws<InvalidOperationException>(() => new List<int>(values).Where(x => x % 2 == 0).First(x => x == 701));
        }

        [Fact]
        public void Single()
        {
            var values = Values(1000);

            Assert.Equal(700, values.Single(x => x == 700));
            Assert.Equal(700, new List<int>(values).Single(x => x == 700));
            Assert.Throws<InvalidOperationException>(() => values.Single(x => x < 0));

            // the second match in a later chunk of the Where than the first
            Assert.Throws<InvalidOperationException>(() => values.Where(x => x % 2 == 0).Single(x => x == 2 || x == 998));

            var calls = 0;
            Assert.Throws<InvalidOperationException>(() => values.Single(x => { ++calls; return x == 10 || x == 20; }));
            Assert.Equal(21, calls);
        }

        [Fact]
        public void Aggregate()
        {
            var values = Values(1000);
            var expected = Iterate(values).Aggregate((a, x) => a * 31 + x);

            Assert.Equal(expected, values.Aggregate((a, x) => a * 31 + x));
            Assert.Equal(expected, new List<int>(values).Aggregate((a, x) => a * 31 + x));
            Assert.Equal(Iterate(values).Where(x => x % 3 != 0).Aggregate((a, x) => a * 31 + x), values.Where(x => x % 3 != 0).Aggregate((a, x) => a * 31 + x));
            Assert.Equal(5, new[] { 5 }.Aggregate((a, x) => a * 31 + x));
            Assert.Throws<InvalidOperationException>(() => new int[0].Aggregate((a, x) => a + x));
        }

        [Fact]
        public void AggregateWithASeed()
        {
            var values = Values(1000);
            var expected = Iterate(values).Aggregate(7L, (a, x) => a * 31 + x);

            Assert.Equal(expected, values.Aggregate(7L, (a, x) => a * 31 + x));
            Assert.Equal(expected, new List<int>(values).Aggregate(7L, (a, x) => a * 31 + x));
            Assert.Equal(Iterate(values).Where(x => x % 3 != 0).Aggregate(7L, (a, x) => a * 31 + x), values.Where(x => x % 3 != 0).Aggregate(7L, (a, x) => a * 31 + x));
            Assert.Equal(7L, new int[0].Aggregate(7L, (a, x) => a * 31 + x));
        }
    }
}
